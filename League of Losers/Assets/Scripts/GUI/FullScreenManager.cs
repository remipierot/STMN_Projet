using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FullScreenManager : MonoBehaviour {
    Toggle FSToggle;
	// Use this for initialization
	void Start () {
        FSToggle = GetComponent<Toggle>();
        FSToggle.isOn = Screen.fullScreen;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OptionsFullScreen()
    {
        Resolution cResol = Screen.currentResolution;
        if (FSToggle.isOn)
            Screen.SetResolution(cResol.width, cResol.height, true);
        else
            Screen.SetResolution(cResol.width, cResol.height, false);

    }
}
