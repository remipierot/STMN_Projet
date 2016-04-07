using UnityEngine;
using System.Collections;

public class PlayerControllerScript : MonoBehaviour
{
    const int STATE_IDLE = 0,       //Animation d'attente (immobile)
              STATE_RUN = 1;        //Animation de course
    const bool FACE_RIGHT = true,   //Regard du Player vers la droite
               FACE_LEFT = false;   //Regard du Player vers la gauche

    public float RunningSpeed = 4,          //Vitesse de course
                 JumpStrength = 400,        //Force de saut
                 DashStrength = 10,         //Force du dash (multiplicateur du vecteur déplacement de base)
                 MsBetweenDashes = 2000;    //Temps minimum entre 2 dash (en millisecondes)

    private Animator m_PlayerAnimator;          //Animator de l'objet, utile pour changer les états d'animation
    private Rigidbody2D m_Body;                 //Rigidbody2D de l'objet, utile pour le saut
    private bool m_CurrentFacing = FACE_RIGHT,  //Direction courante du regard du Player
                 m_Grounded = true,             //Flag indiquant si le Player est au sol ou non
                 m_DoubleJumped = false;        //Flag indiquant si le Player a fait un double saut
    private int m_CurrentState = STATE_IDLE;    //Etat d'animation courant
    private PhotonView m_PhotonView;    		//Objet lié au Network
    private float m_LastDashTime = 0;           //Dernière fois que le dash a été activé (en millisecondes)

    void Awake()
    {
        m_PlayerAnimator = GetComponent<Animator>();
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (m_PhotonView.isMine == false)
        {
            return;
        }

        Vector2 translation = Vector2.zero;
        float horizontal;

        //Vérifie la collision entre le sol et le Player
        m_Grounded = m_Body.velocity.y < 0.09f && m_Body.velocity.y > -0.09f;

        //Gestion du saut
        if (Input.GetButtonDown("Jump"))
        {
            //Si l'on est au sol ou qu'on a pas encore fait de double saut, on peut sauter
            if (m_Grounded || !m_DoubleJumped)
            {
                m_DoubleJumped = !m_Grounded;
                _Jump();
            }
        }

        _SendGroundInfos();
        m_PhotonView.RPC("PhSendGroundInfos", PhotonTargets.Others, m_Grounded, m_Body.velocity.y);

        horizontal = Input.GetAxisRaw("Horizontal");

        //Gestion du déplacement horizontal
        if (horizontal != 0)
        {
            //Vérifier que le regard est dans la bonne direction
            translation = (horizontal > 0) ? Vector2.right : Vector2.left;
            _ChangeDirection(horizontal > 0);
            m_PhotonView.RPC("PhChangeDirection", PhotonTargets.Others, horizontal > 0);

            if (Input.GetButtonDown("Fire1") && (Time.realtimeSinceStartup * 1000 - m_LastDashTime) >= MsBetweenDashes)
            {
                translation *= DashStrength;
                m_LastDashTime = Time.realtimeSinceStartup * 1000;
            }

            //Déplacement
            _Move(translation);

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
            if (m_Grounded)
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
    }

    //Donne la force au Rigidbody2D de sauter
    private void _Jump()
    {
        m_Body.velocity = Vector2.zero;
        m_Body.AddForce(new Vector2(0, JumpStrength));
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
    void PhSendGroundInfos(bool Grounded, float VerticalSpeed)
    {
        m_PlayerAnimator.SetBool("OnGround", Grounded);
        m_PlayerAnimator.SetFloat("VerticalSpeed", VerticalSpeed);
    }
}