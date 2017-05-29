using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleObject : MonoBehaviour {
    public bool Toggle;
    public GameObject ToggleTarget;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        

        
	}

    public void ToggleMe()
    {
        Toggle = !Toggle;
        if (Toggle)
        {
            ToggleTarget.gameObject.SetActive(true);
             Debug.Log("Toggle Target is true");
        }
        else
        {
            ToggleTarget.gameObject.SetActive(false);
              Debug.Log("Toggle Target is false");
        }
    }
}
