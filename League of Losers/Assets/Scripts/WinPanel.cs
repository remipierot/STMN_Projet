using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WinPanel : MonoBehaviour {
    
    public Animator anim;
    public Text WinnerName;
    public Text LoserName;
    public List<Text> OtherPlayerNames;
    bool shown = false;

    // Use this for initialization
    void Start () {
        anim.SetBool("Shown", false);
    }
    
    // Update is called once per frame
    void Update () {
    }
    
    public void Clear()
    {
        Debug.Log("Panel cleared");
        WinnerName.text = "Personne";
        LoserName.text = "Personne";
        foreach (Text text in OtherPlayerNames)
            text.text = "Personne";
    }
    
    public void setWinner(string name, int score)
    {
        Debug.Log("Winner: " + name);
        WinnerName.text = name + ", " + score;
    }
    
    public void setLoser(string name, int score)
    {
        Debug.Log("Loser: " + name);
        LoserName.text = name + ", " + score;
    }
    
    public void setPlayer2(string name, int score)
    {
        OtherPlayerNames[0].text = name + ", " + score;
    }
    
    public void setPlayer3(string name, int score)
    {
        OtherPlayerNames[1].text = name + ", " + score;
    }
    
    public void show()
    {
        Debug.Log("Showing");
        anim.SetBool("Shown", true);
        shown = true;
    }
    
    public void hide()
    {
        Debug.Log("Hiding");
        anim.SetBool("Shown", false);
        shown = false;
    }
    
    public void ReturnToMainMenu()
    {
        if (shown)
            Application.LoadLevel(0);
    }
}
