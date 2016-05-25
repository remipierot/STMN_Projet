using UnityEngine;
using System.Collections;

/**
 * Script démontrant comment recolorer des éléments d'un personnage et l'échange d'un des éléments le composant.
 */
public class PlayerColor : MonoBehaviour {
    private Color color;
    private static System.Random rnd;
    private static int i=0;
    
	void Start () {
        if (rnd == null)
            rnd = new System.Random();
        
        float r = rnd.Next(1000)/(float)1000;
        float g = rnd.Next(1000)/(float)1000;
        float b = rnd.Next(1000)/(float)1000;
        color = new Color(r, g, b, 1);
        
        // recolore le joueur
	    Transform leg = getChildByName(transform, "ClassicLegF1");
        SpriteRenderer spriteRndr = leg.GetComponent<SpriteRenderer>();
        spriteRndr.color = color;
        
        // --- just for the lolz : met l'image de chevelure à la place d'une partie du vêtement de l'archer ---
        
        i++;
        switch (i)
        {
            case 1:
                spriteRndr.sprite = (Sprite) (Resources.Load("Bowman/Art/Bowman CharMaps/Classic/ClassicHead", typeof(Sprite)) as Sprite);
                break;
            case 2:
                spriteRndr.sprite = (Sprite) (Resources.Load("Bowman/Art/Bowman CharMaps/Classic/ClassicArrows", typeof(Sprite)) as Sprite);
                break;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    
    Transform getChildByName(Transform obj, string name)
    {
        // permet de récupérer un des descendants d'un objet
        if (obj.name == name)
            return obj;
        
	    foreach (Transform child in obj)
        {
            Transform res = getChildByName(child, name);
            if (res != null)
                return res;
        }
        
        return null;
    }
}
