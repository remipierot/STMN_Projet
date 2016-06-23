using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script gérant les explosions et les dégât qu'elles infligent aux joueurs.
/// </summary>
public class ExplosionDamage : MonoBehaviour {
    
    public float MsBeforeExplosionDamage = 100; // temporisation avant d'appliquer les dégât
    public PhotonPlayer m_Owner;                // propriétaire de l'explosion
    private float m_ExplosionTimer = 0;
    private List<GameObject> playersInAttackRange = new List<GameObject>();
    private bool exploded = false;              // définit que l'explosion est terminée

    /// <summary>
    /// Initialisation
    /// </summary>
	void Start () {
        m_ExplosionTimer = Time.realtimeSinceStartup * 1000;
	}
	
    /// <summary>
    /// Appelé à chaque nouvelle trame.
    /// </summary>
	void Update () {
        if (!exploded && ((Time.realtimeSinceStartup * 1000 - m_ExplosionTimer) > MsBeforeExplosionDamage))
            DoDamage();
	}
    
    /// <summary>
    /// Inflige des dégâts aux joueurs à portée.
    /// </summary>
    void DoDamage() {
        exploded = true;
        if (m_Owner == null)
            return;
        foreach (GameObject player in playersInAttackRange)
        {
            Debug.Log("Player " + player);
            // fait des dégât aux joueurs dans l'explosion
            Rigidbody2D otherBody = player.GetComponent<Rigidbody2D>();
            if (player.GetComponent<PlayerControllerScript>().canTakeDamage())
            {
                Debug.Log("Can take damage = true");
                ((PhotonView)(player.GetComponent<PhotonView>())).RPC("PhTakeDamage", PhotonTargets.All, transform.position.x < otherBody.transform.position.x, m_Owner);
            }
        }
        
        PlayExplosionSound();
    }
    
    /// <summary>
    /// joue le son d'explosion
    /// TODO
    /// </summary>
    private void PlayExplosionSound()
    {
        Debug.Log("Son explosion");
    }
    
    /// <summary>
    /// Entrée d'un GameObject dans la zone d'explosion.
    /// </summary>
    /// <param name="coll"></param>
    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.tag == "Player")
        {
            if (playersInAttackRange.Contains(coll.gameObject))
                return;
            playersInAttackRange.Add(coll.gameObject);
        }
    }
    /// <summary>
    /// Sortie d'un GameObject de la zone d'explosion.
    /// </summary>
    /// <param name="coll"></param>
    void OnTriggerExit2D(Collider2D coll) {
        if (coll.gameObject.tag == "Player")
            playersInAttackRange.Remove(coll.gameObject);
    }
    
    /// <summary>
    /// Définit le joueur ayant causé l'explosion.
    /// </summary>
    /// <param name="player"></param>
    public void setOwner(PhotonPlayer player)
    {
        m_Owner = player;
    }
}
