using UnityEngine;
using System.Collections;

public class PlayerAttackScript : MonoBehaviour {
    
    public float knockbackStrength = 4;
    public BoxCollider2D detectArea;
    public Transform projectile;
    
    private Animator m_PlayerAnimator;          //Animator de l'objet, utile pour changer les états d'animation
    private Rigidbody2D m_Body;                 //Rigidbody2D de l'objet, utile pour le saut
    private PhotonView m_PhotonView;    		//Objet lié au Network
    
    private int attacking = 0;
    private int attackLength = 10;
    
    void Awake() {
        m_PlayerAnimator = GetComponent<Animator>();
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
    }

	void Start () {
	
	}
	
	void Update () {
        if (!m_PhotonView.isMine)
            return;
        
        if (Input.GetButtonDown("Attack"))
            attacking = attackLength;
        else if (attacking > 0)
            attacking--;
	}
    
    void OnCollisionEnter2D(Collision2D coll) {
        //if (coll.gameObject.tag == "Enemy")
        //    coll.gameObject.SendMessage("ApplyDamage", 10);
        Debug.Log("Collision avec " + coll.gameObject.name + ", attaque: " + ((attacking > 0) ? "oui": "non") + ", tag: " + coll.gameObject.tag);
        if (attacking > 0 && coll.gameObject.tag == "Player")
        {
            Rigidbody2D otherBody = coll.gameObject.GetComponent<Rigidbody2D>();
            ((PhotonView)(coll.gameObject.GetComponent("PhotonView"))).RPC("PhTakeDamage", PhotonTargets.All, m_Body.transform.position.x < otherBody.transform.position.x);
            Debug.Log("Attaque le joueur " + coll.gameObject.name);
            /*
            projectile
            Instantiate(brick, new Vector3(x, y, 0), Quaternion.identity);
            cube.AddComponent<Rigidbody2D>();
            cube.transform.position = new Vector3(x, y, 0);
            //*/
        }
    }
}
