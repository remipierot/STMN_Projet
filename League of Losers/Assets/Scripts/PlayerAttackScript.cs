using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttackScript : MonoBehaviour {
    
    public float knockbackStrength = 4;         //Intensité du knockback
    public BoxCollider2D detectArea;            //Zone de détection d'ennemis (corps à corps)
    public Rigidbody2D m_Projectile;            //Projectile lancé par le personnage lors de l'attaque
    public bool rangedAttack = true;            //Indique que le joueur utilise une attaque à distance. Autrement, indique une attaque corps à corps.
    
    private Rigidbody2D m_Body;                 //Rigidbody2D de l'objet, utile pour le saut
    private PhotonView m_PhotonView;    		//Objet lié au Network
    
    private bool attacking = false;
    private Vector2 mouseStartPosition;
    
    private List<GameObject> playersInAttackRange = new List<GameObject>();
    
    void Awake() {
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
    }

	void Start () {
	
	}
	
	void Update () {
        if (!m_PhotonView.isMine)
            return;
        
        if (!rangedAttack)
        {
            if (Input.GetButtonDown("Attack"))
            {
                if (!attacking)
                {
                    attacking = true;
                    foreach (var player in playersInAttackRange)
                    {
                        // nécessite d'être amélioré
                        Rigidbody2D otherBody = player.GetComponent<Rigidbody2D>();
                        ((PhotonView)(player.GetComponent<PhotonView>())).RPC("PhTakeDamage", PhotonTargets.All, m_Body.transform.position.x < otherBody.transform.position.x);
                    }
                }
            }
            else
                attacking = false;
        }
        else
        {
            if (Input.GetButtonDown("Attack"))
            {
                mouseStartPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                // ajouter effets visuels de visée ici
            }
            if (Input.GetButtonUp("Attack"))
            {
                Vector2 direction = (Vector2)(Input.mousePosition) - mouseStartPosition;
                if (direction.magnitude < 40)
                    // pas de direction de tir
                    direction = new Vector2(1,0);
                else
                    direction.Normalize();
                float angle = Vector2.Angle(new Vector2(1,0), direction);
                if (direction.y < 0)
                    angle *= -1;
                Quaternion directionQuat = Quaternion.Euler(new Vector3(0, 0, angle));
                Rigidbody2D projectileClone = (Rigidbody2D) Instantiate(m_Projectile, transform.position, directionQuat);
                projectileClone.GetComponent<Arrow>().setOwner(this.gameObject.GetComponent<PlayerControllerScript>().owner);
                projectileClone.velocity = direction * 15;
            }
        }
	}
    
    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.tag == "Player")
        {
            if (playersInAttackRange.Contains(coll.gameObject))
                return;
            playersInAttackRange.Add(coll.gameObject);
        }
    }
    void OnTriggerExit2D(Collider2D coll) {
        if (coll.gameObject.tag == "Player")
            playersInAttackRange.Remove(coll.gameObject);
    }
}
