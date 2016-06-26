using UnityEngine;
using System.Collections;

public class DontDestroyObject : MonoBehaviour {
	void Start () {
        DontDestroyOnLoad(this.gameObject);
	}
}
