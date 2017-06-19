using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chase : MonoBehaviour {

    public Transform player;
    public Transform head;
    Animator anim;
    bool pursuing = false;
    bool patrol = true;
    // Use this for initialization

   // string state = "patrol";
    public GameObject[] waypoints;
    int currentWP = 0;
    public float rotSpeed = 0.2f;
   public float speed = 1.5f;
    float accuracyWP = 0.1f;

	void Start () {
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 direction = player.position - this.transform.position;
        direction.y = 0;
        float angle = Vector3.Angle(direction, head.up);

        if(patrol && waypoints.Length > 0)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalking", true);

            if(Vector3.Distance(waypoints[currentWP].transform.position, transform.position) < accuracyWP)
            {

                /*currentWP++;
            if(currentWP >= waypoints.Length)
                {
                    currentWP = 0;
                }*/

                currentWP = Random.Range(0, waypoints.Length);
            }

            direction = waypoints[currentWP].transform.position - transform.position;
            this.transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction), rotSpeed * Time.deltaTime);
            this.transform.Translate(0, 0, Time.deltaTime * speed);
        }

        if (Vector3.Distance(player.position, this.transform.position) < 10 && angle < 30 || pursuing) //state == "pursuing")

        {
            // state = "pursuing";
           // patrol = false;
            pursuing = true;
            Debug.Log("patrol" + patrol + "pursuing" + pursuing);
           // Vector3 direction = player.position - this.transform.position;
            //direction.y = 0;
            this.transform.rotation 
                = Quaternion.Slerp(this.transform.rotation,
                Quaternion.LookRotation(direction), rotSpeed * Time.deltaTime);

            anim.SetBool("isIdle", false);
            if(direction.magnitude > 5)
            {
                this.transform.Translate(0, 0, 0.05f);
                anim.SetBool("isWalking", true);
                anim.SetBool("isAttacking", false);
            }
            else
            {
                anim.SetBool("isAttacking", true);
                anim.SetBool("isWalking", false);
            }
        }
        else
        {
            anim.SetBool("isIdle", true);
            anim.SetBool("isWalking", true);
            anim.SetBool("isAttacking", false);
            // state = "patrol";
            pursuing = false;
           // patrol = true;

        //    Debug.Log("pursuing" + pursuing + "patrol" + patrol);
        }
	}
}
