using UnityEngine;

public class PlayerControllerScript : MonoBehaviour
{
    const int STATE_IDLE = 0,       //Animation d'attente (immobile)
              STATE_RUN = 1;        //Animation de course
    const bool FACE_RIGHT = true,   //Regard du Player vers la droite
               FACE_LEFT = false;   //Regard du Player vers la gauche

    public float RunningSpeed = 4,      //Vitesse de course
                 JumpStrength = 250;    //Force de saut
    public LayerMask GroundLayer;       //Layer associé au sol

    private Animator m_PlayerAnimator;          //Animator de l'objet, utile pour changer les états d'animation
    private Rigidbody2D m_Body;                 //Rigidbody2D de l'objet, utile pour le saut
    private bool m_CurrentFacing = FACE_RIGHT,  //Direction courante du regard du Player
                 m_Grounded = true,             //Flag indiquant si le Player est au sol ou non
                 m_DoubleJumped = false;        //Flag indiquant si le Player a fait un double saut
    private int m_CurrentState = STATE_IDLE;    //Etat d'animation courant
    private float m_GroundCheckHeight = 0f;     //Hauteur locale du ground check (déterminée grâce aux dimensions du sprite au démarrage)

    void Start ()
    {
        m_PlayerAnimator = GetComponent<Animator>();
        m_Body = GetComponent<Rigidbody2D>();
        m_GroundCheckHeight = -GetComponent<SpriteRenderer>().bounds.extents.y;
    }

    void Update ()
    {
        bool direction;
        Vector3 translation;

        //Vérifie la collision entre le sol et le Player
        m_Grounded = Physics2D.OverlapCircle(transform.TransformPoint(0, m_GroundCheckHeight, 0), 0.1f, GroundLayer);

        //Précise à l'Animator si le Player est au sol, et donne sa vitesse verticale
        m_PlayerAnimator.SetBool("OnGround", m_Grounded);
        m_PlayerAnimator.SetFloat("VerticalSpeed", m_Body.velocity.y);

        //Gestion du saut
        if (Input.GetKeyUp("up"))
        {
            //Si l'on est au sol ou qu'on a pas encore fait de double saut, on peut sauter
            if (m_Grounded || !m_DoubleJumped)
            {
                m_DoubleJumped = !m_Grounded;

                //Important de reset la velocity pour ne pas en tenir compte quand on fait un double saut
                m_Body.velocity = Vector2.zero;
                m_Body.AddForce(new Vector2(0, JumpStrength));
            }
        }

        //Gestion du déplacement horizontal
        if (Input.GetKey("right") ^ Input.GetKey("left"))
        {
            //Vérifier que le regard est dans la bonne direction
            direction = Input.GetKey("right");
            translation = (direction == FACE_RIGHT) ? Vector3.right : Vector3.left;
            _ChangeDirection(direction);

            //Déplacement
            transform.Translate(translation * RunningSpeed * Time.deltaTime);

            //Si l'on est au sol, passer en animation de course
            if (m_Grounded)
            {
                _ChangeState(STATE_RUN);
            }
        }
        else
        {
            //Si l'on est au sol, passer en animation d'attente
            if (m_Grounded)
            {
                _ChangeState(STATE_IDLE);
            }
        }
    }

    //Changer la direction de regard du Player
    private void _ChangeDirection (bool Direction)
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
    private void _ChangeState (int State)
    {
        if (m_CurrentState != State)
        {
            m_PlayerAnimator.SetInteger("State", State);
            m_CurrentState = State;
        }
    }
}