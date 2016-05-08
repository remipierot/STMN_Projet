using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControllerScript : MonoBehaviour
{
    public const int STATE_IDLE = 0,       //Animation d'attente (immobile)
              STATE_RUN = 1,        //Animation de course
              STATE_DASH = 2,       //Dash
              STATE_AIMING = 3,     //Visée lors d'une attaque rangée
              STATE_GRAPPLE = 4,    //Utilisation du grappin
              STATE_JUMP = 5;       //Saut
    const bool FACE_RIGHT = true,   //Regard du Player vers la droite
               FACE_LEFT = false;   //Regard du Player vers la gauche

    public float RunningSpeed = 4,          //Vitesse de course
                 JumpStrength = 400,        //Force de saut
                 DashStrength = 1300,       //Force du dash
                 MsBetweenDashes = 2000,    //Temps minimum entre 2 dash (en millisecondes)
                 MsBeforeDashGravityRestored = 200, // Temps avant que la gravité soit restaurée lors d'un dash
                 HitInvincibilityMs = 2000; // Temps durant lequel le joueur est invincible après s'être pris des dégâts
    
    public int m_Lives = 3; // nombre de vies restantes
    
    public BoxCollider2D m_Box;
    
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
    private Vector2 translation;
    
    private float originalGravityScale;
    
    private static List<GameObject> playerList = new List<GameObject>();
    
    public PhotonPlayer owner;

    void Awake()
    {
        m_PlayerAnimator = GetComponent<Animator>();
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
        m_AttackScript = GetComponent<PlayerAttackScript>();
        
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
            translation = m_CurrentFacing ? Vector2.right : Vector2.left;
        else
            translation = Vector2.zero;
        _Move(translation);
        
        
        // -------- tout ce qui vient après n'est exécuté que si le personnage est contrôlé par le joueur --------
        
        if (m_PhotonView.isMine == false)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");

        //Gestion du saut
        if (Input.GetButtonDown("Jump") && m_CurrentState != STATE_DASH)
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
                _Jump();
                m_PhotonView.RPC("PhJump", PhotonTargets.Others);
            }
        }
        
        // Gestion du dash
        float dash = Input.GetAxisRaw("Dash");
        if (dash < 0 && (Time.realtimeSinceStartup * 1000 - m_LastDashTime) >= MsBetweenDashes)
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
        }
        else if (dash > 0 && (Time.realtimeSinceStartup * 1000 - m_LastDashTime) >= MsBetweenDashes)
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
        }

        //Gestion du déplacement horizontal
        else if (horizontal != 0 && m_CurrentState != STATE_DASH && m_CurrentState != STATE_AIMING)
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
                && m_CurrentState != STATE_AIMING)
            {
                ChangeState(STATE_IDLE);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_IDLE);
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts) {
            Vector2 norm = contact.normal;
            norm.Normalize();
            if (norm.y > .7)
            {
                m_Grounded = true;
                m_GroundTimer = Time.realtimeSinceStartup * 1000;
                _SendGroundInfos();
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
        m_Body.velocity = Vector2.zero;
        m_Body.AddForce(new Vector2(0, JumpStrength));
        m_Grounded = false;
        ChangeState(STATE_JUMP);
        _SendGroundInfos();
        m_PlayerAnimator.SetTrigger("Jump");
    }
    
    private void _Dash()
    {
        m_Body.velocity = Vector2.zero;
        if (m_CurrentFacing)
            m_Body.AddForce(new Vector2(DashStrength, 0));
        else
            m_Body.AddForce(new Vector2(-DashStrength, 0));
        originalGravityScale = m_Body.gravityScale;
        m_Body.gravityScale = 0;
        m_Body.drag = 10;
        m_LastDashTime = Time.realtimeSinceStartup * 1000;
        m_PlayerAnimator.SetTrigger("Dash");
    }

    //Déplace le Player horizontalement
    private void _Move(Vector2 Translation)
    {
        transform.Translate(Translation * RunningSpeed * Time.deltaTime * transform.right.x);
    }

    //Précise à l'Animator si le Player est au sol, et donne sa vitesse verticale
    private void _SendGroundInfos()
    {
        m_PlayerAnimator.SetBool("Grounded", m_Grounded);
        if (!m_Grounded && m_CurrentState != STATE_JUMP)
            m_PlayerAnimator.SetTrigger("Fall");
    }
    
    public bool CanAttack()
    {
        return m_Grounded && m_CurrentState != STATE_DASH;
    }
    
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
    
    public bool canTakeDamage()
    {
        return ((Time.realtimeSinceStartup * 1000 - m_LastHitTime) > HitInvincibilityMs);
    }
    [PunRPC]
    void PhTakeDamage(bool direction, PhotonPlayer attacker)
    {
        // fonction potentiellement à améliorer dans le futur
        
        if ((Time.realtimeSinceStartup * 1000 - m_LastHitTime) <= HitInvincibilityMs)
            // pas de spam
            return;
        m_LastHitTime = Time.realtimeSinceStartup * 1000;
        
        m_Lives--;
        if (m_Lives == 0)
        {
            // game over
            //PhSendDestruction();
            //m_PhotonView.RPC("PhSendDestruction", PhotonTargets.Others, STATE_IDLE);
            m_PlayerAnimator.SetTrigger("Die");
            m_Lives = 3;
        }
        
        m_Body.velocity = Vector2.zero;
        if (direction)
            m_Body.AddForce(new Vector2(200, 500));
        else
            m_Body.AddForce(new Vector2(-200, 500));
        
        attacker.AddScore(1);
        
        m_PlayerAnimator.SetTrigger("TakeDamage");
    }

    [PunRPC]
    void PhSendDestruction()
    {
        Destroy(gameObject);
        RechercheLobby recherche = GetComponent<RechercheLobby>();
        recherche.instantiate();
    }
}