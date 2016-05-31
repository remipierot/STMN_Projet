using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


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
    private GameObject randomArene;

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
    public const int couleurVert = 1;
    public const int couleurRouge = 2;
    public const int couleurBleu = 3;
    public const int couleurJaune = 4;
    int couleurActuelle =0;

    private int numeroJoueur = 0;
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
    private string nomArene = "";

    //Photon View
    private PhotonView m_PhotonView;

    Text tempsAfficherCharacter;
    Text tempsAfficherArene;

    private bool enJeu = false;
    private bool stopTimer = false;
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
        timerArene = (int)timer;
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
                        
                        if (timerInt != (int)timer)
                        {
                            Debug.Log("Mise à jour timer");
                            m_PhotonView.RPC("updateTimer_RPC", PhotonTargets.AllBuffered, timer);
                        }
                    }
                }
            }
        }

        //Lorsque le timer est à 0 on passe à la selection de l'arène
        else
        {
            //Si ils ne sont pas dans la selection d'arene
            if(!areneSelection)
            {
                PhotonNetwork.player.customProperties = new ExitGames.Client.Photon.Hashtable();
                PhotonNetwork.player.customProperties.Add("Classe", idClasseJoueur);
                PhotonNetwork.player.customProperties.Add("Couleur", affecterColorClasse());

                //Si la partie n'a pas débuté on passe à la selection de l'arene
                if(!enJeu)
                {
                    if (PhotonNetwork.isMasterClient)
                    {
                        m_PhotonView.RPC("initialisationArene_RPC", PhotonTargets.All);
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

                if(timerArene.Equals(0))
                {
                    //On instancie la scène selectionné
                    //PhotonNetwork.LoadLevel(areneVote());
                    nomArene = areneVote();
                    if(nomArene == "Arene1")
                    {
                        SceneManager.LoadScene(3);
                    }
                    else if( nomArene == "Arene2")
                    {
                        SceneManager.LoadScene(4);
                    }
                    else if (nomArene == "Arene3")
                    {
                        SceneManager.LoadScene(5);
                    }
                    
                }
            }
        }
	}

    //Initialisation des variables pour tout le monde
    [PunRPC]
    void initialisationArene_RPC()
    {
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
        Debug.Log("Affichage de l'image");
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

    /**
     * 
     *  VOTE DE L'ARENE
     * 
     * */
    public void clickArene1()
    {
        arene1.GetComponent<Button>().interactable = false;
        m_PhotonView.RPC("addArene_RPC", PhotonTargets.AllBuffered, numeroJoueur,1);
    }

    public void clickArene2()
    {
        arene2.GetComponent<Button>().interactable = false;
        m_PhotonView.RPC("addArene_RPC", PhotonTargets.AllBuffered, numeroJoueur,2);
    }

    public void clickArene3()
    {
        arene3.GetComponent<Button>().interactable = false;
        m_PhotonView.RPC("addArene_RPC", PhotonTargets.AllBuffered, numeroJoueur,3);
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
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }

}
