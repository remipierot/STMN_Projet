using UnityEngine;
using System.Collections;

public class Arrow : MonoBehaviour {
    
    public bool isExplosive = false; // vrai pour une flèche explosive
    //public CircleCollider2D explosionRegion;
    public GameObject explosionParticleSystem;
    
    private PhotonPlayer m_Owner;
    private Rigidbody2D m_Body;
    private bool m_Broken = false; // flèche cassée et en train de tomber, ne peut faire de dégât
    private bool m_Fixed = false; // flèche plantée dans le sol
    private bool m_Launched = false; // flèche lancée par le joueur
    private PhotonView m_PhotonView; // synchronisation de la flèche
    
    public bool isBroken() { return m_Broken || m_Fixed; }
    public void _break() {
        m_Broken = true;
        if (isExplosive)
        {
            // désactivation de la synchronisation
            m_PhotonView.synchronization = ViewSynchronization.Off;
            // effet de particule épique
            GameObject partSystem = (GameObject)Instantiate(explosionParticleSystem, transform.position, Quaternion.identity);
            if (m_PhotonView.isMine)
                partSystem.GetComponent<ExplosionDamage>().setOwner(m_Owner);
            // destruction de la flèche
            Destroy(this.gameObject);
        }
        else
        {
            m_Body.velocity = new Vector2((m_Body.velocity.x>0)?1:-1, 0);
            m_Body.angularVelocity = 650;
            // pas besoin de synchroniser la flèche pendant qu'elle retombe au sol...
            m_PhotonView.synchronization = ViewSynchronization.Off;
        }
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(m_Owner.ID);
            stream.SendNext(m_Broken);
            stream.SendNext(m_Fixed);
            stream.SendNext(m_Launched);
        }
        else
        {
            int id = (int) stream.ReceiveNext();
            m_Broken = (bool) stream.ReceiveNext();
            m_Fixed = (bool) stream.ReceiveNext();
            bool oldLaunched = m_Launched;
            m_Launched = (bool) stream.ReceiveNext();
            if (oldLaunched != m_Launched)
                Launch();
            foreach (var player in PhotonNetwork.playerList)
                if (player.ID == m_PhotonView.ownerId)
                    m_Owner = player;
        }
        Debug.Log("SERIALIZE, mode=" + (stream.isWriting ? "write": "read"));
    }

	// Use this for initialization
	void Start () {
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
        m_Body.isKinematic = true;
	}
    
    public void Launch()
    {
        m_Launched = true;
        m_Body.isKinematic = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (!m_Launched || m_Body.velocity.magnitude < 1)
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
            {
                if (((PlayerControllerScript)(coll.gameObject.GetComponent<PlayerControllerScript>())).owner != m_Owner)
                {
                    // collision avec le joueur, on lui enlève une vie
                    ((PhotonView)(coll.gameObject.GetComponent<PhotonView>())).RPC("PhTakeDamage", PhotonTargets.All, m_Body.velocity.x > 0, m_Owner);
                    _break();
                }
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
        else if (!coll.isTrigger)
        {
            // collision du terrain - on bloque la flèche
            GetComponent<Rigidbody2D>().isKinematic = true;
            m_Broken = true;
            m_Fixed = true;
            m_PhotonView.synchronization = ViewSynchronization.Off;
            if (isExplosive)
                _break();
        }
    }
    
    public void setOwner(PhotonPlayer player)
    {
        m_Owner = player;
    }
}
