using UnityEngine;
using System.Collections;

public class BackgroundRandomiser : MonoBehaviour {

    public Sprite background1;
    public Sprite background2;
    

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);
        switch (Random.Range(0, 2))
        {
            case 0: this.GetComponent<SpriteRenderer>().sprite = background1;break;
            case 1: this.GetComponent<SpriteRenderer>().sprite = background2; break;
        }
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
