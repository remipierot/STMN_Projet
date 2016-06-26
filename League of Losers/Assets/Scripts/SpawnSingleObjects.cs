using UnityEngine;

public class SpawnSingleObjects : MonoBehaviour {
    public GameObject[] SingleObjects;
    public string SingleObjectsTag;

	void Start () {
        GameObject[] currentSingleObjects = GameObject.FindGameObjectsWithTag(SingleObjectsTag);
        bool alreadyExist;

        foreach(GameObject single in SingleObjects)
        {
            if (single != null)
            {
                alreadyExist = false;
                foreach (GameObject currentSingle in currentSingleObjects)
                {
                    if (single.name.Equals(currentSingle.name))
                    {
                        alreadyExist = true;
                        break;
                    }
                }

                if (!alreadyExist)
                {
                    Instantiate(single).name = single.name;
                }
            }
        }
            
	}
}
