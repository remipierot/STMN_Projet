using UnityEngine;
using System.Collections;

public class PlayerNetwork : MonoBehaviour {
    const string VERSION = "v0.1"; 				//Version du jeu
    public string RoomName = "Test", 			//Nom de la salle par défaut
				  PlayerPrefabName = "Megaman"; //Le nom du prefab des joueurs qui apparaissent
	public GameObject SpawnPoint; 				//Le point d'apparition des joueurs

    void Start()
    {
        Debug.Log("Connect using settings");
        PhotonNetwork.ConnectUsingSettings(VERSION);
    }

    void OnJoinedLobby()
    {
        Debug.Log("Lobby joined");

		//Déclaration de la pièce
        RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 4 }; 
        PhotonNetwork.JoinOrCreateRoom(RoomName, roomOptions, TypedLobby.Default);
    }
    
    void OnJoinedRoom()
    {
		//Instancie le joueur quand il arrive dans la pièce
        PhotonNetwork.Instantiate(PlayerPrefabName, SpawnPoint.transform.position, SpawnPoint.transform.rotation, 0); 
        Debug.Log("Connecté à la partie");
    }
}
