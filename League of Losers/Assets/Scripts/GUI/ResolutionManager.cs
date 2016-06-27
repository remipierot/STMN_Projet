using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

/// <summary>
/// Classe pour gérer la résolution dans les paramètres
/// </summary>
public class ResolutionManager : MonoBehaviour {

    Dropdown dropdown;

    // Use this for initialization
    void Start() {

        dropdown = GetComponent<Dropdown>();
        foreach (Resolution res in Screen.resolutions)
        {
            Dropdown.OptionData list = new Dropdown.OptionData(res.width + " x " + res.height);
            dropdown.options.Add(list);
        }
        dropdown.value = Array.IndexOf(Screen.resolutions, Screen.currentResolution);
    }
	// Update is called once per frame
	void Update () {
        dropdown.captionText.text = Screen.resolutions[dropdown.value].width + " x " + Screen.resolutions[dropdown.value].height;
	}

    public void refreshResolution()
    {
        Screen.SetResolution(Screen.resolutions[dropdown.value].width, Screen.resolutions[dropdown.value].height, Screen.fullScreen);
    }
}
