using UnityEngine;
using System.Collections;

public class PlayerControllerScript : MonoBehaviour
{
    const int STATE_IDLE = 0,       //Animation d'attente (immobile)
              STATE_RUN = 1,        //Animation de course
              STATE_DASH = 2;       //Dash (pas encore utilisé)
    const bool FACE_RIGHT = true,   //Regard du Player vers la droite
               FACE_LEFT = false;   //Regard du Player vers la gauche

    public float RunningSpeed = 4,          //Vitesse de course
                 JumpStrength = 400,        //Force de saut
                 DashStrength = 1300,        //Force du dash
                 MsBetweenDashes = 2000,    //Temps minimum entre 2 dash (en millisecondes)
                 MsBeforeDashGravityRestored = 200; // Temps avant que la gravité soit restaurée lors d'un dash

    private Animator m_PlayerAnimator;          //Animator de l'objet, utile pour changer les états d'animation
    private Rigidbody2D m_Body;                 //Rigidbody2D de l'objet, utile pour le saut
    private bool m_CurrentFacing = FACE_RIGHT,  //Direction courante du regard du Player
                 m_Grounded = true,             //Flag indiquant si le Player est au sol ou non
                 m_DoubleJumped = false;        //Flag indiquant si le Player a fait un double saut
    private int m_CurrentState = STATE_IDLE;    //Etat d'animation courant
    private PhotonView m_PhotonView;    		//Objet lié au Network
    private float m_LastDashTime = 0;           //Dernière fois que le dash a été activé (en millisecondes)
    private Vector2 translation;
    
    private float originalGravityScale;

    void Awake()
    {
        m_PlayerAnimator = GetComponent<Animator>();
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        //Vérifie la collision entre le sol et le Player
        m_Grounded = m_Body.velocity.y < 0.09f && m_Body.velocity.y > -0.09f;
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
            _ChangeDirection(horizontal > 0);
            m_PhotonView.RPC("PhChangeDirection", PhotonTargets.Others, horizontal > 0);

            //Si l'on est au sol, passer en animation de course
            if (m_Grounded)
            {
                _ChangeState(STATE_RUN);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, STATE_RUN);
            }
        }
        else
        {
            //Si l'on est au sol, passer en animation d'attente
            if (m_Grounded && m_CurrentState != STATE_IDLE && m_CurrentState != STATE_DASH)
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
            Vector3 flippedScale = transform.localScale;
            flippedScale.x *= -1;
            transform.localScale = flippedScale;

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
        transform.Translate(Translation * RunningSpeed * Time.deltaTime);
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
    void PhTakeDamage(bool direction)
    {
        // à améliorer
        m_Body.velocity = Vector2.zero;
        if (direction)
            m_Body.AddForce(new Vector2(200, 500));
        else
            m_Body.AddForce(new Vector2(-200, 500));
    }

    [PunRPC]
    void PhSendGroundInfos(bool Grounded, float VerticalSpeed)
    {
        m_PlayerAnimator.SetBool("OnGround", Grounded);
        m_PlayerAnimator.SetFloat("VerticalSpeed", VerticalSpeed);
    }
}