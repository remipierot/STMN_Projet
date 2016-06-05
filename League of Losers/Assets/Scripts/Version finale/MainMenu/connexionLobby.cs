using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class connexionLobby : MonoBehaviour {

    private bool spawn = false;
    private int maxPlayer = 1;
    private Room[] game;
    private string roomName = "Test";
    private string maxPlayerString = "4";
    public string Version = "v0.1";

    private string username = "";
    private string gameName = "";
    private bool gamePrivate = true;
    private string gameMode = "Match à mort";
    private int tempsPartie;

    private Hashtable propertiesGame;

    //GameObject du matchmaking à masquer
    private GameObject matchmaking;
    private GameObject createPanel;
    private GameObject sliderTemps;

    private GameObject textTemps;

    //Parametre de création de partie à afficher
    private string tempsString;


	// Use this for initialization
	void Start () {
        matchmaking = GameObject.Find("Matchmaking");
        createPanel = GameObject.Find("CreatePanel");
        sliderTemps = GameObject.Find("sliderTempsMax"); //récupération slider temps

        textTemps = GameObject.Find("textTemps");

        tempsPartie = (int)sliderTemps.GetComponent<Slider>().value;

        textTemps.GetComponent<Text>().text = tempsPartie.ToString();

        createPanel.SetActive(false); //Masquage du panel de creation du serveur
        matchmaking.SetActive(false); //Masquage du matchmaking

        PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "My Player name");
        GameObject.Find("Username").GetComponent<InputField>().text = PhotonNetwork.player.name;

        if(!PhotonNetwork.connected)
        Connect();
	}
	
    void OnDestroy()
    {
        PlayerPrefs.SetString("Username", PhotonNetwork.player.name);
    }

    void Connect()
    {
        PhotonNetwork.ConnectUsingSettings(Version);
    }

    public void usernameChanged(string name)
    {
        username = name;
        PhotonNetwork.player.name = username;
        Debug.Log(username);
    }

    public void gameNameChanged(string name)
    {
        gameName = name;
    }

    public void gamePrivateChanged(bool enabled)
    {
        gamePrivate = !enabled;
    }

    public void gameModeChanged(int index)
    {
        Text mode = GameObject.Find("gameMode").GetComponent<Dropdown>().captionText;
        string nom = mode.text;
        Debug.Log(nom);
    }

    public void joinRoom()
    {
        //Connexion à la partie
        PhotonNetwork.JoinRoom(gameName);
    }

    public void createRoom()
    {
        //Créer la partie
        RoomOptions roomOptions = new RoomOptions() { isVisible = gamePrivate, maxPlayers = 4 }; //Déclaration de la piece
        roomOptions.customRoomProperties = new ExitGames.Client.Photon.Hashtable();
        roomOptions.customRoomProperties.Add("Mode", gameMode); //Option pour le mode de jeu
        roomOptions.customRoomProperties.Add("Map", ""); //Option pour la map
        roomOptions.customRoomProperties.Add("Temps", tempsPartie); //Option pour le temps Maximum de la partie

        roomOptions.customRoomPropertiesForLobby = new string[]
        {
            "Mode",
            "Map",
            "Temps"
        };

        PhotonNetwork.CreateRoom(gameName, roomOptions, TypedLobby.Default);
    }


    public void tempsPartieChanged(int temps)
    {
        tempsPartie = (int)sliderTemps.GetComponent<Slider>().value;

        textTemps.GetComponent<Text>().text = tempsPartie.ToString();

    }

     
	// Update is called once per frame
	void Update () {

        if(PhotonNetwork.insideLobby)
        {
            foreach (RoomInfo game in PhotonNetwork.GetRoomList())
            {
                Debug.Log(game.name + " " + game.playerCount + "/" + game.maxPlayers + " " + game.customProperties["Mode"] + " " + game.customProperties["Map"]);
                
            }
        }
	}

    void OnJoinedRoom()
    {
        Debug.Log("Rejoins une salle");
        PhotonNetwork.LoadLevel(1);
    }

}


