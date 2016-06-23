using UnityEngine;
using System.Collections;

/// <summary>
/// Recolore des parties d'un personnage
/// </summary>
public class PlayerColor : MonoBehaviour {
    /// <summary>
    /// Initialisation. Recolore certaines parties du joueur en fonction de sa propriété photon "Couleur".
    /// </summary>
	void Awake () {
        PhotonPlayer owner = null;
        PhotonView m_PhotonView;
        
        // récupère l'ID du joueur
        m_PhotonView = GetComponent<PhotonView>();
        
        foreach (var player in PhotonNetwork.playerList)
            if (player.ID == m_PhotonView.ownerId)
                owner = player;
        if (owner == null)
        {
            Debug.Log("Couldn't find PhotonPlayer !");
            return;
        }
        
        Color col;
        if (owner.customProperties.ContainsKey("Couleur"))
        {
            col = (Color) owner.customProperties["Couleur"];

            Color colClothes = Color.white;
            Color colHair = Color.white;
            Color colSkin = Color.white;
            
            if (col == Color.green)
            {
                // couleur normale
                colClothes = new Color(110/255f, 220/255f, 110/255f, 1);
            }
            else if (col == Color.red)
            {
                colHair = new Color(100/255f, 80/255f, 40/255f, 1);
                colClothes = new Color(1, 30/255f, 20/255f, 1);
            }
            else if (col == Color.blue)
            {
                colHair = new Color(1, 200/255f, 90/255f, 1);
                colClothes = new Color(130/255f, 160/255f, 1, 1);
            }
            else if (col == Color.yellow)
            {
                colHair = new Color(140/255f, 80/255f, 0, 1);
                colClothes = new Color(200/255f, 200/255f, 0, 1);
            }
            else if(col == Color.magenta)
            {
                colHair = new Color(204 / 255f, 0f, 153/255f, 1);
                colClothes = new Color(204 / 255f, 0f, 153 / 255f, 1);
            }
            else if(col == Color.black)
            {
                colHair = new Color(0f, 0f, 0, 1);
                colClothes = new Color(0f, 0f, 0, 1);
            }
            // recolore le joueur
            transform.GetComponent<Spriter2UnityDX.EntityRenderer>().Color = colSkin;
            getChildByName(transform, "ClassicLegF1").GetComponent<SpriteRenderer>().color = colClothes;
            getChildByName(transform, "ClassicLegB1").GetComponent<SpriteRenderer>().color = colClothes;
            getChildByName(transform, "ClassicPelvis").GetComponent<SpriteRenderer>().color = colClothes;
            getChildByName(transform, "ClassicHead").GetComponent<SpriteRenderer>().color = colHair;
            getChildByName(transform, "ClassicHair").GetComponent<SpriteRenderer>().color = colHair;
        }
        
        /*
        // exemple de changement de sprite
        
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
        */
	}
    
    /// <summary>
    /// Récupère un objet enfant en fonction de son nom.
    /// </summary>
    /// <param name="obj">le parent</param>
    /// <param name="name">le nom de l'enfant</param>
    /// <returns></returns>
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
