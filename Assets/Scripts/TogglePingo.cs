using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogglePingo : MonoBehaviour
{

    public GameObject togglePingo;
    bool toggle;

    void Start()
    {

        toggle = false;
    }
    // Use this for initialization
    public void togglePingoObject()
    {
        toggle = !toggle;
        togglePingo.SetActive(toggle);

    }
}
