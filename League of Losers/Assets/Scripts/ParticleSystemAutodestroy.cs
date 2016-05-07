using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleSystemAutodestroy : MonoBehaviour {
    
    private List<ParticleSystem> systems = new List<ParticleSystem>();

	// Use this for initialization
	void Start () {
        foreach (Transform child in transform)
        {
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
        Destroy(gameObject);
	}
}
