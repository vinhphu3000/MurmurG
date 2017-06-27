using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowFlock : MonoBehaviour
{

    public GameObject flockPrefab;
    public GameObject goalPrefab;
    public float tankSize = 5;
    public bool attack = true;
    public bool engage = false;
    public bool bird = false;
    public bool insect = false;


    public int numFlock = 20;
    public GameObject[] allFlock;
    public static Vector3 goalPos = Vector3.zero;


    // Use this for initialization
    void Start()
    {

        allFlock = new GameObject[numFlock];
        for (int i = 0; i < numFlock; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-tankSize, tankSize), Random.Range(-tankSize, tankSize), Random.Range(-tankSize, tankSize));
            allFlock[i] = (GameObject)Instantiate(flockPrefab, pos, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //different flock types from the same flock prefab script
        if (bird)
        {
            for (int i = 0; i < numFlock; i++)
            {
                allFlock[i].GetComponent<FlockerHead>().bird = true;
                allFlock[i].GetComponent<FlockerHead>().insect = false;
            }
        }
        else if (insect)
        {
            for (int i = 0; i < numFlock; i++)
            {
                allFlock[i].GetComponent<FlockerHead>().bird = false;
                allFlock[i].GetComponent<FlockerHead>().insect = true;
            }
        }
        else
        {
            for (int i = 0; i < numFlock; i++)
            {
                allFlock[i].GetComponent<FlockerHead>().bird = false;
                allFlock[i].GetComponent<FlockerHead>().insect = false;
            }

        }

        // different global behaviors for flock prefab, perhaps integrate custom features with respect to flock types
        if (attack)
        {
            goalPos = goalPrefab.transform.position;
            for (int i = 0; i < numFlock; i++)
            {
                allFlock[i].GetComponent<FlockerHead>().attack = true;
                allFlock[i].GetComponent<FlockerHead>().engage = false;
            }
        }

        else if (engage)
        {
            goalPos = goalPrefab.transform.position;

            for (int i = 0; i < numFlock; i++)
            {
                allFlock[i].GetComponent<FlockerHead>().attack = false;
                allFlock[i].GetComponent<FlockerHead>().engage = true;
            }

        }

        else
        {

            for (int i = 0; i < numFlock; i++)
            {
                allFlock[i].GetComponent<FlockerHead>().attack = false;
                allFlock[i].GetComponent<FlockerHead>().engage = false;
            }
            if (Random.Range(0, 10000) < 50)
            {
                RandomGoal();
            }
        }
    }

    void RandomGoal()
    {
        goalPos = new Vector3
            (Random.Range(transform.position.x - tankSize, transform.position.x + tankSize),
            Random.Range(-1.5f, transform.position.y + tankSize),
            Random.Range(transform.position.z - tankSize, transform.position.z + tankSize));
        goalPrefab.transform.position = goalPos;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5F);
        Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y + tankSize / 2 - .75f, transform.position.z),
            new Vector3(tankSize * 2, tankSize + 1.5f, tankSize * 2));
    }
}
