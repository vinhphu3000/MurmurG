using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowFlock : MonoBehaviour
{
    [Header("Flock Manager")]
    public GameObject flockPrefab;
    public GameObject goalPrefab;
    public float tankSize = 5;
    public int numFlock = 20;
    public GameObject[] allFlock;
    public static Vector3 goalPos = Vector3.zero;
    public float minSpeed;
    public float maxSpeed;
    public float agentSpeed;
    public float agentRotation;
    public float agentNeighborDistance;

    [Header("Flock State")]
    public bool attack = true;
    public bool engage = false;

    [Header("Flock Type")]
    public bool bird = false;
    public bool insect = false;

    [Header("NPC Target")]
    public Transform player;
    public Transform head;
    Animator anim;
    string state = "patrol";
    public float rotSpeed = 0.2f;
    public float speed = 1.5f;
    public float npcViewRange;
    public float npcViewAngle;
    public float npcEngageMagnitude;

    // Use this for initialization
    void Start()
    {
        anim = goalPrefab.GetComponent<Animator>();

        allFlock = new GameObject[numFlock];
        for (int i = 0; i < numFlock; i++)
        {
            Vector3 pos = new Vector3(Random.Range(transform.position.x - tankSize, transform.position.x + tankSize), 
                Random.Range(transform.position.y - tankSize, transform.position.y + tankSize), 
                Random.Range(transform.position.z - tankSize, transform.position.z + tankSize));
            allFlock[i] = (GameObject)Instantiate(flockPrefab, pos, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 npcDirection = player.position - goalPrefab.transform.position;
        float angle = Vector3.Angle(npcDirection, head.up);

        if (state == "patrol")
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", true);
            flockPrefab.GetComponent<Renderer>().sharedMaterial.color = Color.green;
            //  flockManager.GetComponent<FollowFlock>().attack = false;
            //   flockManager.GetComponent<FollowFlock>().engage = false;
            attack = false;
            engage = false;

            
        }
        if (Vector3.Distance(transform.position, goalPrefab.transform.position) >= tankSize + 1)
            goalPrefab.transform.position = transform.position;

        if (Vector3.Distance(player.position, goalPrefab.transform.position) < npcViewRange && (angle < npcViewAngle || state == "pursuing")) //state == "pursuing")

        {
            state = "pursuing";
            flockPrefab.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;
            attack = false;
            engage = true;

            goalPrefab.transform.rotation
                = Quaternion.Slerp(goalPrefab.transform.rotation,
                Quaternion.LookRotation(npcDirection), rotSpeed * Time.deltaTime);

           // anim.SetBool("isIdle", false);
            if (npcDirection.magnitude > npcEngageMagnitude)
            {
                goalPrefab.transform.Translate(0, 0, Time.deltaTime * speed);
                anim.SetBool("isWalking", true);
                anim.SetBool("isAttacking", false);
            }
            else
            {
              anim.SetBool("isAttacking", true);
              anim.SetBool("isWalking", false);
                flockPrefab.GetComponent<FlockerHead>().changeMaterial();
                attack = true;
                engage = false;

            }
        }
        else
        {
            //   anim.SetBool("isIdle", true);
           anim.SetBool("isWalking", true);
           anim.SetBool("isAttacking", false);
            state = "patrol";
        }
    

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
