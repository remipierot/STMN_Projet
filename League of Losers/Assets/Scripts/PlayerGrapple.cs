using UnityEngine;
using System.Collections;

public class PlayerGrapple : MonoBehaviour {

    public int ShootingStrength = 25,
               ShootingAngle = 45,
               GrappleProjectionStrength = 500,
               GrappleDurationInMilliseconds = 500;
    public Rigidbody2D Body, 
                       ParentBody;
    public BoxCollider2D Collider;
    public Renderer Renderer;
    public Transform PopSpot;
    public bool IsHooked = false;
    public LineRenderer Rope;
    public PhotonView m_PhotonView;
    public PlayerControllerScript m_PlayerController;
    
    private float m_StartTime = 0;

    public Vector3 LeftShoot { get; private set; }
    public Vector3 RightShoot { get; private set; }
    public Vector3 LeftProjection { get; private set; }
    public Vector3 RightProjection { get; private set; }

    public bool IsGrappling
    {
        get
        {
            return !Body.isKinematic;
        }
        set
        {
            if (!value)
            {
                Body.velocity = Vector3.zero;
                transform.position = PopSpot.position;
                transform.rotation = PopSpot.rotation;
            }
            else
                m_StartTime = Time.realtimeSinceStartup;

            Body.isKinematic = !value;
            Renderer.enabled = value;
            Collider.enabled = value;
        }
    }

    void Start()
    {
        LeftShoot = Quaternion.Euler(0, 0, -ShootingAngle) * Vector3.left;
        RightShoot = Quaternion.Euler(0, 0, ShootingAngle) * Vector3.right;
        LeftProjection = Quaternion.Euler(0, 0, -ShootingAngle - 15) * Vector3.left;
        RightProjection = Quaternion.Euler(0, 0, ShootingAngle + 15) * Vector3.right;
        Renderer.enabled = false;
        Collider.enabled = false;
    }

    void Update()
    {
        if (Input.GetButtonDown("Grapple") && m_PhotonView.isMine && m_PlayerController.canGrapple())
        {
            IsGrappling = true;
            m_PhotonView.RPC("PhSetGrappling", PhotonTargets.Others, IsGrappling, IsHooked);
        }

        if (IsGrappling)
        {
            Body.velocity = (transform.parent.right.x > 0) ? RightShoot * ShootingStrength : LeftShoot * ShootingStrength;

            if (1000 * (Time.realtimeSinceStartup - m_StartTime) > GrappleDurationInMilliseconds)
            {
                IsGrappling = false;
                m_PhotonView.RPC("PhSetGrappling", PhotonTargets.Others, IsGrappling, IsHooked);
            }
        }

        if (IsHooked)
            IsHooked = false;

        DrawRope();
    }

    void OnTriggerEnter2D(Collider2D CollidedObject)
    {
        if (IsGrappling)
        {
            if (CollidedObject.gameObject.tag == "MainCamera")
                // on ignore
                return;
            if (CollidedObject.gameObject.tag == "Player")
            {
                /*CollidedObject.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector3(
                    PopSpot.position.x - CollidedObject.gameObject.transform.position.x,
                    PopSpot.position.y - CollidedObject.gameObject.transform.position.y,
                    PopSpot.position.z - CollidedObject.gameObject.transform.position.z
                );*/
            }
            else if (CollidedObject.gameObject.tag != "ArenaEdge" && CollidedObject.gameObject.tag != "Projectile")
            {
                m_StartTime = 0;
                IsGrappling = false;
                m_PhotonView.RPC("PhSetGrappling", PhotonTargets.Others, IsGrappling, IsHooked);
                IsHooked = true;
                
                m_PlayerController.doGrappleAnim(true);
                StartCoroutine(EndGrappleAnim());
            }
        }
    }

    void DrawRope()
    {
        if (IsGrappling)
        {
            Rope.enabled = true;
            Vector3 start = Vector3.zero,
                    end = Vector3.zero;

            start.x = (ParentBody.transform.right.x > 0) ? ParentBody.position.x + 0.1f : ParentBody.position.x - 0.1f;
            start.y = ParentBody.transform.position.y + 0.45f;
            start.z = -5;
            end.x = transform.position.x;
            end.y = transform.position.y;
            end.z = -5;

            Rope.SetPositions(new Vector3[] { start, end });
        }
        else
            Rope.enabled = false;
    }
    
    [PunRPC]
    void PhSetGrappling(bool grapple, bool hooked)
    {
        IsGrappling = grapple;
        IsHooked = hooked;
        if (IsHooked)
        {
            m_PlayerController.doGrappleAnim(true);
            StartCoroutine(EndGrappleAnim());
        }
    }
    
    IEnumerator EndGrappleAnim()
    {
        yield return new WaitForSeconds(.1f);
        m_PlayerController.doGrappleAnim(false);
    }
}
