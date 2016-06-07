using UnityEngine;
using System.Collections;

public class DeathmatchCountdown : MonoBehaviour {
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
            StartCoroutine(Countdown((float)r.customProperties["Temps"]));
        }
    }
    
    IEnumerator Countdown(float minutes)
    {
        yield return new WaitForSeconds(minutes / 60f);
        Debug.Log("Fin de partie");
        // TODO
    }
}
