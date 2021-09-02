using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnTarget : MonoBehaviour
{
    private int current = 0;
    private bool lockedOn = false;
    PlayerController playerControl;
    public GameObject[] targetLocations;
    public GameObject closest;

    private void Start()
    {
        playerControl = GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<PlayerController>();
    }


    // Update is called once per frame
    void Update()
    {
        if(closest != null && lockedOn)
        {

        }


        if(Input.GetKeyDown(KeyCode.Z))
        {
            FindClosestEnemy();
            lockedOn = !lockedOn;
        }

        if(lockedOn)
        {
            if(!closest)
            {
                lockedOn = false;
                closest = null;
            }
        }

        transform.LookAt(new Vector3(closest.transform.position.x, transform.position.y, closest.transform.position.z));
    }

    GameObject FindClosestEnemy()
    {
        targetLocations = GameObject.FindGameObjectsWithTag("Target");

        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach(GameObject go in targetLocations)
        {
            Vector3 diff = (go.transform.position - position);
            float currentDist = diff.sqrMagnitude;

            if(currentDist < distance)
            {
                closest = go;
                distance = currentDist;
            }
        }
        return closest;
    }

}
