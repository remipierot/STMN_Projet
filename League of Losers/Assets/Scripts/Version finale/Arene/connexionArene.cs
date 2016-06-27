using UnityEngine;
using System.Collections;

/// <summary>
/// Classe de connexion à l'arene
/// </summary>
public class connexionArene : MonoBehaviour {
   
    public string playerPrefabName = "Bowman"; ///Le nom du prefab des joueurs qui apparaissent
    public GameObject spawnPoint1; /// Le point d'apparition des joueurs
    public GameObject spawnPoint2; /// Le point d'apparition des joueurs
    public GameObject spawnPoint3; /// Le point d'apparition des joueurs
    public GameObject spawnPoint4; /// Le point d'apparition des joueurs

    public GameObject SpawnSpot; ///spawn du joueur
    
    int hackCounter = 0;



    /// <summary>
    ///     Use this for initialization
    ///     Récupère la propriété "Classe" du joueur pour savoir quels prefabs lancer
    ///     Attribue un point de spawn a chaque joueur
    /// </summary>
    /// <param name=""></param>
    /// <param name="DoInstanciation"></param>
    /// <returns></returns>

    void DoInstanciation () {

        Debug.Log(PhotonNetwork.player.ID);
        switch((int)PhotonNetwork.player.customProperties["Classe"])
        {
            case 1:
                playerPrefabName = "Bowman";
                break;
            case 2:
                playerPrefabName = "Knightman";
                break;
            default:
                playerPrefabName = "Bowman";
                break;
        }

        /*int place = 0;
        foreach(var player in PhotonNetwork.playerList)
        {
            if(comparePlace(player.ID,place))
            {
                
            }
        }*/

        switch(PhotonNetwork.player.ID)
        {
                
            case 1:
                {
                   GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint1.transform.position, spawnPoint1.transform.rotation, 0); //Instancie le joueur quand il arrive dans la pièce
                   playerObj.GetComponent<PlayerControllerScript>().m_RespawnPoint = spawnPoint1;
                }
                break;
            case 2:
                {
                    GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint2.transform.position, spawnPoint2.transform.rotation, 0); //Instancie le joueur quand il arrive dans la pièce
                    playerObj.GetComponent<PlayerControllerScript>().m_RespawnPoint = spawnPoint2;
                }
                break;
            case 3:
                {
                    GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint3.transform.position, spawnPoint3.transform.rotation, 0); //Instancie le joueur quand il arrive dans la pièce
                    playerObj.GetComponent<PlayerControllerScript>().m_RespawnPoint = spawnPoint3;
                }
                break;
            case 4:
                {
                    GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabName, spawnPoint4.transform.position, spawnPoint4.transform.rotation, 0); //Instancie le joueur quand il arrive dans la pièce
                    playerObj.GetComponent<PlayerControllerScript>().m_RespawnPoint = spawnPoint4;
                }
                break;
        }
	}
	
	// Update is called once per frame
	void Update () {
	    hackCounter++;
        if (hackCounter == 60)
        {
            DoInstanciation();
        }
	}

    bool comparePlace(int a, int b)
    {
        return a > b;
    }
}
