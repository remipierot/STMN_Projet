using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttackScript : MonoBehaviour {
    
    public float knockbackStrength = 4;
    public BoxCollider2D detectArea;
    public Transform projectile;
    
    private Animator m_PlayerAnimator;          //Animator de l'objet, utile pour changer les états d'animation
    private Rigidbody2D m_Body;                 //Rigidbody2D de l'objet, utile pour le saut
    private PhotonView m_PhotonView;    		//Objet lié au Network
    
    private bool attacking = false;
    
    private static List<GameObject> playersInAttackRange = new List<GameObject>();
    
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
        {
            foreach (var player in playersInAttackRange)
            {
                Rigidbody2D otherBody = player.GetComponent<Rigidbody2D>();
                ((PhotonView)(player.GetComponent("PhotonView"))).RPC("PhTakeDamage", PhotonTargets.All, m_Body.transform.position.x < otherBody.transform.position.x);
                PhotonNetwork.player.AddScore(1);
                /*
                projectile
                Instantiate(brick, new Vector3(x, y, 0), Quaternion.identity);
                cube.AddComponent<Rigidbody2D>();
                cube.transform.position = new Vector3(x, y, 0);
                //*/
            }
        }
	}
    
    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.tag == "Player")
            playersInAttackRange.Add(coll.gameObject);
    }
    void OnTriggerExit2D(Collider2D coll) {
        if (coll.gameObject.tag == "Player")
            playersInAttackRange.Remove(coll.gameObject);
    }
}
