using UnityEngine;
using System.Collections;

public class DeathmatchCountdown : MonoBehaviour {
    public WinPanel panel;
    
	void Start () {
        StartCoroutine(PrepareCountdown());
	}
    
    IEnumerator PrepareCountdown()
    {
        Room r = PhotonNetwork.room;
        while (r == null)
        {
            // pas de PhotonRoom, on attend...
            yield return new WaitForSeconds(.2f); // cette syntaxe est complètement idiote
            r = PhotonNetwork.room;
        }
        
        if (r.customProperties.ContainsKey("Temps"))
        {
            Debug.Log("Match de " + r.customProperties["Temps"] + " minutes");
            StartCoroutine(Countdown((float)((int)r.customProperties["Temps"])));
        }
        //else
        //    StartCoroutine(Countdown(.1f));
    }
    
    IEnumerator Countdown(float minutes)
    {
        yield return new WaitForSeconds(minutes * 60f);
        Debug.Log("Fin de partie");
        
        panel.Clear();
        
        PhotonPlayer pl1 = null;
        PhotonPlayer pl2 = null;
        PhotonPlayer pl3 = null;
        PhotonPlayer pl4 = null;
        int maxScore = -1;
        int minScore = 999;
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            if (player.GetScore() > maxScore)
            {
                pl1 = player;
                maxScore = player.GetScore();
            }
            else if (player.GetScore() < minScore)
            {
                pl4 = player;
                minScore = player.GetScore();
            }
        }
        maxScore = -1;
        minScore = 999;
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            if (player == pl1 || player == pl4)
                continue;
            if (player.GetScore() > maxScore)
            {
                pl2 = player;
                maxScore = player.GetScore();
            }
            else if (player.GetScore() < minScore)
            {
                pl3 = player;
                minScore = player.GetScore();
            }
        }
        
        panel.setWinner(pl1.name, pl1.GetScore());
        if (pl4 != null)
            panel.setLoser(pl4.name, pl4.GetScore());
        if (pl2 != null)
            panel.setPlayer2(pl2.name, pl2.GetScore());
        if (pl3 != null)
            panel.setPlayer3(pl3.name, pl3.GetScore());
        
        panel.show();
        
        
        // supprime les joueurs
        foreach (GameObject playerChar in GameObject.FindGameObjectsWithTag("Player")) {
            if (playerChar.GetComponent<PlayerControllerScript>().owner != pl1)
                playerChar.GetComponent<PlayerControllerScript>().DieFinal();
        }
    }
}
