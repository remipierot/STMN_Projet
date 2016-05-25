using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Script gérant les explosions et les dégât qu'elles infligent aux joueurs.
 */
public class ExplosionDamage : MonoBehaviour {
    
    public float MsBeforeExplosionDamage = 100; // temporisation avant d'appliquer les dégât
    public PhotonPlayer m_Owner;                // propriétaire de l'explosion
    private float m_ExplosionTimer = 0;
    private List<GameObject> playersInAttackRange = new List<GameObject>();
    private bool exploded = false;              // définit que l'explosion est terminée

	void Start () {
        m_ExplosionTimer = Time.realtimeSinceStartup * 1000;
	}
	
	void Update () {
        if (!exploded && ((Time.realtimeSinceStartup * 1000 - m_ExplosionTimer) > MsBeforeExplosionDamage))
            DoDamage();
	}
    
    void DoDamage() {
        exploded = true;
        if (m_Owner == null)
            return;
        foreach (var player in playersInAttackRange)
        {
            // fait des dégât aux joueurs dans l'explosion
            Rigidbody2D otherBody = player.GetComponent<Rigidbody2D>();
            ((PhotonView)(player.GetComponent<PhotonView>())).RPC("PhTakeDamage", PhotonTargets.All, transform.position.x < otherBody.transform.position.x, m_Owner);
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
    
    public void setOwner(PhotonPlayer player)
    {
        m_Owner = player;
    }
}
