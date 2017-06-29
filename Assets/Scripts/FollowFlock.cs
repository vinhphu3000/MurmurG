using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowFlock : MonoBehaviour
{
    public enum FlockType { BIRD, INSECT, TADPOLE };
    public enum NpcState { PATROL, PURSUE, ATTACK };
    public enum FlockState { PATROL, PURSUE, ATTACK };
    [Header("Flock Manager")]
    public GameObject flockPrefab;
    public float tankSize = 5;
    public int numFlock = 20;
    public GameObject[] allFlock;
    public static Vector3 goalPos = Vector3.zero;
    public float minSpeed;
    public float maxSpeed;
    public float agentSpeed;
    public float agentRotation;
    public float agentNeighborDistance;
    public FlockType activeFlockType = FlockType.BIRD;
    public FlockState activeFlockState = FlockState.PATROL;

    [Header("NPC Target")]
    public GameObject goalPrefab;
    public Transform player;
    public Transform head;
    public NpcState activeNpcState = NpcState.PATROL;
    Animator anim;
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

        // reset NPC if too far outside of tank
        if (Vector3.Distance(transform.position, goalPrefab.transform.position) >= tankSize + 1)
            goalPrefab.transform.position = transform.position;

        if (Vector3.Distance(player.position, goalPrefab.transform.position) < npcViewRange
            && (angle < npcViewAngle || activeNpcState == NpcState.PURSUE))

        {
            if (npcDirection.magnitude > npcEngageMagnitude) activeNpcState = NpcState.PURSUE;
            else activeNpcState = NpcState.ATTACK;
        }
        else
        {
            activeNpcState = NpcState.PATROL;
        }

        switch (activeNpcState)
        {
            case NpcState.PATROL:
                {
                    anim.SetBool("isIdle", false);
                    anim.SetBool("isWalking", true);
                    anim.SetBool("isAttacking", false);
                    flockPrefab.GetComponent<Renderer>().sharedMaterial.color = Color.green;
                    activeFlockState = FlockState.PATROL;
                }
                break;

            case NpcState.PURSUE:
                {
                    flockPrefab.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;

                    goalPrefab.transform.rotation
                        = Quaternion.Slerp(goalPrefab.transform.rotation,
                        Quaternion.LookRotation(npcDirection), rotSpeed * Time.deltaTime);

                    goalPrefab.transform.Translate(0, 0, Time.deltaTime * speed);
                    anim.SetBool("isWalking", true);
                    anim.SetBool("isAttacking", false);
                    activeFlockState = FlockState.PURSUE;
                }
                break;

            case NpcState.ATTACK:
                {
                    anim.SetBool("isAttacking", true);
                    anim.SetBool("isWalking", false);
                    flockPrefab.GetComponent<FlockerHead>().changeMaterial();
                    activeFlockState = FlockState.ATTACK;
                }
                break;
        }



        //different flock types from the same flock prefab script

        switch (activeFlockType)
        {
            case FlockType.BIRD:
                {
                    for (int i = 0; i < numFlock; i++)
                    {
                        allFlock[i].GetComponent<FlockerHead>().activeFlockType = FlockerHead.FlockType.BIRD;
                    }
                }
                break;

            case FlockType.INSECT:
                {
                    for (int i = 0; i < numFlock; i++)
                    {
                        allFlock[i].GetComponent<FlockerHead>().activeFlockType = FlockerHead.FlockType.INSECT;
                    }
                }
                break;

            case FlockType.TADPOLE:
                {
                    for (int i = 0; i < numFlock; i++)
                    {
                        allFlock[i].GetComponent<FlockerHead>().activeFlockType = FlockerHead.FlockType.TADPOLE;
                    }
                }

                break;
        }

        // different global behaviors for flock prefab, perhaps integrate custom features with respect to flock types

        switch (activeFlockState)
        {
            case FlockState.PATROL:
                {

                    for (int i = 0; i < numFlock; i++)
                    {
                        allFlock[i].GetComponent<FlockerHead>().activeFlockState = FlockerHead.FlockState.PATROL;
                    }
                    if (Random.Range(0, 10000) < 50) RandomGoal();
                }
                break;

            case FlockState.PURSUE:
                {
                    goalPos = goalPrefab.transform.position;

                    for (int i = 0; i < numFlock; i++)
                    {
                        allFlock[i].GetComponent<FlockerHead>().activeFlockState = FlockerHead.FlockState.PURSUE;
                    }
                }
                break;

            case FlockState.ATTACK:
                {
                    goalPos = goalPrefab.transform.position;
                    for (int i = 0; i < numFlock; i++)
                    {
                        allFlock[i].GetComponent<FlockerHead>().activeFlockState = FlockerHead.FlockState.ATTACK;
                    }

                }
                break;
        }

    }

        /*
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
        */
    

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
