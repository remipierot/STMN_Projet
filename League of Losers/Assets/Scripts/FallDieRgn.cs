using UnityEngine;
using System.Collections;

public class FallDieRgn : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision");
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Mort d'un joueur");
            collision.gameObject.GetComponent<PlayerControllerScript>().dieFall();
        }
    }
}
