using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Classe pour afficher le score de la partie en cours 
/// </summary>
public class GUI_ScorePanel : MonoBehaviour {

    public GameObject scoresPanel;
    GameObject SPJ1;
    GameObject SPJ2;
    GameObject SPJ3;
    GameObject SPJ4;

    Text NameJ1;
    Text NameJ2;
    Text NameJ3;
    Text NameJ4;
    Text ScoreJ1;
    Text ScoreJ2;
    Text ScoreJ3;
    Text ScoreJ4;

    Color couleurJ1;
    Color couleurJ2;
    Color couleurJ3;
    Color couleurJ4;

    // Use this for initialization
    void Start () {
        SPJ1 = GameObject.Find("SPJoueur1");
        SPJ2 = GameObject.Find("SPJoueur2");
        SPJ3 = GameObject.Find("SPJoueur3");
        SPJ4 = GameObject.Find("SPJoueur4");
        NameJ1 = GameObject.Find("NameJ1").GetComponent<Text>();
        NameJ2 = GameObject.Find("NameJ2").GetComponent<Text>();
        NameJ3 = GameObject.Find("NameJ3").GetComponent<Text>();
        NameJ4 = GameObject.Find("NameJ4").GetComponent<Text>();
        ScoreJ1 = GameObject.Find("ScoreJ1").GetComponent<Text>();
        ScoreJ2 = GameObject.Find("ScoreJ2").GetComponent<Text>();
        ScoreJ3 = GameObject.Find("ScoreJ3").GetComponent<Text>();
        ScoreJ4 = GameObject.Find("ScoreJ4").GetComponent<Text>();

        couleurJ1 = GameObject.Find("ColorJ1").GetComponent<RawImage>().color;
        couleurJ2 = GameObject.Find("ColorJ2").GetComponent<RawImage>().color;
        couleurJ3 = GameObject.Find("ColorJ3").GetComponent<RawImage>().color;
        couleurJ4 = GameObject.Find("ColorJ4").GetComponent<RawImage>().color;
    }
	
	// Update is called once per frame
	void Update () {

        ///Activation des encarts joueurs présents
        if (PhotonNetwork.playerList.Length >= 2)
            SPJ2.SetActive(true);
        else
            SPJ2.SetActive(false);

        if (PhotonNetwork.playerList.Length >= 3)
            SPJ3.SetActive(true);
        else
            SPJ3.SetActive(false);

        if (PhotonNetwork.playerList.Length == 4)
            SPJ4.SetActive(true);
        else
            SPJ4.SetActive(false);

        ///Remplissage du menu
        int pNumber = 1;
        foreach (var player in PhotonNetwork.playerList) ///Pour chaque joueur de la salle
        {
            switch (pNumber)
            {
                case 1:
                    NameJ1.text = player.name;
                    ScoreJ1.text = player.GetScore().ToString();    
                    if(player.customProperties.ContainsKey("Couleur"))
                        couleurJ1 = idCouleur((int)player.customProperties["Couleur"]); 
                    break;
                case 2:
                    NameJ2.text = player.name;
                    ScoreJ2.text = player.GetScore().ToString();
                    if (player.customProperties.ContainsKey("Couleur"))
                        couleurJ2 = idCouleur((int)player.customProperties["Couleur"]);
                    break;
                case 3:
                    NameJ3.text = player.name;
                    ScoreJ3.text = player.GetScore().ToString();
                    if (player.customProperties.ContainsKey("Couleur"))
                        couleurJ3 = idCouleur((int)player.customProperties["Couleur"]);
                    break;
                case 4:
                    NameJ4.text = player.name;
                    ScoreJ4.text = player.GetScore().ToString();
                    if (player.customProperties.ContainsKey("Couleur"))
                        couleurJ4 = idCouleur((int)player.customProperties["Couleur"]);
                    break;
            }
            pNumber++;
        }

        /// Gestion de l'apparition/disparition du menu
        if (Input.GetButton("Stats"))
            scoresPanel.SetActive(true);
        else
            scoresPanel.SetActive(false);
    }

    Color idCouleur(int idCouleur)
    {
    
        switch(idCouleur)
        {
            case 1:
                return Color.red; 
                
            case 2:
                return Color.blue;
                
            case 3:
                return Color.black;
            case 4:
                return Color.magenta;
            case 5:
                return Color.green;
            case 6:
                return Color.yellow;
            default:
                return Color.white;
        }
    }
}
