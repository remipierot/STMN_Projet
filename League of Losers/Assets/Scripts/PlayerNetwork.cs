using UnityEngine;
using System.Collections;

public class PlayerNetwork : MonoBehaviour {
    const string VERSION = "v0.1"; //version du jeu
    public string roomName = "Test"; //Nom de la salle par défaut
    public string playerPrefabName = "Megaman"; //Le nom du prefab des joueurs qui apparaissent
    public GameObject spawnPoint; // Le point d'apparition des joueurs

    void Start()
    {
        Debug.Log("Connect using settings");
        PhotonNetwork.ConnectUsingSettings(VERSION);
    }

    void OnJoinedLobby()
    {
        Debug.Log("Lobby joined");
        RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 4 }; //Déclaration de la piece
        PhotonNetwork.JoinOrCreateRoom(roomName,roomOptions,TypedLobby.Default);
    }
    
    void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate(playerPrefabName, spawnPoint.transform.position, spawnPoint.transform.rotation, 0); //Instancie le joueur quand il arrive dans la pièce
        Debug.Log("Connecté à la partie");
    }
}
