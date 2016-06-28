using UnityEngine;
using System.Collections;

public class AutoUpdateSFXVolume : MonoBehaviour {

	void Awake () {
        AudioSource audio = GetComponent<AudioSource>();

        if(audio != null && SFXHandler.SFXEnabled)
        {
            audio.volume = SFXHandler.SFXVolume;
            audio.Play();
        }
	}
}
