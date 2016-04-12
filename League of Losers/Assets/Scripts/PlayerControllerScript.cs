using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControllerScript : MonoBehaviour
{
    const int STATE_IDLE = 0,       //Animation d'attente (immobile)
              STATE_RUN = 1,        //Animation de course
              STATE_DASH = 2,       //Dash (pas encore utilisé)
              STATE_AIMING = 3;     //Visée lors d'une attaque rangée
    const bool FACE_RIGHT = true,   //Regard du Player vers la droite
               FACE_LEFT = false;   //Regard du Player vers la gauche

    public float RunningSpeed = 4,          //Vitesse de course
                 JumpStrength = 400,        //Force de saut
                 DashStrength = 1300,       //Force du dash
                 MsBetweenDashes = 2000,    //Temps minimum entre 2 dash (en millisecondes)
                 MsBeforeDashGravityRestored = 200, // Temps avant que la gravité soit restaurée lors d'un dash
                 HitInvincibilityMs = 2000; // Temps durant lequel le joueur est invincible après s'être pris des dégâts
    
    public int m_Lives = 3; // nombre de vies restantes
    
    private Animator m_PlayerAnimator;          //Animator de l'objet, utile pour changer les états d'animation
    private Rigidbody2D m_Body;                 //Rigidbody2D de l'objet, utile pour le saut
    private bool m_CurrentFacing = FACE_RIGHT,  //Direction courante du regard du Player
                 m_Grounded = true,             //Flag indiquant si le Player est au sol ou non
                 m_DoubleJumped = false;        //Flag indiquant si le Player a fait un double saut
    private int m_CurrentState = STATE_IDLE;    //Etat d'animation courant
    public PhotonView m_PhotonView;    		//Objet lié au Network
    private float m_LastDashTime = 0;           //Dernière fois que le dash a été activé (en millisecondes)
    private float m_LastHitTime = 0;            //Dernière fois que le joueur a été touché (pour l'invincibilité)
    private Vector2 translation;
    
    private float originalGravityScale;
    
    private static List<GameObject> playerList = new List<GameObject>();
    
    public PhotonPlayer owner;

    void Awake()
    {
        m_PlayerAnimator = GetComponent<Animator>();
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
        
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

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if( stream.isWriting == true )
            stream.SendNext(m_Lives);
        else
            m_Lives = (int)stream.ReceiveNext();
    }

    void Update()
    {
        //Vérifie la collision entre le sol et le Player
        m_Grounded = m_Body.velocity.y < 0.09f && m_Body.velocity.y > -0.09f && m_CurrentState != STATE_DASH; // le dash désactive temporairement toute vélocité verticale
        _SendGroundInfos();
        
        // restauration de la gravité du personnage après le dash
        if (m_CurrentState == STATE_DASH && (Time.realtimeSinceStartup * 1000 - m_LastDashTime) >= MsBeforeDashGravityRestored)
        {
            m_Body.gravityScale = originalGravityScale;
            m_Body.drag = 0;
            _ChangeState(STATE_IDLE);
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
                m_DoubleJumped = !m_Grounded;
                _Jump();
                m_PhotonView.RPC("PhJump", PhotonTargets.Others);
            }
        }
        
        // Gestion du dash
        float dash = Input.GetAxisRaw("Dash");
        if (dash < 0 && (Time.realtimeSinceStartup * 1000 - m_LastDashTime) >= MsBetweenDashes)
        {
            if (m_CurrentFacing)
            {
                _ChangeDirection(false);
                m_PhotonView.RPC("PhChangeDirection", PhotonTargets.Others, false);
            }
            _ChangeState(STATE_DASH);
            m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_DASH);
        }
        else if (dash > 0 && (Time.realtimeSinceStartup * 1000 - m_LastDashTime) >= MsBetweenDashes)
        {
            if (!m_CurrentFacing)
            {
                _ChangeDirection(true);
                m_PhotonView.RPC("PhChangeDirection", PhotonTargets.Others, true);
            }
            _ChangeState(STATE_DASH);
            m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_DASH);
        }

        //Gestion du déplacement horizontal
        else if (horizontal != 0 && m_CurrentState != STATE_DASH)
        {
            //Vérifier que le regard est dans la bonne direction
            if ((horizontal > 0) != m_CurrentFacing)
            {
                _ChangeDirection(horizontal > 0);
                m_PhotonView.RPC("PhChangeDirection", PhotonTargets.Others, horizontal > 0);
            }

            //passer en animation de course
            if (m_CurrentState != STATE_RUN)
            {
                _ChangeState(STATE_RUN);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_RUN);
            }
        }
        else
        {
            //passe en animation d'attente
            if (m_CurrentState != STATE_IDLE && m_CurrentState != STATE_DASH)
            {
                _ChangeState(STATE_IDLE);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_IDLE);
            }
        }
    }

    //Changer la direction de regard du Player
    private void _ChangeDirection(bool Direction)
    {
        if (m_CurrentFacing != Direction)
        {
            transform.right *= -1;
            m_CurrentFacing = Direction;
        }
    }

    //Changer l'état d'animation
    private void _ChangeState(int State)
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
    }

    //Déplace le Player horizontalement
    private void _Move(Vector2 Translation)
    {
        transform.Translate(Translation * RunningSpeed * Time.deltaTime * transform.right.x);
    }

    //Précise à l'Animator si le Player est au sol, et donne sa vitesse verticale
    private void _SendGroundInfos()
    {
        m_PlayerAnimator.SetBool("OnGround", m_Grounded);
        m_PlayerAnimator.SetFloat("VerticalSpeed", m_Body.velocity.y);
    }

    [PunRPC]
    void PhChangeDirection(bool Direction)
    {
        _ChangeDirection(Direction);
    }

    [PunRPC]
    void PhChangeState(int State)
    {
        _ChangeState(State);
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
            PhSendDestruction();
            m_PhotonView.RPC("PhSendDestruction", PhotonTargets.Others, STATE_IDLE);
            m_Lives = 3;
        }
        
        m_Body.velocity = Vector2.zero;
        if (direction)
            m_Body.AddForce(new Vector2(200, 500));
        else
            m_Body.AddForce(new Vector2(-200, 500));
        
        attacker.AddScore(1);
    }

    [PunRPC]
    void PhSendGroundInfos(bool Grounded, float VerticalSpeed)
    {
        m_PlayerAnimator.SetBool("OnGround", Grounded);
        m_PlayerAnimator.SetFloat("VerticalSpeed", VerticalSpeed);
    }

    [PunRPC]
    void PhSendDestruction()
    {
        Destroy(gameObject);
        RechercheLobby recherche = GetComponent<RechercheLobby>();
        recherche.instantiate();
    }
}