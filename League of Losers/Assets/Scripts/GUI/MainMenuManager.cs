﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        
                    
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Button_Play()
    {
        SceneManager.LoadScene(1);
    }

    public void Button_Quit()
    {
        Application.Quit();
    }


}
