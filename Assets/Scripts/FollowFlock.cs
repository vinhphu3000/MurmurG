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
        if (attack)
        {
            goalPos = goalPrefab.transform.position;
            for (int i = 0; i < numFlock; i++)
            {
                allFlock[i].GetComponent<FlockerHead>().attack = true;
                allFlock[i].GetComponent<FlockerHead>().engage = false;
            }
        }

        else if(engage)
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
        goalPos = new Vector3(Random.Range(-tankSize, tankSize), Random.Range(-0, tankSize), Random.Range(-tankSize, tankSize));
        goalPrefab.transform.position = goalPos;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5F);
        Gizmos.DrawWireCube(new Vector3(0, tankSize/2 - 1.5f,0), new Vector3(tankSize * 2, tankSize, tankSize * 2));
    }
}
