using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Script gérant le déplacement d'un joueur : course, saut, dash, etc.
 */
public class PlayerControllerScript : MonoBehaviour
{
    public const int STATE_IDLE = 0,       //Animation d'attente (immobile)
              STATE_RUN = 1,        //Animation de course
              STATE_DASH = 2,       //Dash
              STATE_AIMING = 3,     //Visée lors d'une attaque rangée
              STATE_GRAPPLE = 4,    //Utilisation du grappin
              STATE_JUMP = 5,       //Saut
              STATE_HIT = 6,        //Prise de dégâts
              STATE_DEAD = 7;       //Mort
    const bool FACE_RIGHT = true,   //Regard du Player vers la droite
               FACE_LEFT = false;   //Regard du Player vers la gauche

    public float RunningSpeed = 4,          //Vitesse de course
                 JumpStrength = 400,        //Force de saut
                 DashStrength = 1300,       //Force du dash
                 MsBetweenDashes = 2000,    //Temps minimum entre 2 dash (en millisecondes)
                 MsBeforeDashGravityRestored = 200, // Temps avant que la gravité soit restaurée lors d'un dash
                 HitAnimationMs = 1000,     // Temps durant lequel le joueur joue l'animation de prise de dégâts
                 HitInvincibilityMs = 3000, // Temps durant lequel le joueur est invincible après s'être pris des dégâts
                 MsBeforeDeathRespawn = 5000; // Temps avant le respawn du joueur après sa mort
    
    public int m_Lives = 3; // nombre de vies restantes
    
    public GameObject m_RespawnPoint;
    public PlayerGrapple GrapplingHook;

    private Animator m_PlayerAnimator;          //Animator de l'objet, utile pour changer les états d'animation
    private Rigidbody2D m_Body;                 //Rigidbody2D de l'objet, utile pour le saut
    private PlayerAttackScript m_AttackScript;  //Script gérant l'attaque
    private bool m_CurrentFacing = FACE_RIGHT,  //Direction courante du regard du Player
                 m_Grounded = true,             //Flag indiquant si le Player est au sol ou non
                 m_DoubleJumped = false;        //Flag indiquant si le Player a fait un double saut
    private int m_CurrentState = STATE_IDLE;    //Etat d'animation courant
    public PhotonView m_PhotonView;    		//Objet lié au Network
    private float m_LastDashTime = 0;           //Dernière fois que le dash a été activé (en millisecondes)
    private float m_LastHitTime = 0;            //Dernière fois que le joueur a été touché (pour l'invincibilité)
    private float m_GroundTimer = 0;            //Dernière fois que le joueur est entré en collision avec le sol (pour la détection de m_Grounded)
    private float m_DieRespawnTimer = 0;        //Début de l'animation de mort
    private Vector2 translation;
    
    private float originalGravityScale;
    
    private static List<GameObject> playerList = new List<GameObject>();
    
    public PhotonPlayer owner;
    private PhotonPlayer m_LastAttacker;
    private BoxCollider2D m_Collider;
    
    // particules
    public GameObject m_DashParticles;
    public GameObject m_JumpParticles;
    public GameObject m_HurtParticles;
    
    public bool m_WasRunning = false;   // utilisé pour tomber à la verticale
    public bool finalDeath = false;     // fin du personnage, qui ne doit plus être contrôlé par le joueur
    
    public PlayerGUI m_GUI;

    void Awake()
    {
        m_PlayerAnimator = GetComponent<Animator>();
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
        m_AttackScript = GetComponent<PlayerAttackScript>();
        m_Collider = GetComponent<BoxCollider2D>();
        
        // lie le gameobject au joueur
        foreach (var player in PhotonNetwork.playerList)
            if (player.ID == m_PhotonView.ownerId)
                owner = player;
        if (owner == null)
            Debug.Log("Couldn't find PhotonPlayer !");
        
        playerList.Add(this.gameObject);
        // nettoie la liste
        List<GameObject> playersToDelete = new List<GameObject>();
        foreach (var player in playerList)
            if (player == null)
                playersToDelete.Add(player);
        foreach (var player in playersToDelete)
            playerList.Remove(player);
        
        foreach (var player in playerList)
        {
            foreach (var collision1 in player.GetComponents<BoxCollider2D>())
            {
                if (collision1.isTrigger)
                    continue;
                foreach (var collision2 in this.GetComponents<BoxCollider2D>())
                {
                    if (collision2.isTrigger)
                        continue;
                    Physics2D.IgnoreCollision(collision1, collision2);
                }
            }
        }
        
        if (m_PhotonView.isMine)
            CameraFollowGameobject.target = gameObject;
    }

    void Update()
    {
        if (m_Grounded && Mathf.Abs(m_Body.velocity.y) > .1 &&
            (Time.realtimeSinceStartup * 1000 - m_GroundTimer) >= 100)
        {
            // le joueur est en train de tomber
            m_Grounded = false;
            _SendGroundInfos();
        }
        
        // restauration de la gravité du personnage après le dash
        if (m_CurrentState == STATE_DASH && (Time.realtimeSinceStartup * 1000 - m_LastDashTime) >= MsBeforeDashGravityRestored)
        {
            m_Body.gravityScale = originalGravityScale;
            m_Body.drag = 0;
            ChangeState(STATE_IDLE);
        }

        //Déplacement
        if (m_CurrentState == STATE_RUN)
        {
            translation = m_CurrentFacing ? Vector2.right : Vector2.left;
            m_WasRunning = true;
        }
        else
            translation = Vector2.zero;
        _Move(translation);
        
        if (m_WasRunning && m_CurrentState == STATE_IDLE)
        {
            // permet de tomber verticalement
            m_WasRunning = false;
            float direction = m_CurrentFacing ? 1 : -1;
            float newVelocX = m_Body.velocity.x - direction*RunningSpeed;
            if (newVelocX * m_Body.velocity.x < 0);
                newVelocX = 0;
            m_Body.velocity = new Vector2(newVelocX, m_Body.velocity.y);
        }
        
        
        // -------- tout ce qui vient après n'est exécuté que si le personnage est contrôlé par le joueur --------
        
        if (m_PhotonView.isMine == false)
            return;
        
        // gestion du respawn après mort
        if (m_CurrentState == STATE_DEAD && (Time.realtimeSinceStartup * 1000 - m_DieRespawnTimer) >= MsBeforeDeathRespawn && !finalDeath)
        {
            ChangeState(STATE_IDLE);
            m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_IDLE);
            m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "respawn");
            m_Body.transform.position = new Vector3(m_RespawnPoint.transform.position.x, m_RespawnPoint.transform.position.y, m_RespawnPoint.transform.position.z);
            m_Body.velocity = Vector2.zero;
            
            m_PhotonView.RPC("PhSetAlpha", PhotonTargets.Others, .3f);
            SetAlpha(.3f);
            StartCoroutine(RestoreAlpha(HitInvincibilityMs/1000f));
        }
        
        // restaure les contrôles après avoir été touché
        if (m_CurrentState == STATE_HIT && (Time.realtimeSinceStartup * 1000 - m_LastHitTime) > HitAnimationMs)
        {
            ChangeState(STATE_IDLE);
            m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_IDLE);
        }

        float horizontal = Input.GetAxisRaw("Horizontal");

        if (GrapplingHook.IsGrappling)
        {
            if (m_CurrentState != STATE_DEAD)
            {
                ChangeState(STATE_GRAPPLE);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_GRAPPLE);
            }
            else
                GrapplingHook.IsGrappling = false;
        }

        if (GrapplingHook.IsHooked)
        {
            m_Body.AddForce(
                (transform.right.x > 0)
                ? GrapplingHook.RightProjection * GrapplingHook.GrappleProjectionStrength
                : GrapplingHook.LeftProjection * GrapplingHook.GrappleProjectionStrength
            );
        }

        //Gestion du saut
        if (Input.GetButtonDown("Jump") && m_CurrentState != STATE_DASH && m_CurrentState != STATE_DEAD && m_CurrentState != STATE_HIT)
        {
            //Si l'on est au sol ou qu'on a pas encore fait de double saut, on peut sauter
            if (m_Grounded || !m_DoubleJumped)
            {
                if (m_AttackScript.IsAiming())
                {
                    m_AttackScript.ExitAiming();
                    ChangeState(STATE_IDLE);
                    m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_IDLE);
                }
                m_DoubleJumped = !m_Grounded;
                if (!m_DoubleJumped)
                    m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "jump");
                else
                    m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "jumpdouble");
                _Jump();
                m_PhotonView.RPC("PhJump", PhotonTargets.Others);
            }
        }
        
        // Gestion du dash
        float dash = Input.GetAxisRaw("Dash");
        if (dash < 0 && m_CurrentState != STATE_DEAD && m_CurrentState != STATE_HIT && m_CurrentState != STATE_DASH)
        {
            if ((Time.realtimeSinceStartup * 1000 - m_LastDashTime) >= MsBetweenDashes)
            {
                // dash gauche
                if (m_AttackScript.IsAiming())
                {
                    m_AttackScript.ExitAiming();
                    ChangeState(STATE_IDLE);
                    m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_IDLE);
                }
                if (m_CurrentFacing)
                {
                    ChangeDirection(false);
                    m_PhotonView.RPC("PhChangeDirection", PhotonTargets.Others, false);
                }
                ChangeState(STATE_DASH);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_DASH);
                m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "dash");
            }
            //else
            //    m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "dashrecharge");
        }
        else if (dash > 0 && m_CurrentState != STATE_DEAD && m_CurrentState != STATE_HIT && m_CurrentState != STATE_DASH)
        {
            if ((Time.realtimeSinceStartup * 1000 - m_LastDashTime) >= MsBetweenDashes)
            {
                // dash droit
                if (m_AttackScript.IsAiming())
                {
                    m_AttackScript.ExitAiming();
                    ChangeState(STATE_IDLE);
                    m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_IDLE);
                }
                if (!m_CurrentFacing)
                {
                    ChangeDirection(true);
                    m_PhotonView.RPC("PhChangeDirection", PhotonTargets.Others, true);
                }
                ChangeState(STATE_DASH);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_DASH);
                m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "dash");
            }
            //else
            //    m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "dashrecharge");
        }

        //Gestion du déplacement horizontal
        else if (horizontal != 0 && m_CurrentState != STATE_DASH && m_CurrentState != STATE_AIMING && m_CurrentState != STATE_DEAD && m_CurrentState != STATE_HIT)
        {
            //Vérifier que le regard est dans la bonne direction
            if ((horizontal > 0) != m_CurrentFacing)
            {
                ChangeDirection(horizontal > 0);
                m_PhotonView.RPC("PhChangeDirection", PhotonTargets.Others, horizontal > 0);
            }

            //passer en animation de course
            if (m_CurrentState != STATE_RUN)
            {
                ChangeState(STATE_RUN);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_RUN);
            }
        }
        else
        {
            //passe en animation d'attente
            if (m_CurrentState != STATE_IDLE
                && m_CurrentState != STATE_DASH
                && m_CurrentState != STATE_DEAD
                && m_CurrentState != STATE_HIT
                && m_CurrentState != STATE_AIMING)
            {
                ChangeState(STATE_IDLE);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_IDLE);
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // analyse la collision afin de savoir si le joueur touche actuellement le sol
        foreach (ContactPoint2D contact in collision.contacts) {
            Vector2 norm = contact.normal;
            norm.Normalize();
            if (norm.y > .7)
            {
                m_Grounded = true;
                m_GroundTimer = Time.realtimeSinceStartup * 1000;
                _SendGroundInfos();
                if (m_PhotonView.isMine && (Time.realtimeSinceStartup * 1000 - m_GroundTimer) <= 100)
                    m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "hitground");
            }
        }
    }

    //Changer la direction de regard du Player
    public void ChangeDirection(bool Direction)
    {
        if (m_CurrentFacing != Direction)
        {
            transform.right *= -1;
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, -transform.localScale.z);
            m_CurrentFacing = Direction;
        }
    }

    //Changer l'état d'animation
    public void ChangeState(int State)
    {
        if (m_CurrentState != State)
        {
            m_PlayerAnimator.SetInteger("State", State);
            m_CurrentState = State;
        }
        switch (m_CurrentState)
        {
            case STATE_DASH:
                _Dash();
                break;
        }
    }

    //Donne la force au Rigidbody2D de sauter
    private void _Jump()
    {
        if (m_PhotonView.isMine)
        {
            m_Body.velocity = Vector2.zero;
            m_Body.AddForce(new Vector2(0, JumpStrength));
        }
        m_Grounded = false;
        ChangeState(STATE_JUMP);
        _SendGroundInfos();
        m_PlayerAnimator.SetTrigger("Jump");
        Instantiate(m_JumpParticles, transform.position, transform.rotation);
    }
    
    // Effectue un dash dans la direction visée par le joueur
    private void _Dash()
    {
        if (m_PhotonView.isMine)
        {
            m_Body.velocity = Vector2.zero;
            if (m_CurrentFacing)
                m_Body.AddForce(new Vector2(DashStrength, 0));
            else
                m_Body.AddForce(new Vector2(-DashStrength, 0));
            m_GUI.AnimateDash(MsBetweenDashes/1000f);
        }
        originalGravityScale = m_Body.gravityScale;
        m_Body.gravityScale = 0;
        m_Body.drag = 10;
        m_LastDashTime = Time.realtimeSinceStartup * 1000;
        m_PlayerAnimator.SetTrigger("Dash");
        // TODO - trouver pourquoi les particules de dash ne fonctionnent pas.
        //Instantiate(m_DashParticles, transform.position, transform.rotation);
        Instantiate(m_JumpParticles, transform.position, transform.rotation);
    }

    //Déplace le Player horizontalement
    private void _Move(Vector2 Translation)
    {
        float newVelocX = (Mathf.Abs(m_Body.velocity.x) > Mathf.Abs(translation.x*RunningSpeed)) ? m_Body.velocity.x : translation.x*RunningSpeed;
        m_Body.velocity = new Vector2(newVelocX, m_Body.velocity.y);
    }

    //Précise à l'Animator si le Player est au sol, et donne sa vitesse verticale
    private void _SendGroundInfos()
    {
        m_PlayerAnimator.SetBool("Grounded", m_Grounded);
        if (!m_Grounded && m_CurrentState != STATE_JUMP)
            m_PlayerAnimator.SetTrigger("Fall");
    }
    
    // Retourne vrai si le joueur est en mesure d'attaquer (sur le sol et immobile ou en train de courir)
    public bool CanAttack()
    {
        return m_Grounded && m_CurrentState != STATE_DASH && m_CurrentState != STATE_DEAD && m_CurrentState != STATE_HIT;
    }
    
    // Retourne la direction du joueur
    public bool GetCurrentFacing()
    {
        return m_CurrentFacing;
    }

    [PunRPC]
    void PhChangeDirection(bool Direction)
    {
        ChangeDirection(Direction);
    }

    [PunRPC]
    void PhChangeState(int State)
    {
        ChangeState(State);
    }
    [PunRPC]
    void PhJump()
    {
        _Jump();
    }
    
    // Retourne vrai si le joueur n'est pas dans son cooldown le protégeant des dégâts
    public bool canTakeDamage()
    {
        return m_CurrentState != STATE_DASH && m_CurrentState != STATE_DEAD && m_CurrentState != STATE_HIT && ((Time.realtimeSinceStartup * 1000 - m_LastHitTime) > HitInvincibilityMs);
    }
    [PunRPC]
    void PhTakeDamage(bool direction, PhotonPlayer attacker)
    {
        if ((Time.realtimeSinceStartup * 1000 - m_LastHitTime) <= HitInvincibilityMs)
            // pas de spam
            return;
        Debug.Log("Take damage");
        m_LastHitTime = Time.realtimeSinceStartup * 1000;
        
        if (attacker != owner)
            m_LastAttacker = attacker;
        
        m_Body.velocity = Vector2.zero;
        
        Instantiate(m_HurtParticles, transform.position, transform.rotation);
        
        if (m_CurrentState == STATE_DASH)
        {
            m_Body.gravityScale = originalGravityScale;
            m_Body.drag = 0;
        }
        
        m_Lives--;
        if (m_Lives == 0)
        {
            // game over
            if (m_PhotonView.isMine)
            {
                ChangeState(STATE_DEAD);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_DEAD);
                if (m_LastAttacker != null)
                {
                    m_LastAttacker.AddScore(1);
                    m_LastAttacker = null;
                }
                m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "die");
            }
            m_PlayerAnimator.SetTrigger("Die");
            m_Lives = 3;
            m_DieRespawnTimer = Time.realtimeSinceStartup * 1000;
        }
        
        else
        {
            if (direction)
                m_Body.AddForce(new Vector2(200, 500));
            else
                m_Body.AddForce(new Vector2(-200, 500));
            
            ChangeState(STATE_HIT);
            m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_HIT);
            m_PlayerAnimator.SetTrigger("TakeDamage");
            if (m_PhotonView.isMine)
                m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "takedamage");
            SetAlpha(.3f);
            StartCoroutine(RestoreAlpha(HitInvincibilityMs/1000f));
        }
    }

    public void dieFall()
    {
        if (m_CurrentState == STATE_DEAD)
            return;
        Debug.Log("Mort par chute");
        m_Lives--;
        if (m_Lives <= 0)
        {
            m_Lives = 3;
            
            // doublement mort ? On met DEUX FOIS le temps de respawn normal.
            m_DieRespawnTimer = Time.realtimeSinceStartup * 1000 + MsBeforeDeathRespawn;
        }
        else
            m_DieRespawnTimer = Time.realtimeSinceStartup * 1000;
        
        if (m_PhotonView.isMine)
        {
            if (m_LastAttacker != null)
            {
                m_LastAttacker.AddScore(1);
                m_LastAttacker = null;
            }
            ChangeState(STATE_DEAD);
            m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_DEAD);
        }
        m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "falldie");
    }

    [PunRPC]
    void PhSendDestruction()
    {
        Destroy(gameObject);
        RechercheLobby recherche = GetComponent<RechercheLobby>();
        recherche.instantiate();
    }
    
    [PunRPC]
    public void PhPlayerSpeaks(string emotion)
    {
        // appelé lorsque le joueur doit émettre un son.
        // à remplir. Les différents sons sont déjà spécifiés ci dessous.
        Debug.Log("Joue l'émotion : " + emotion);
        switch (emotion)
        {
            case "respawn":
                // respawn après être mort
                // "Hohoho !"
                break;
            case "fire":
                // tir d'une flèche
                // "Yah !"
                break;
            case "firespecial":
                // tir d'un poulet
                // "Muhaha !"
                break;
            case "die":
                // meurt (explosion/flèche)
                // "Nooooo !"
                break;
            case "falldie":
                // meurt (chute hors terrain)
                // "NOOooooo..."
                break;
            case "dash":
                // effectue un dash
                // "Yeeha !"
                break;
            case "jump":
                // saute
                // "Huh !"
                break;
            case "jumpdouble":
                // double saut
                // "Yaah !"
                break;
            case "aim":
                // vise avec l'arc
                // "Hmmm !"
                break;
            case "hitground":
                // tombe à terre
                // "ow"
                break;
            case "dashrecharge":
                // attente de la recharge du dash
                // pas utilisé pour le moment
                // "Naargh !"
                break;
            case "specialrecharge":
                // attente de la recharge de l'attaque spéciale
                // "Naargh !"
                break;
            case "killother":
                // a tué un adversaire
                // "Huhuhu" ou "Muhahaha !"
                break;
            case "takedamage":
                // se prend un coup
                // "Owww !"
                break;
            default:
                Debug.Log("ERREUR : émotion inconnue : " + emotion);
                break;
        }
    }
    
    // tue de manière permanente le joueur en fin de jeu
    public void DieFinal()
    {
        ChangeState(STATE_DEAD);
        m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_DEAD);
        m_PlayerAnimator.SetTrigger("Die");
        finalDeath = true;
    }
    
    [PunRPC]
    void PhSetAlpha(float alpha)
    {
        SetAlpha(.3f);
        StartCoroutine(RestoreAlpha(HitInvincibilityMs/1000f));
    }
    
    void SetAlpha(float alpha)
    {
        SpriteRenderer[] ChildrenRenderer = GetComponentsInChildren<SpriteRenderer>() as SpriteRenderer[];
        foreach (SpriteRenderer spRender in ChildrenRenderer)
            spRender.color = new Color(spRender.color.r, spRender.color.g, spRender.color.b, alpha);
    }
    
    IEnumerator RestoreAlpha(float time)
    {
        yield return new WaitForSeconds(time);
        SetAlpha(1f);
    }
}