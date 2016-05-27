using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class roomConfig : Photon.MonoBehaviour {

    public Texture archer;
    public Texture classeTexture;

    private GameObject player1;
    private GameObject player2;
    private GameObject player3;
    private GameObject player4;

    private GameObject arene1;
    private GameObject arene2;
    private GameObject arene3;

    private GameObject vote1;
    private GameObject vote2;
    private GameObject vote3;

    private GameObject arenaSelection;
    private GameObject characterSelection;


    private RawImage player1Image;
    private RawImage player2Image;
    private RawImage player3Image;
    private RawImage player4Image;

    //Id des classes joueurs
    private const int idArcher = 1;
    private const int idChevalier = 2;
    private int idClasseJoueur = 1;

    //Id des couleurs pour les joueurs
    private const int couleurVert = 1;
    private const int couleurRouge = 2;
    private const int couleurBleu = 3;
    private const int couleurJaune = 4;
    int couleurActuelle =0;

    private int numeroJoueur = 0;
    private int nombreJoueur = 0;

    //Timer pour la selection du personnage
    private float timer = 30;
    private int timerInt = 0;

    //Vote pour les arenes
    int voteArene1 = 0;
    int voteArene2 = 0;
    int voteArene3 = 0;

    //Photon View
    private PhotonView m_PhotonView;

    Text tempsAfficherCharacter;
    Text tempsAfficherArene;

    private bool enJeu = false;
    private bool stopTimer = false;

	// Use this for initialization
	void Start () {

        characterSelection = GameObject.Find("CharacterSelection");

        m_PhotonView = GameObject.Find("Manager").GetComponent<PhotonView>();

        player1 = GameObject.Find("Player1");
        player2 = GameObject.Find("Player2");
        player3 = GameObject.Find("Player3");
        player4 = GameObject.Find("Player4");

        arene1 = GameObject.Find("Arene1");
        arene2 = GameObject.Find("Arene2");
        arene3 = GameObject.Find("Arene3");

        vote1 = GameObject.Find("Vote1");
        vote2 = GameObject.Find("Vote2");
        vote3 = GameObject.Find("Vote3");

        arenaSelection = GameObject.Find("ArenaSelection");
        

        player1Image = player1.GetComponentInChildren<RawImage>();
        player2Image = player2.GetComponentInChildren<RawImage>();
        player3Image = player3.GetComponentInChildren<RawImage>();
        player4Image = player4.GetComponentInChildren<RawImage>();

        numeroJoueur = PhotonNetwork.player.ID;
        nombreJoueur = PhotonNetwork.playerList.Length;

        activationCanvasPlayer();

        tempsAfficherCharacter = GameObject.Find("Temps").GetComponent<Text>();
        tempsAfficherArene = GameObject.Find("Timer").GetComponent<Text>();

        timerInt = (int)timer;
        arenaSelection.SetActive(false); // Desactivation de la selection de l'arene
	}

    void activationCanvasPlayer()
    {
        switch (PhotonNetwork.playerList.Length)
        {
            case 1:
                player1.SetActive(true);
                player2.SetActive(false);
                player3.SetActive(false);
                player4.SetActive(false);
                break;
            case 2:
                player1.SetActive(true);
                player2.SetActive(true);
                player3.SetActive(false);
                player4.SetActive(false);
                break;
            case 3:
                player1.SetActive(true);
                player2.SetActive(true);
                player3.SetActive(true);
                player4.SetActive(false);
                break;
            case 4:
                player1.SetActive(true);
                player2.SetActive(true);
                player3.SetActive(true);
                player4.SetActive(true);
                break;
            default:
                break;
        }
    }
	
	// Update is called once per frame
	void Update () {

        if(!timerInt.Equals(0))
        {
            //Si un joueur rejoint la partie on affiche son canvas
            if (nombreJoueur != PhotonNetwork.playerList.Length)
            {
                nombreJoueur = PhotonNetwork.playerList.Length;
                timer = 30;
                activationCanvasPlayer();
            }

            //Si le nombre de joueur est supérieur à 1, on active le timer
            if (!nombreJoueur.Equals(1))
            {
                if (PhotonNetwork.isMasterClient)
                {
                    if (!stopTimer)
                    {
                        timer -= Time.deltaTime;
                        Debug.Log("timer" + timer + " " + timerInt);
                        if (timerInt != (int)timer)
                        {
                            m_PhotonView.RPC("updateTimer_RPC", PhotonTargets.AllBuffered, timer);
                        }
                    }
                }
            }
        }

        //Lorsque le timer est à 0 on passe à la selection de l'arène
        else
        {
            PhotonNetwork.player.customProperties = new ExitGames.Client.Photon.Hashtable();
            PhotonNetwork.player.customProperties.Add("Classe", idClasseJoueur);
            PhotonNetwork.player.customProperties.Add("Couleur", affecterColorClasse());


            //Si la partie n'a pas débuté
            if(!enJeu)
            {
                stopTimer = true;
                arenaSelection.SetActive(true);
                characterSelection.SetActive(false);
                enJeu = true;
            }
                //Si la partie a débuté
            else
            {
                //On passe directement au mode de jeu selectionné ainsi qu'à la partie
            }
        }
	}
    /**
     * lors du clic sur l'image de l'archer
     * */
    public void onClickArcher()
    {
        classeTexture = archer;
        idClasseJoueur = idArcher;
        m_PhotonView.RPC("addImageArcher_RPC", PhotonTargets.AllBuffered, numeroJoueur);
    }

    /**
     * Affiche l'image de la classe choisit par l'utilisateur
     * */
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
            case idArcher:
                return archer;
            default :
                return archer; //Par défaut retourne la texture de l'archer
        }
    }

    [PunRPC]
    void addImageArcher_RPC(int joueur)
    {
        afficherImageClasse(joueur,idArcher);
    }


    /**
     * 
     * COULEUR DU PERSO
     * 
     * */

    public void clickCouleurVert()
    {
        Debug.Log("Vert");
        //couleurActuelle = couleurVert;
        m_PhotonView.RPC("addCouleur_RPC", PhotonTargets.AllBuffered,  numeroJoueur, couleurVert);
    }

    public void clickCouleuRouge()
    {
        Debug.Log("Rouge");
        //couleurActuelle = couleurRouge;
        m_PhotonView.RPC("addCouleur_RPC", PhotonTargets.AllBuffered, numeroJoueur, couleurRouge);
    }
    
    public void clickCouleurBleu()
    {
        Debug.Log("Bleu");
        //couleurActuelle = couleurBleu;
        m_PhotonView.RPC("addCouleur_RPC", PhotonTargets.AllBuffered, numeroJoueur, couleurBleu);
    }
        
    public void clickCouleurJaune()
    {
        Debug.Log("Jaune");
        //couleurActuelle = couleurJaune;
        m_PhotonView.RPC("addCouleur_RPC", PhotonTargets.AllBuffered, numeroJoueur, couleurJaune);
    }

 
    void couleurClasse(int idJoueur)
    {
        switch(idJoueur)
        {
            case 1:
                //Colorier le fond
                player1Image.color = affecterColorClasse();
                break;
            case 2:
                player2Image.color = affecterColorClasse();
                break;
            case 3:
                player3Image.color = affecterColorClasse();
                break;
            case 4:
                player4Image.color = affecterColorClasse();
                break;

        }
    }

    Color affecterColorClasse()
    {
        switch (couleurActuelle)
        {
            case 1:
                //Colorier le fond
                return Color.green;

            case 2:
                return Color.red;

            case 3:
                return Color.blue;

            case 4:
                return Color.yellow;

            default:
                return Color.green;
        }
    }


    [PunRPC]
    void addCouleur_RPC(int idJoueur, int couleur)
    {
        couleurActuelle = couleur;
        couleurClasse(idJoueur);
    }


    /**
     * 
     * TIMER
     * 
     * */
    void updateTimer(float timer)
    {
        timerInt = (int)timer;
        tempsAfficherCharacter.text = timerInt.ToString();
    }


    [PunRPC]
    void updateTimer_RPC(float timer)
    {
        updateTimer(timer);
    }

    /**
     * 
     *  VOTE DE L'ARENE
     * 
     * */
    void clickArene1()
    {
        
    }

    void clickArene2()
    {

    }

    void clickArene3()
    {

    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

}
