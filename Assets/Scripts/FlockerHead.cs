using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class FlockerHead : MonoBehaviour
{

    public float speed = 0.1f;
    public float rotationSpeed = 4.0f;
    Vector3 averageHeading;
    Vector3 averagePosition;
    public float neighborDistance = 1.0f;

    bool turning = false;
    // Use this for initialization
    void Start()
    {
        speed = Random.Range(0.5f, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, Vector3.zero) >= FollowFlock.tankSize)
        {
            turning = true;
        }
        else
        {
            turning = false;
        }

        if (turning)
        {
            Vector3 direction = Vector3.zero - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction),
                rotationSpeed * Time.deltaTime);
            speed = Random.Range(0.5f, 1);
        }
        else
        {



            if (Random.Range(0, 5) < 1)
                ApplyRules();
        }
        transform.Translate(0, 0, Time.deltaTime * speed);
    }

    public void changeMaterial()
    {
        this.gameObject.GetComponent<Renderer>().sharedMaterial.color = Color.red;
        Debug.Log("Change Material has been triggered to Red");

    }

   public void ApplyRules()
    {
        GameObject[] gos;
        gos = FollowFlock.allFlock;

        Vector3 vCenter = Vector3.zero;
        Vector3 vAvoid = Vector3.zero;
        float gSpeed = 0.1f;

        Vector3 goalPos = FollowFlock.goalPos;


        float dist;

        int groupSize = 0;

        foreach (GameObject go in gos)
        {
            if (go != this.gameObject)
            {
                dist = Vector3.Distance(go.transform.position, this.transform.position);

                if (dist <= neighborDistance)
                {
                    vCenter += go.transform.position;
                    groupSize++;

                    if (dist < 1.0f)
                    {
                        vAvoid = vAvoid + (this.transform.position - go.transform.position);
                    }

                    FlockerHead anotherFlock = go.GetComponent<FlockerHead>();
                    gSpeed = gSpeed + anotherFlock.speed;
                }
            }

        }

        if (groupSize > 0)
        {
            vCenter = vCenter / groupSize + (goalPos - this.transform.position);
            speed = gSpeed / groupSize;

            Vector3 direction = (vCenter + vAvoid) - transform.position;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
        }
    }
}
