using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GUIMainMenu : MonoBehaviour {

    // Size of the elements of the GUI that can be edited in the Manager's parameters
    public int buttonSizeX;
    public int buttonSizeY;
    public int creditsWindowSizeX;
    public int creditsWindowSizeY;

    // Image contenant les crédits mis en forme
    public Texture2D creditsImage;
    
    // Position of the credits pop up window (initialised as false)
    private bool creditsWindow;

    // These units will be used to place the elements depending on the screen size
    private int unitX;
    private int unitY;
    


    // Use this for initialization
    void Start ()
    {
        creditsWindow = false;
        unitX = Screen.width / 64;
        unitY = Screen.height / 64;
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void CreditsWindow(int windowID)
    {
        GUI.DrawTexture(new Rect(creditsWindowSizeX/2 - (creditsWindowSizeX * 11 / 12) /2, creditsWindowSizeY/12, creditsWindowSizeX*11/12, creditsWindowSizeY*9/12 ),creditsImage);
        if (GUI.Button(new Rect(creditsWindowSizeX/2 - creditsWindowSizeX/6, creditsWindowSizeY- creditsWindowSizeY / 9, creditsWindowSizeX/3, creditsWindowSizeY/12), "Retour"))
            creditsWindow = false;
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
            creditsWindow = true;
        }
        // Make the fourth button (Quitter)
        if (GUI.Button(new Rect(unitX * 32 - buttonSizeX / 2, unitY * 28 + 3 * buttonSizeY, buttonSizeX, buttonSizeY), "Quitter"))
        {
            Application.Quit();
        }

        // Display the credits in a pop up window
        if (creditsWindow)
            GUI.Window(0, new Rect(unitX * 32 - creditsWindowSizeX/2, unitY * 18, creditsWindowSizeX, creditsWindowSizeY), CreditsWindow, "Credits");
    }

	
}
