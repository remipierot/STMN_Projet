using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Script supprimant automatiquement l'objet auquel il est attaché, une fois que l'effet de particule de celui-ci est terminé
 */
public class ParticleSystemAutodestroy : MonoBehaviour {
    
    private List<ParticleSystem> systems = new List<ParticleSystem>();
    public float AdditionalWaitTimeMs = 0; // nombre de secondes à attendre après la fin du système de particule avant de le détruire

	void Start () {
        foreach (Transform child in transform)
        {
            // enregistre les systèmes de particule
            ParticleSystem system = child.GetComponent<ParticleSystem>();
            if (system != null)
                systems.Add(system);
        }
	}
	
	// Update is called once per frame
	void Update () {
        foreach (ParticleSystem system in systems)
            if(system.IsAlive())
                return;
        // supprime le système
        StartCoroutine(DestroySelf());
	}
    
    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(AdditionalWaitTimeMs/1000);
        Destroy(gameObject);
    }
}
