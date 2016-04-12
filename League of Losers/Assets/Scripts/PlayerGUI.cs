using UnityEngine;
using System.Collections;

public class PlayerGUI : MonoBehaviour {

    public PlayerControllerScript playerController;
    private Renderer curseu;
    private Renderer vie1;
    private Renderer vie2;
    private Renderer vie3;
    
    // Use this for initialization
    void Start () {
        playerController = GetComponent<PlayerControllerScript>();
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r.name == "curseur")
            {
                curseu = r;
            }
            else if (r.name == "vie1")
            {
                vie1 = r;
            }
            else if (r.name == "vie2")
            {
                vie2 = r;
            }
            else if (r.name == "vie3")
            {
                vie3 = r;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        if(!playerController.m_PhotonView.isMine)
        {
            curseu.enabled = false;
            switch(playerController.m_Lives)
            {
                case 1:
                    vie2.enabled = false;
                    break;
                case 2:
                    vie3.enabled = false;
                    break;

                default:
                    break;
            }
        }

        switch (playerController.m_Lives)
        {
            case 1:
                vie2.enabled = false;
                break;
            case 2:
                vie3.enabled = false;
                break;

            default:
                break;
        }
    }
}
