﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class roomConfig : Photon.MonoBehaviour {

    //Texture
    public Texture archer;
    public Texture chevalier;
    public Texture classeTexture; //texture choisit


    private GameObject player1;
    private GameObject player2;
    private GameObject player3;
    private GameObject player4;

    private GameObject arene1;
    private GameObject arene2;
    private GameObject arene3;
    private GameObject randomArene;

    private GameObject vote1;
    private GameObject vote2;
    private GameObject vote3;

    private GameObject arenaSelection;
    private GameObject characterSelection;

    private GameObject startTimerButton;
    private GameObject menuButton;

    private RawImage player1Image;
    private RawImage player2Image;
    private RawImage player3Image;
    private RawImage player4Image;


    private Text player1Couleur;
    private Text player2Couleur;
    private Text player3Couleur;
    private Text player4Couleur;


    //Id des classes joueurs
    private const int idArcher = 1;
    private const int idChevalier = 2;
    private int idClasseJoueur = 1;

    //Id des couleurs pour les joueurs
    public const int couleurVert = 1;
    public const int couleurRouge = 2;
    public const int couleurBleu = 3;
    public const int couleurJaune = 4;
    public const int couleurNoir = 5;
    public const int couleurRose = 6;
    int couleurActuelle =0;

    private int idJoueur = 0;
    private int nombreJoueur = 0;

    //Timer pour la selection du personnage
    private float timer = 30;
    private int timerInt = 0;


    //Timer pour la selection de l'arene
    private float timerArene =  0;

    //Vote pour les arenes
    private int voteArene1 = 0;
    private int voteArene2 = 0;
    private int voteArene3 = 0;
    private int nbVote = 0;
    private string nomArene = "";

    //Bouton pret on/off
    public GameObject buttonPlayer1On;
    public GameObject buttonPlayer2On;
    public GameObject buttonPlayer3On;
    public GameObject buttonPlayer4On;

    public GameObject buttonPlayer1Off;
    public GameObject buttonPlayer2Off;
    public GameObject buttonPlayer3Off;
    public GameObject buttonPlayer4Off;

    bool allPlayerReady;

    //Photon View
    private PhotonView m_PhotonView;

    Text tempsAfficherCharacter;
    Text tempsAfficherArene;

    //Boolean
    private bool enJeu = false;
    private bool stopTimer = true;
    private bool areneSelection = false;

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
        randomArene = GameObject.Find("RandomArene");

        vote1 = GameObject.Find("Vote1");
        vote2 = GameObject.Find("Vote2");
        vote3 = GameObject.Find("Vote3");

        arenaSelection = GameObject.Find("ArenaSelection");

        startTimerButton = GameObject.Find("StartTimer");
        menuButton = GameObject.Find("Menu");

        //Si ce n'est pas l'admin on masque le bouton
        if(!PhotonNetwork.isMasterClient)
        {
            startTimerButton.SetActive(false);
        }

        player1Image = player1.GetComponentInChildren<RawImage>();
        player2Image = player2.GetComponentInChildren<RawImage>();
        player3Image = player3.GetComponentInChildren<RawImage>();
        player4Image = player4.GetComponentInChildren<RawImage>();

        player1Couleur= GameObject.Find("playerNumber1").GetComponent<Text>();
        player2Couleur = GameObject.Find("playerNumber2").GetComponent<Text>();
        player3Couleur = GameObject.Find("playerNumber3").GetComponent<Text>();
        player4Couleur = GameObject.Find("playerNumber4").GetComponent<Text>();

        idJoueur = PhotonNetwork.player.ID;
        nombreJoueur = PhotonNetwork.playerList.Length;

        //Liste des joueurs pret
        allPlayerReady = false;

        //placeDansEcran();
        activationCanvasPlayer();
        onlyReadyInteractable();

        tempsAfficherCharacter = GameObject.Find("Temps").GetComponent<Text>();
        tempsAfficherArene = GameObject.Find("Timer").GetComponent<Text>();

        timerInt = (int)timer;
        timerArene = (int)timer;
        arenaSelection.SetActive(false); // Desactivation de la selection de l'arene
    }

    void remiseAZeroCanvas()
    {
        couleurActuelle = 0;
        idClasseJoueur = 1;
        int place = 1;
        Color couleur;
        /*foreach (var player in PhotonNetwork.playerList)
        {
            couleur = Color.white;//(Color)player.customProperties["Couleur"];
            afficherImageClasse(placeDansEcran(player.ID), 2);
            remiseAZeroCouleur(idJoueur,Color.white);
            place++;
        }*/
        player1Image.texture = null;
        player1Couleur.color = Color.white;
        player2Image.texture = null;
        player2Couleur.color = Color.white;
        player3Image.texture = null;
        player3Couleur.color = Color.white;
        player4Image.texture = null;
        player4Couleur.color = Color.white;

        //Remise à zéro des boutons 
        buttonPlayer1Off.SetActive(true);
        buttonPlayer1On.SetActive(false);
        buttonPlayer2Off.SetActive(true);
        buttonPlayer2On.SetActive(false);
        buttonPlayer3Off.SetActive(true);
        buttonPlayer3On.SetActive(false);
        buttonPlayer4Off.SetActive(true);
        buttonPlayer4On.SetActive(false);
        allPlayerReady = false;
    }
    
    //Affichage du canvas en fonction du nombre de joueurs
    int placeDansEcran(int id)
    {
        int place = 1; //On commence à 1
        foreach(var player in PhotonNetwork.playerList)
        {
            if(player.ID == id)
            {
                return (nombreJoueur+1 - place);
            }
            place++;
        }
        return -1;
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

    void detectButtonReady()
    {
        switch(PhotonNetwork.playerList.Length)
        {
            case 1:
                {
                    if (buttonPlayer1On.activeSelf)
                    {
                        allPlayerReady = true;
                    }
                }
                break;

            case 2:
                {
                    if (buttonPlayer1On.activeSelf && buttonPlayer2On.activeSelf)
                    {
                        allPlayerReady = true;
                    }
                }
                break;

            case 3:

                if (buttonPlayer1On.activeSelf && buttonPlayer2On.activeSelf && buttonPlayer3On.activeSelf)
                {
                    allPlayerReady = true;
                }
                break;

            case 4:

                if (buttonPlayer1On.activeSelf && buttonPlayer2On.activeSelf && buttonPlayer3On.activeSelf && buttonPlayer4On.activeSelf)
                {
                    allPlayerReady = true;
                }
                break;
        }
    }

    void onlyReadyInteractable()
    {
        switch(placeDansEcran(idJoueur))
        {
            case 1:
                {
                    buttonPlayer2Off.GetComponent<Button>().interactable = false;
                    buttonPlayer2On.GetComponent<Button>().interactable = false;

                    buttonPlayer3Off.GetComponent<Button>().interactable = false;
                    buttonPlayer3On.GetComponent<Button>().interactable = false;

                    buttonPlayer4Off.GetComponent<Button>().interactable = false;
                    buttonPlayer4On.GetComponent<Button>().interactable = false;

                }
                break;
            case 2:
                {
                    buttonPlayer1Off.GetComponent<Button>().interactable = false;
                    buttonPlayer1On.GetComponent<Button>().interactable = false;

                    buttonPlayer3Off.GetComponent<Button>().interactable = false;
                    buttonPlayer3On.GetComponent<Button>().interactable = false;

                    buttonPlayer4Off.GetComponent<Button>().interactable = false;
                    buttonPlayer4On.GetComponent<Button>().interactable = false;
                }
                break;
            case 3:
                {
                    buttonPlayer1Off.GetComponent<Button>().interactable = false;
                    buttonPlayer1On.GetComponent<Button>().interactable = false;

                    buttonPlayer2Off.GetComponent<Button>().interactable = false;
                    buttonPlayer2On.GetComponent<Button>().interactable = false;

                    buttonPlayer4Off.GetComponent<Button>().interactable = false;
                    buttonPlayer4On.GetComponent<Button>().interactable = false;
                }
                break;
            case 4:
                {
                    buttonPlayer1Off.GetComponent<Button>().interactable = false;
                    buttonPlayer1On.GetComponent<Button>().interactable = false;

                    buttonPlayer2Off.GetComponent<Button>().interactable = false;
                    buttonPlayer2On.GetComponent<Button>().interactable = false;

                    buttonPlayer3Off.GetComponent<Button>().interactable = false;
                    buttonPlayer3On.GetComponent<Button>().interactable = false;
                }
                break;
        }
    }
	
	// Update is called once per frame
	void Update () {

        if(!timerInt.Equals(0) && !allPlayerReady)
        {
            //Si un joueur rejoint la partie on affiche son canvas
            if (nombreJoueur != PhotonNetwork.playerList.Length)
            {
                nombreJoueur = PhotonNetwork.playerList.Length;
                timer = 30;
                remiseAZeroCanvas();
                activationCanvasPlayer();
                onlyReadyInteractable();
                stopTimer = true; //Si le nombre de joueurs changent on arrete le timer.
                menuButton.SetActive(true);
            }

            //Si le nombre de joueur est supérieur à 1, on active le timer
            if (PhotonNetwork.isMasterClient)
            {
                detectButtonReady();
                if (!stopTimer)
                {
                    timer -= Time.deltaTime;

                    if (timerInt != (int)timer)
                    {
                        Debug.Log("Mise à jour timer");
                        PhotonNetwork.room.open = false;
                        m_PhotonView.RPC("updateTimer_RPC", PhotonTargets.AllBuffered, timer);
                    }
                }
                else
                {
                    PhotonNetwork.room.open = true;
                    startTimerButton.SetActive(true);
                }
             }
        }

        //Lorsque le timer est à 0 on passe à la selection de l'arène
        else
        {
            //Si ils ne sont pas dans la selection d'arene
            if(!areneSelection)
            {
                //Si la partie n'a pas débuté on passe à la selection de l'arene
                if(!enJeu)
                {
                    if (PhotonNetwork.isMasterClient)
                    {
                        m_PhotonView.RPC("initialisationVoteArene_RPC", PhotonTargets.All);
                    }
                }
                    //Si la partie a débuté
                else
                {
                    //On passe directement au mode de jeu selectionné ainsi qu'à la partie
                }
            }
                //Partie gestion de l'arene
            else
            {
                if (PhotonNetwork.isMasterClient)
                {
                        timer -= Time.deltaTime;

                        if (timerArene != (int)timer)
                        {
                            Debug.Log("Mise à jour timer");
                            m_PhotonView.RPC("updateTimer_RPC", PhotonTargets.All, timer);
                        }
                }
                Debug.Log(nbVote);
                if(timerArene.Equals(0) || nbVote.Equals(PhotonNetwork.playerList.Length))
                {
                    //On instancie la scène selectionné
                    //PhotonNetwork.LoadLevel(areneVote());
                    Debug.Log("Nombre de joueurs : " + nbVote);

                    if (PhotonNetwork.isMasterClient)
                    {
                        nomArene = areneVote();
                        m_PhotonView.RPC("loadArene_RPC", PhotonTargets.All, nomArene);
                    } 
                }
            }
        }
    }

    [PunRPC]
    public void loadArene_RPC(string nomArene)
    {
        if (nomArene == "Arene1")
        {
            SceneManager.LoadScene(3);
        }
        else if (nomArene == "Arene2")
        {
            SceneManager.LoadScene(4);
        }
        else if (nomArene == "Arene3")
        {
            SceneManager.LoadScene(5);
        }
    }
                

    //Initialisation des variables pour tout le monde
    [PunRPC]
    void initialisationVoteArene_RPC()
    {
        PhotonNetwork.player.customProperties = new ExitGames.Client.Photon.Hashtable();
        PhotonNetwork.player.customProperties.Add("Classe", idClasseJoueur);
        PhotonNetwork.player.customProperties.Add("Couleur", couleurJoueurCourant());
        stopTimer = true;
        arenaSelection.SetActive(true);
        characterSelection.SetActive(false);
        enJeu = true;
        areneSelection = true;
        timer = 30;
        timerInt = 0;
    }

    /**
     * Fonction qui retourne la carte avec le plus de vote
     * */
    string areneVote()
    {
        int voteGagnant = returnNombre(returnNombre(voteArene1, voteArene2,"Arene1","Arene2"), voteArene3,nomArene,"Arene3");
        return nomArene;     
    }

    //Algo de tri pour le vote
    int returnNombre(int a, int b,string nA, string nB)
    {
        if (a > b)
        {
            nomArene = nA;
            return a;
        }
        else if (a == b)
        {
            System.Random random = new System.Random();
            int nb = random.Next(0, 2);
            switch (nb)
            {
                case 0:
                    {
                        nomArene = nA;
                    }
                    return a;

                case 1:
                    {
                        nomArene = nB;
                    }
                    return b;
                default:
                    {
                        nomArene = nA;
                    }
                    return a;
            }
        }
        else
        {
            nomArene = nB;
            return b;
        }
    }


    /**
     * lors du clic sur l'image de l'archer
     * */
    public void onClickArcher()
    {
        classeTexture = archer;
        idClasseJoueur = idArcher;
        m_PhotonView.RPC("addImageArcher_RPC", PhotonTargets.All, idJoueur);
    }

    public void onClickChevalier()
    {
        classeTexture = chevalier;
        idClasseJoueur = idChevalier;
        m_PhotonView.RPC("addImageChevalier_RPC", PhotonTargets.All, idJoueur);
    }

    /**
     * Affiche l'image de la classe choisit par l'utilisateur
     * */
    void afficherImageClasse(int idJoueur, int classe)
    {
        switch (placeDansEcran(idJoueur))
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
            case idChevalier:
                return chevalier;
            default :
                return null; //Par défaut retourne la texture de l'archer
        }
    }

    [PunRPC]
    void addImageArcher_RPC(int idJoueur)
    {
        Debug.Log("Affichage de l'image");
        afficherImageClasse(idJoueur,idArcher);
    }

    [PunRPC]
    void addImageChevalier_RPC(int idJoueur)
    {
        afficherImageClasse(idJoueur, idChevalier);
    }
    /**
     * 
     * COULEUR DU PERSO
     * 
     * */

    public void clickCouleurVert()
    {
        Debug.Log("Vert");

        m_PhotonView.RPC("addCouleur_RPC", PhotonTargets.All, idJoueur, couleurVert);
    }

    public void clickCouleuRouge()
    {
        Debug.Log("Rouge");

        m_PhotonView.RPC("addCouleur_RPC", PhotonTargets.All, idJoueur, couleurRouge);
    }
    
    public void clickCouleurBleu()
    {
        Debug.Log("Bleu");

        m_PhotonView.RPC("addCouleur_RPC", PhotonTargets.All, idJoueur, couleurBleu);
    }
        
    public void clickCouleurJaune()
    {
        Debug.Log("Jaune");

        m_PhotonView.RPC("addCouleur_RPC", PhotonTargets.All, idJoueur, couleurJaune);
    }

    public void clickCouleurNoir()
    {
        Debug.Log("Noir");

        m_PhotonView.RPC("addCouleur_RPC", PhotonTargets.All, idJoueur, couleurNoir);
    }

    public void clickCouleurRose()
    {
        Debug.Log("Rose");

        m_PhotonView.RPC("addCouleur_RPC", PhotonTargets.All, idJoueur, couleurRose);
    }

    Color couleurJoueurCourant()
    {
        switch(placeDansEcran(idJoueur))
        {
            case 1:
                return player1Couleur.color;

            case 2:
                return player2Couleur.color;

            case 3 :
                return player3Couleur.color;

            case 4:
                return player4Couleur.color;

            default :
                return Color.black;
        }
    }

 
    void couleurClasse(int idJoueur)
    {

        switch(placeDansEcran(idJoueur))
        {
            case 1:
                //Colorier le fond
                player1Couleur.color = affecterColorClasse();
                break;
            case 2:
                player2Couleur.color = affecterColorClasse();
                break;
            case 3:
                player3Couleur.color = affecterColorClasse();
                break;
            case 4:
                player4Couleur.color = affecterColorClasse();
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
            case 5:
                return Color.black;

            case 6:
                return Color.magenta;

            default:
                return Color.green;
        }
    }

    void remiseAZeroCouleur(int idJoueur, Color couleur)
    {
        switch (placeDansEcran(idJoueur))
        {
            case 1:
                //Colorier le fond
                player1Couleur.color = couleur;
                break;
            case 2:
                player2Couleur.color = couleur;
                break;
            case 3:
                player3Couleur.color = couleur;
                break;
            case 4:
                player4Couleur.color = couleur;
                break;
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
        if(areneSelection)
        {
            timerArene = (int)timer;
            tempsAfficherArene.text = timerArene.ToString();
        }
        else
        {
            timerInt = (int)timer;
            tempsAfficherCharacter.text = timerInt.ToString();
        }
    }


    [PunRPC]
    void updateTimer_RPC(float timer)
    {
        updateTimer(timer);
    }

    /// <summary>
    /// Bouton "PRET"
    /// </summary>
    public void clickPretPlayer()
    {
        m_PhotonView.RPC("buttonReady_RPC", PhotonTargets.Others, idJoueur);
    }



    [PunRPC]
    void buttonReady_RPC( int idJoueur)
    {
        int num = placeDansEcran(idJoueur);

            switch (num)
            {
                case 1:
                    {
                        buttonPlayer1Off.SetActive(buttonPlayer1On.activeSelf);
                        buttonPlayer1On.SetActive(!buttonPlayer1Off.activeSelf);
                    }
                    break;
                case 2:
                    {
                        buttonPlayer2Off.SetActive(buttonPlayer2On.activeSelf);
                        buttonPlayer2On.SetActive(!buttonPlayer2Off.activeSelf);
                    }
                    break;
                case 3:
                    {
                        buttonPlayer3Off.SetActive(buttonPlayer3On.activeSelf);
                        buttonPlayer3On.SetActive(!buttonPlayer3Off.activeSelf);
                    }
                    break;
                case 4:
                    {
                        buttonPlayer4Off.SetActive(buttonPlayer4On.activeSelf);
                        buttonPlayer4On.SetActive(!buttonPlayer4Off.activeSelf);
                    }
                    break;

                default:
                    break;
            }
    }

    /*******************************************************************************************************
     * 
     *  VOTE DE L'ARENE
     * 
     * */
    public void clickArene1()
    {
        buttonInteractable();
        m_PhotonView.RPC("addArene_RPC", PhotonTargets.AllBuffered, idJoueur,1);
    }

    public void clickArene2()
    {
        buttonInteractable();
        m_PhotonView.RPC("addArene_RPC", PhotonTargets.AllBuffered, idJoueur,2);
    }

    public void clickArene3()
    {
        buttonInteractable();
        m_PhotonView.RPC("addArene_RPC", PhotonTargets.AllBuffered, idJoueur,3);
    }

    public void buttonInteractable()
    {
        arene1.GetComponent<Button>().interactable = false;
        arene2.GetComponent<Button>().interactable = false;
        arene3.GetComponent<Button>().interactable = false;
        randomArene.GetComponent<Button>().interactable = false;
    }

    public void clickAleatoire()
    {
        randomArene.GetComponent<Button>().interactable = false;
        System.Random random = new System.Random();
        int nombre = random.Next(1, 4);

        switch(nombre)
        {
            case 1:
                clickArene1();
                break;
            case 2 :
                clickArene2();
                break;
            case 3:
                clickArene3();
                break;
            default:
                break;
        }
    }

    [PunRPC]
    void addArene_RPC(int idJoueur, int arene)
    {
        switch(arene)
        {
            case 1:
                {
                    voteArene1 += 1;
                    vote1.GetComponent<Text>().text = voteArene1.ToString();
                    break;
                }
               
            case 2:
                {
                    voteArene2 += 1;
                    vote2.GetComponent<Text>().text = voteArene2.ToString();
                    break;
                }
                
            case 3:
                {
                    voteArene3 += 1;
                    vote3.GetComponent<Text>().text = voteArene3.ToString();
                    break;
                }
                
            default:
                break;
        }
        nbVote++;
    }

    public void clickRetourMenu()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }

    public void clickStartTimer()
    {
        m_PhotonView.RPC("startTimer_RPC", PhotonTargets.All);
    }

    [PunRPC]
    private void startTimer_RPC()
    {
        menuButton.SetActive(false);
        startTimerButton.SetActive(false);
        stopTimer = false;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

}
