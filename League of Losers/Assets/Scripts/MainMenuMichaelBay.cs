using UnityEngine;
using System.Collections;

public class MainMenuMichaelBay : MonoBehaviour {
    
    public GameObject explosion;
    public float scale = .3f;
    public BoxCollider2D area;

	// Use this for initialization
	void Start () {
        StartCoroutine(PopExplosion());
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    
    void OnDestroy()
    {
        StopAllCoroutines();
    }
    
    IEnumerator PopExplosion()
    {
        float time = Random.Range(.3f, 4f);
        yield return new WaitForSeconds(time);
        float x = transform.position.x + area.offset.x + Random.Range(-area.size.x*.5f, area.size.x*.5f);
        float y = transform.position.y + area.offset.y + Random.Range(-area.size.y*.5f, area.size.y*.5f);
        GameObject obj = (GameObject) Instantiate(explosion, new Vector3(x,y,transform.position.z), Quaternion.identity);
        obj.transform.localScale = new Vector3(scale, scale, scale);
        StartCoroutine(PopExplosion());
    }
}
