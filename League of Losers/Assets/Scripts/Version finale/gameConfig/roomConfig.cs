using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class roomConfig : MonoBehaviour {

    public Texture archer;
    public Texture classeTexture;
    private RawImage player1Image;
    private RawImage player2Image;
    private RawImage player3Image;
    private RawImage player4Image;
    private const int idArcher = 1;
    private const int idChevalier = 2;

    private int numeroJoueur = 0;

	// Use this for initialization
	void Start () {
        player1Image = GameObject.Find("Player1").GetComponentInChildren<RawImage>();
        player2Image = GameObject.Find("Player2").GetComponentInChildren<RawImage>();
        player3Image = GameObject.Find("Player3").GetComponentInChildren<RawImage>();
        player4Image = GameObject.Find("Player4").GetComponentInChildren<RawImage>();

        numeroJoueur = PhotonNetwork.playerList.Length;
        Debug.Log(numeroJoueur);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void onClickArcher()
    {
        classeTexture = archer;
        GameObject.Find("Manager").GetComponent<PhotonView>().RPC("addImageArcher_RPC", PhotonTargets.AllBuffered, numeroJoueur);
    }

    void afficherImageClasse(int joueur, int classe)
    {
        switch(joueur)
        {
            case 1:
                player1Image.texture = affecterTextureClasse(classe);
                break;
            case 2:
                player2Image.texture = affecterTextureClasse(classe);
                break;
            case 3:
                player3Image.texture = affecterTextureClasse(classe);
                break;
            case 4:
                player4Image.texture = affecterTextureClasse(classe);
                break;

            default:
                break;
        }
    }

    Texture affecterTextureClasse(int idClasse)
    {
        switch(idClasse)
        {
            case 1:
                return archer;
            default :
                return archer; //Par défaut retourne la texture de l'archer
        }
    }

    [PunRPC]
    void addImageArcher_RPC(int joueur)
    {
        afficherImageClasse(joueur,idArcher);
        Debug.Log("Envoyer a tout le monde");
    }

}
