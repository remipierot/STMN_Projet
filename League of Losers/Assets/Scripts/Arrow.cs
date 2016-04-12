using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {
    
    private PhotonPlayer m_Owner;
    private Rigidbody2D m_Body;
    private bool m_Broken = false; // flèche cassée et en train de tomber, ne peut faire de dégât
    private bool m_Fixed = false; // flèche plantée dans le sol
    public PhotonView m_PhotonView; // synchronisation de la flèche
    
    public bool isBroken() { return m_Broken || m_Fixed; }
    public void _break() {
        m_Broken = true;
        m_Body.velocity = new Vector2((m_Body.velocity.x>0)?1:-1, 0);
        m_Body.angularVelocity = 650;
        // pas besoin de synchroniser la flèche pendant qu'elle retombe au sol...
        m_PhotonView.synchronization = ViewSynchronization.Off;
    }

	// Use this for initialization
	void Start () {
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
	}
	
	// Update is called once per frame
	void Update () {
        if (m_Body.velocity.magnitude < 10)
            return;
        Vector2 direction = m_Body.velocity;
        float angle = Vector2.Angle(new Vector2(1,0), direction);
        if (direction.y < 0)
            angle *= -1;
        m_Body.rotation = angle;
	}
    
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (m_Fixed)
            return;
        
        if (coll.gameObject.tag == "ArenaEdge")
        {
            // bord du terrain - on supprime la flèche
            Destroy(this.gameObject);
        }
        else if (coll.gameObject.tag == "Player")
        {
            if (!m_Broken)
                if (((PlayerControllerScript)(coll.gameObject.GetComponent<PlayerControllerScript>())).owner != m_Owner)
                {
                    // collision avec le joueur, on lui enlève une vie
                    ((PhotonView)(coll.gameObject.GetComponent<PhotonView>())).RPC("PhTakeDamage", PhotonTargets.All, m_Body.velocity.x > 0, m_Owner);
                    _break();
                }
        }
        else if (coll.gameObject.tag == "Projectile")
        {
            Arrow arrow = coll.gameObject.GetComponent<Arrow>();
            if (!arrow.isBroken())
            {
                // collision avec une autre flèche
                arrow._break();
                _break();
            }
            
            // aucune collision avec les autres projectiles
        }
        else
        {
            // collision du terrain - on bloque la flèche
            GetComponent<Rigidbody2D>().isKinematic = true;
            m_Broken = true;
            m_Fixed = true;
            m_PhotonView.synchronization = ViewSynchronization.Off;
        }
    }
    
    public void setOwner(PhotonPlayer player)
    {
        m_Owner = player;
    }
}
