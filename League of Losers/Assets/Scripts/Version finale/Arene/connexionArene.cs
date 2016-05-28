using UnityEngine;
using System.Collections;

public class connexionArene : MonoBehaviour {
   
    public string playerPrefabName = "Bowman"; //Le nom du prefab des joueurs qui apparaissent
    public GameObject spawnPoint1; // Le point d'apparition des joueurs
    public GameObject spawnPoint2; // Le point d'apparition des joueurs
    public GameObject spawnPoint3; // Le point d'apparition des joueurs
    public GameObject spawnPoint4; // Le point d'apparition des joueurs

    public GameObject SpawnSpot; //spawn du joueur

    

	// Use this for initialization
	void Start () {

        Debug.Log(PhotonNetwork.player.ID);

        switch(PhotonNetwork.player.ID)
        {
                
            case 1:
                {
                    Debug.Log("INSTANCIATION 1");
                   GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint1.transform.position, spawnPoint1.transform.rotation, 0); //Instancie le joueur quand il arrive dans la pièce
                   //playerObj.GetComponent<PlayerControllerScript>().m_RespawnPoint = SpawnSpot;
                }
                break;
            case 2:
                {
                    Debug.Log("INSTANCIATION 2");
                    GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint2.transform.position, spawnPoint2.transform.rotation, 0); //Instancie le joueur quand il arrive dans la pièce
                   // playerObj.GetComponent<PlayerControllerScript>().m_RespawnPoint = SpawnSpot;
                }
                break;
            case 3:
                {
                    GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint3.transform.position, spawnPoint3.transform.rotation, 0); //Instancie le joueur quand il arrive dans la pièce
                    playerObj.GetComponent<PlayerControllerScript>().m_RespawnPoint = SpawnSpot;
                }
                break;
            case 4:
                {
                    GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint4.transform.position, spawnPoint4.transform.rotation, 0); //Instancie le joueur quand il arrive dans la pièce
                    playerObj.GetComponent<PlayerControllerScript>().m_RespawnPoint = SpawnSpot;
                }
                break;
        }
        if(PhotonNetwork.isMasterClient)
        {
            Debug.Log(PhotonNetwork.room.customProperties["Mode"]);
        }
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
}
