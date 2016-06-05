using UnityEngine;
using System.Collections;

/**
 * Script gérant le comportement d'un projectile tel qu'une flèche.
 */
public class Arrow : MonoBehaviour {
    
    public bool isExplosive = false; // vrai pour une flèche explosive
    //public CircleCollider2D explosionRegion;
    public GameObject explosionParticleSystem;
    
    private PhotonPlayer m_Owner;
    private Rigidbody2D m_Body;
    private bool m_Broken = true; // flèche cassée et en train de tomber, ne peut faire de dégât
    private bool m_Fixed = true; // flèche plantée dans le sol
    private bool m_Launched = false; // flèche lancée par le joueur
    private PhotonView m_PhotonView; // synchronisation de la flèche
    
    public bool isBroken() { return m_Broken || m_Fixed; }
    public bool isLaunched() { return m_Launched; }
    public void _break() {
        m_Broken = true;
        if (isExplosive)
        {
            // effet de particule épique
            GameObject partSystem = (GameObject)Instantiate(explosionParticleSystem, transform.position, Quaternion.identity);
            if (m_PhotonView.isMine)
            {
                partSystem.GetComponent<ExplosionDamage>().setOwner(m_Owner);
                
                // destruction de la flèche
                PhotonNetwork.Destroy(this.gameObject);
            }
        }
        else
        {
            m_Body.velocity = new Vector2((m_Body.velocity.x>0)?1:-1, 0);
            m_Body.angularVelocity = 650;
            // pas besoin de synchroniser la flèche pendant qu'elle retombe au sol...
            //m_PhotonView.synchronization = ViewSynchronization.Off;
        }
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // gère la synchronisation des attributs de la flèche pour les différents joueurs
        if (stream.isWriting)
        {
            stream.SendNext(m_Owner.ID);
            stream.SendNext(m_Broken);
            stream.SendNext(m_Fixed);
            stream.SendNext(m_Launched);
        }
        else
        {
            // synchronisation avec l'owner
            int id = (int) stream.ReceiveNext();
            m_Broken = (bool) stream.ReceiveNext();
            m_Fixed = (bool) stream.ReceiveNext();
            bool oldLaunched = m_Launched;
            m_Launched = (bool) stream.ReceiveNext();
            
            // gestion des informations lues
            foreach (var player in PhotonNetwork.playerList)
                if (player.ID == m_PhotonView.ownerId)
                    m_Owner = player;
            
            if (m_Body == null)
                m_Body = GetComponent<Rigidbody2D>();
            
            if (m_Broken)
            {
                if (isExplosive)
                    Destroy(this.gameObject);
            }
            if (m_Fixed)
            {
                GetComponent<Rigidbody2D>().isKinematic = true;
            }
            if (oldLaunched != m_Launched && !m_Fixed && !m_Broken)
                Launch();
        }
    }

	void Start () {
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
        m_Body.isKinematic = true;
	}
    
    public void Launch()
    {
        m_Launched = true;
        m_Broken = false;
        m_Fixed = false;
        m_Body.isKinematic = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (!m_Launched || m_Body.velocity.magnitude < 1)
            return;
        // oriente le projectile dans la direction de déplacement
        Vector2 direction = m_Body.velocity;
        float angle = Vector2.Angle(new Vector2(1,0), direction);
        if (direction.y < 0)
            angle *= -1;
        m_Body.rotation = angle;
	}
    
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (m_Fixed || !m_Launched)
            return;
        
        if (coll.gameObject.tag == "ArenaEdge")
        {
            if (m_PhotonView.isMine)
                // bord du terrain - on supprime la flèche
                PhotonNetwork.Destroy(this.gameObject);
        }
        else if (coll.gameObject.tag == "Player")
        {
            if (!m_Broken && !m_Fixed)
            {
                if (((PlayerControllerScript)(coll.gameObject.GetComponent<PlayerControllerScript>())).owner != m_Owner)
                {
                    // collision avec le joueur, on lui enlève une vie
                    ((PhotonView)(coll.gameObject.GetComponent<PhotonView>())).RPC("PhTakeDamage", PhotonTargets.All, m_Body.velocity.x > 0, m_Owner);
                    _break();
                    if (m_PhotonView.isMine)
                        m_PhotonView.RPC("PhArrowSound", PhotonTargets.All, "hitplayer");
                }
            }
        }
        else if (coll.gameObject.tag == "Projectile")
        {
            Arrow arrow = coll.gameObject.GetComponent<Arrow>();
            if (!arrow.isBroken() && arrow.isLaunched())
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
            m_Body.velocity = Vector2.zero;
            m_Body.isKinematic = true;
            m_Broken = true;
            m_Fixed = true;
            if (isExplosive)
                _break();
            
            if (m_PhotonView.isMine)
                m_PhotonView.RPC("PhArrowSound", PhotonTargets.All, "stickground");
        }
    }
    
    public void setOwner(PhotonPlayer player)
    {
        m_Owner = player;
    }
    
    [PunRPC]
    void PhArrowSound(string sound)
    {
        Debug.Log("Son flèche : " + sound);
        switch (sound)
        {
            case "stickground":
                // flèche rentre en contact avec le bord du terrain
                // "plop"
                break;
            case "launch":
                // flèche est lancée dans l'air
                // "fiiioooouuu"
                break;
            case "hitplayer":
                // flèche touche joueur
                // "paf"
                break;
            default:
                Debug.Log("ERREUR : son flèche inconnue : " + sound);
                break;
        }
    }
}
