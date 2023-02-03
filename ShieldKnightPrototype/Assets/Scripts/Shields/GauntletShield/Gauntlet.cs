using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gauntlet : MonoBehaviour
{
    public Transform gauntletPos;
    Quaternion gauntletRot;
    // Start is called before the first frame update
    void Awake()
    {
        gauntletPos.position = gauntletPos.position;
        gauntletRot = gauntletPos.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        gauntletPos.position = gauntletPos.position;
        gauntletRot = gauntletPos.rotation;

        transform.position = gauntletPos.position;
        transform.rotation = gauntletRot;
    }
}
