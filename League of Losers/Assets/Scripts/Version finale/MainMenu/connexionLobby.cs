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
    private bool gamePrivate = false;


	// Use this for initialization
	void Start () {
        PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "My Player name");
        GameObject.Find("Username").GetComponent<InputField>().text = PhotonNetwork.player.name;
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
        gamePrivate = enabled;
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
        PhotonNetwork.CreateRoom(gameName, roomOptions, TypedLobby.Default);
    }


     
	// Update is called once per frame
	void Update () {

        if(PhotonNetwork.insideLobby)
        {
            foreach (RoomInfo game in PhotonNetwork.GetRoomList())
            {
                Debug.Log(game.name + " " + game.playerCount + "/" + game.maxPlayers + " " + game.visible);
            }
        }
	}

    void OnJoinedRoom()
    {
        Debug.Log("Rejoins une salle");
        SceneManager.LoadScene(1);
    }
}
