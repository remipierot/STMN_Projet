using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GUIMainMenu : MonoBehaviour {

    
    public int buttonSizeX;
    public int buttonSizeY;
    private int unitX;
    private int unitY;
    // Use this for initialization
    void Start ()
    {
        unitX = Screen.width / 64;
        unitY = Screen.height / 64;
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}


    void OnGUI()
    {
        // Make the first button (Jouer)
        if (GUI.Button(new Rect(unitX * 32 - buttonSizeX / 2, unitY * 22, buttonSizeX, buttonSizeY), "Jouer"))
        {
            SceneManager.LoadScene(1);
        }
        // Make the second button (Options)
        if (GUI.Button(new Rect(unitX * 32 - buttonSizeX / 2, unitY * 24 + buttonSizeY, buttonSizeX, buttonSizeY), "Options"))
        {
            SceneManager.LoadScene(2);
        }
        // Make the third button (Credits)
        if (GUI.Button(new Rect(unitX * 32 - buttonSizeX / 2, unitY * 26 + 2*buttonSizeY, buttonSizeX, buttonSizeY), "Credits"))
        {
            SceneManager.LoadScene(3);
        }
        // Make the fourth button (Quitter)
        if (GUI.Button(new Rect(unitX * 32 - buttonSizeX / 2, unitY * 28 + 3 * buttonSizeY, buttonSizeX, buttonSizeY), "Quitter"))
        {
            Application.Quit();
        }
    }

}
