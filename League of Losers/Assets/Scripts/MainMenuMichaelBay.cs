using UnityEngine;
using System.Collections;

/// <summary>
/// Créé aléatoirement des explosions dans la zone spécifiée (utilisé pour le menu principal)
/// </summary>
public class MainMenuMichaelBay : MonoBehaviour {
    
    public GameObject explosion;
    public float scale = .3f;
    public BoxCollider2D area;

    /// <summary>
	/// Use this for initialization
    /// </summary>
	void Start () {
        StartCoroutine(PopExplosion());
	}
    
	/// <summary>
	/// Update is called once per frame
    /// </summary>
	void Update () {
	
	}
    
    /// <summary>
    /// Appelé lors du déchargement de la scène.
    /// </summary>
    void OnDestroy()
    {
        StopAllCoroutines();
    }
    
    /// <summary>
    /// Créé une explosion aléatoirement dans la zone spécifiée.
    /// Fonction récursive.
    /// </summary>
    /// <returns></returns>
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
