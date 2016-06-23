using UnityEngine;
using System.Collections;

/// <summary>
/// Script gérant le comportement d'un projectile tel qu'une flèche.
/// </summary>
public class Arrow : MonoBehaviour {
    
    public bool isExplosive = false; // vrai pour une flèche explosive
    //public CircleCollider2D explosionRegion;
    public GameObject explosionParticleSystem;
    
    private PhotonPlayer m_Owner;
    private Rigidbody2D m_Body;
    private bool m_Broken = false; // flèche cassée et en train de tomber, ne peut faire de dégât
    private bool m_Fixed = true; // flèche plantée dans le sol
    private bool m_Launched = false; // flèche lancée par le joueur
    private PhotonView m_PhotonView; // synchronisation de la flèche
    private BoxCollider2D m_Coll;
    
    public GameObject m_HitParticles;
    private SpriteRenderer renderer;
    private ParticleSystem particles;
    public int m_BrokenLifetimeSeconds=60;
    
    /// <summary>
    /// Retourne vrai si la flèche est au sol, ou en train de tomber
    /// </summary>
    /// <returns>vrai si la flèche est au sol, ou en train de tomber</returns>
    public bool isBroken() { return m_Broken || m_Fixed; }
    /// <summary>
    /// Retourne vrai si la flèche a été lancée
    /// </summary>
    /// <returns>vrai si lancée</returns>
    public bool isLaunched() { return m_Launched; }
    /// <summary>
    /// "Casse" la flèche, la rendant non dangereuse et la faisant tomber au sol. Dans le cas d'une flèche explosive, fait exploser la flèche.
    /// </summary>
    public void _break() {
        m_Broken = true;
        if (isExplosive)
        {
            if (m_PhotonView.isMine)
            {
                // effet de particule épique
                GameObject partSystem = (GameObject)Instantiate(explosionParticleSystem, transform.position, Quaternion.identity);
                partSystem.GetComponent<ExplosionDamage>().setOwner(m_Owner);
                m_PhotonView.RPC("PhSpawnExplosion", PhotonTargets.Others);
            }
            
            if (renderer != null)
                renderer.enabled = false;
            if (particles != null)
                particles.Stop();
        }
        else
        {
            m_Body.velocity = new Vector2((m_Body.velocity.x>0)?1:-1, 0);
            m_Body.angularVelocity = 650;
        }
        
        // destruction de la flèche
        StartCoroutine(DisposeOfArrow());
    }
    
    /// <summary>
    /// Sérialisation des propriétés de la flèche.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
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
            bool oldBroken = m_Broken;
            m_Broken = (bool) stream.ReceiveNext();
            m_Fixed = (bool) stream.ReceiveNext();
            bool oldLaunched = m_Launched;
            m_Launched = (bool) stream.ReceiveNext();
            
            if (m_Body == null)
                m_Body = GetComponent<Rigidbody2D>();
            if (m_PhotonView == null)
                m_PhotonView = GetComponent<PhotonView>();
            if (m_Coll == null)
                m_Coll = GetComponent<BoxCollider2D>();
            // gestion des informations lues
            foreach (var player in PhotonNetwork.playerList)
                if (player.ID == m_PhotonView.ownerId)
                    m_Owner = player;
            
            if (m_Fixed)
            {
                m_Body.isKinematic = true;
                m_Body.freezeRotation = true;
                m_Coll.enabled = false;
            }
            if (oldLaunched != m_Launched && !m_Fixed && !m_Broken)
                Launch();
            if (oldBroken != m_Broken && isExplosive)
            {
                if (renderer != null)
                    renderer.enabled = false;
                if (particles != null)
                    particles.Stop();
            }
        }
    }

    /// <summary>
    /// Initialisation des différentes propriétés de la flèche.
    /// </summary>
	void Start () {
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
        m_Coll = GetComponent<BoxCollider2D>();
        m_Body.isKinematic = true;
        
        SpriteRenderer rndr = GetComponent<SpriteRenderer>();
        if (rndr != null)
            renderer = rndr;
        foreach (Transform obj in transform)
        {
            rndr = obj.GetComponent<SpriteRenderer>();
            if (rndr != null)
                renderer = rndr;
            ParticleSystem part = obj.GetComponent<ParticleSystem>();
            if (part != null)
                particles = part;
        }
        
        if (!m_PhotonView.isMine)
            // la flèche est instanciée mais pas correctement attachée à son lanceur pour les autres joueurs.
            // On compense en masquant la flèche...
            if (renderer != null)
                renderer.enabled = false;
	}
    
    /// <summary>
    /// Change les propriétés physiques de la flèche, lui permettant d'être lancée.
    /// </summary>
    public void Launch()
    {
        m_Launched = true;
        m_Broken = false;
        m_Fixed = false;
        m_Body.isKinematic = false;
        if (renderer != null)
            renderer.enabled = true;
    }
	
    /// <summary>
	/// Update is called once per frame
    /// </summary>
	void Update () {
        if (!m_Launched || m_Body.velocity.magnitude < 1 || m_Fixed)
            return;
        // oriente le projectile dans la direction de déplacement
        Vector2 direction = m_Body.velocity;
        float angle = Vector2.Angle(new Vector2(1,0), direction);
        if (direction.y < 0)
            angle *= -1;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
	}
    
    /// <summary>
    /// Superposition de la flèche et d'un autre objet physique.
    /// Gère les différentes collisions : avec joueur, avec terrain, etc.
    /// </summary>
    /// <param name="coll"></param>
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
            if (!m_Broken)
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
            if (!m_Broken)
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
        }
        else if (!coll.isTrigger)
        {
            // collision du terrain - on bloque la flèche
            m_Body.velocity = Vector2.zero;
            m_Body.angularVelocity = 0;
            m_Body.isKinematic = true;
            m_Body.freezeRotation = true;
            m_Coll.enabled = false;
            m_Broken = true;
            StartCoroutine(DisposeOfArrow());
            m_Fixed = true;
            if (isExplosive)
                _break();
            
            if (m_PhotonView.isMine)
                m_PhotonView.RPC("PhArrowSound", PhotonTargets.All, "stickground");
            
            if (m_HitParticles != null)
            {
                Instantiate(m_HitParticles, transform.position, transform.rotation);
                m_PhotonView.RPC("PhSpawnHit", PhotonTargets.Others);
            }
        }
    }
    
    /// <summary>
    /// Définit le joueur ayant lancé la flèche. Permet d'actualiser le score.
    /// </summary>
    /// <param name="player"></param>
    public void setOwner(PhotonPlayer player)
    {
        m_Owner = player;
    }
    
    /// <summary>
    /// Joue un son.
    /// </summary>
    /// <param name="sound">type de son ou d'évènement à jouer</param>
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
    
    /// <summary>
    /// Détruit la flèche après m_BrokenLifetimeSeconds secondes.
    /// </summary>
    /// <returns></returns>
    IEnumerator DisposeOfArrow()
    {
        yield return new WaitForSeconds(m_BrokenLifetimeSeconds);
        PhotonNetwork.Destroy(this.gameObject);
    }
    
    /// <summary>
    /// Créé un effet visuel d'explosion pour les flèches explosives.
    /// </summary>
    [PunRPC]
    void PhSpawnExplosion()
    {
        GameObject partSystem = (GameObject)Instantiate(explosionParticleSystem, transform.position, Quaternion.identity);
    }
    
    /// <summary>
    /// Créé un effet visuel d'impact.
    /// </summary>
    [PunRPC]
    void PhSpawnHit()
    {
        Instantiate(m_HitParticles, transform.position, transform.rotation);
    }
}
