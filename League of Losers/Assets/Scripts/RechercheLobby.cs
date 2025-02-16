﻿
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Classe de recherche et de connexion au lobby pour tout nos test
/// Cette classe est la première version de la connexion avant l'intégration
/// de la selection du personnage et de l'arene
/// </summary>
public class RechercheLobby : MonoBehaviour
{

    private bool spawn = false;
    private int maxPlayer = 1;
    public GameObject SpawnSpot;
    private Room[] game;
    private string roomName = "Test";
    bool connecting = false;
    List<string> chatMessages;
    int maxChatMessages = 5;
    private string maxPlayerString = "4";
    public string Version = "v0.1";
    private Vector3 up;
    private Vector2 scrollPosition;
    private bool seConnecter = false;
    public bool useKnightman = true;

    void Start()
    {
        PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "My Player name");
        chatMessages = new List<string>();
        Connect();
    }

    void OnDestroy()
    {
        PlayerPrefs.SetString("Username", PhotonNetwork.player.name);
    }

    public void AddChatMessage(string m)
    {
        GetComponent<PhotonView>().RPC("AddChatMessage_RPC", PhotonTargets.AllBuffered, m);
    }

    /// <summary>
    /// Action pour récupérer des messages chat
    /// </summary>
    /// <param name="m"></param>
    [PunRPC]
    void AddChatMessage_RPC(string m)
    {
        while (chatMessages.Count >= maxChatMessages)
        {
            chatMessages.RemoveAt(0);
        }
        chatMessages.Add(m);
    }

    void Connect()
    {
        PhotonNetwork.ConnectUsingSettings(Version);///Connexion avec la version du logiciel qui est définie
    }

    /// <summary>
    /// Déclaration des GUI
    /// </summary>
    void OnGUI()
    {
        GUI.color = Color.grey;
        if (PhotonNetwork.connected == false && connecting == false)
        {
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Username: ");
            PhotonNetwork.player.name = GUILayout.TextField(PhotonNetwork.player.name);
            GUILayout.EndHorizontal();



            if (GUILayout.Button("Jouer"))
            {
                connecting = true;
                Connect();
            }

            if (GUILayout.Button("Quitter "))
            {
                Application.Quit();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        if (PhotonNetwork.connected == true && connecting == false)
        {
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            foreach (string msg in chatMessages)
            {
                GUILayout.Label(msg);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        if (PhotonNetwork.insideLobby == true)
        {

            GUI.Box(new Rect(Screen.width / 2.5f, Screen.height / 3f, 400, 550), "");
            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(Screen.width / 2.5f, Screen.height / 3, 400, 500));
            GUI.color = Color.cyan;
            GUILayout.Box("Lobby");
            GUI.color = Color.white;

            GUILayout.Label("Session Name:");
            roomName = GUILayout.TextField(roomName);
            GUILayout.Label("Max amount of players 1 - 4:");
            maxPlayerString = GUILayout.TextField(maxPlayerString, 2);
            if (maxPlayerString != "")
            {

                maxPlayer = int.Parse(maxPlayerString);

                if (maxPlayer > 4) maxPlayer = 4;
                if (maxPlayer == 0) maxPlayer = 1;
            }
            else
            {
                maxPlayer = 1;
            }

            if (GUILayout.Button("Create Room "))
            {
                if (roomName != "" && maxPlayer > 0)
                {
                    seConnecter = true;
                    RoomOptions roomOptions = new RoomOptions() { isVisible = true, maxPlayers = 4 }; //Déclaration de la piece
                    PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
                }
            }

            GUILayout.Space(20);
            GUI.color = Color.yellow;
            GUILayout.Box("Sessions Open");
            GUI.color = Color.red;
            GUILayout.Space(20);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(400), GUILayout.Height(300));

            foreach (RoomInfo game in PhotonNetwork.GetRoomList())
            {
                GUI.color = Color.green;
                GUILayout.Box(game.name + " " + game.playerCount + "/" + game.maxPlayers + " " + game.visible);
                if (GUILayout.Button("Join Session"))
                {
                    PhotonNetwork.JoinRoom(game.name);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        //Affichage du score et du nom
        if (PhotonNetwork.inRoom) //Si on est dans une salle
        {
           GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height)); //On déclare une zone de dessin GUI
           var playerList = new System.Text.StringBuilder();
           foreach (var player in PhotonNetwork.playerList) //Pour chaque joueur de la salle
           {
               GUILayout.Label("Username: ");
               GUILayout.Label(player.name); //nom
               GUILayout.Label("Score: ");
               GUILayout.Label(player.GetScore().ToString()); //score
           }
           GUILayout.EndArea();
        }
    }

    void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
    }

    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("OnPhotonRandomJoinFailed");
        PhotonNetwork.CreateRoom(null);
    }
    /// <summary>
    /// Instanciation du joueur quand il arrive dans la salle
    /// </summary>
    public void instantiate()
    {
        GameObject playerObj;
        if (useKnightman)
            playerObj = PhotonNetwork.Instantiate("Knightman", SpawnSpot.transform.position, SpawnSpot.transform.rotation, 0);
        else
            playerObj = PhotonNetwork.Instantiate("Bowman", SpawnSpot.transform.position, SpawnSpot.transform.rotation, 0);
        playerObj.GetComponent<PlayerControllerScript>().m_RespawnPoint = SpawnSpot;
    }


    void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        connecting = false;
        spawn = true;
        instantiate();
    }

    void OnLevelWasLoaded(int level)
    {
        if (level == 1)
        { //Replace 1 with what your scene number is. You can check what it is in the build settings.

        }
    }

    void Update()
    {
        if (seConnecter && spawn)
        {
            spawn = false;
            seConnecter = false;
            Debug.Log("Connecté à la partie");
        }
    }
}