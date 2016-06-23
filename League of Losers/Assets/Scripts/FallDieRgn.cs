using UnityEngine;
using System.Collections;

/// <summary>
/// Région tuant instantanément les joueurs.
/// </summary>
public class FallDieRgn : MonoBehaviour {

    /// <summary>
	/// Use this for initialization
    /// </summary>
	void Start () {
	
	}
    
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update () {
	
	}
    
	/// <summary>
	/// Rentrée d'un joueur dans la zone.
	/// </summary>
	/// <param name="collision"></param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Mort d'un joueur");
            collision.gameObject.GetComponent<PlayerControllerScript>().dieFall();
        }
    }
}
