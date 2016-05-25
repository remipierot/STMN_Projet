using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * Script supprimant automatiquement l'objet auquel il est attaché, une fois que l'effet de particule de celui-ci est terminé
 */
public class ParticleSystemAutodestroy : MonoBehaviour {
    
    private List<ParticleSystem> systems = new List<ParticleSystem>();

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
        Destroy(gameObject);
	}
}
