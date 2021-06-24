using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMove : MonoBehaviour
{
    public bool dirRight;
    public float speed;

    public float limit;

    void Update()
    {
        if (dirRight)
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        else
            transform.Translate(-Vector3.right * speed * Time.deltaTime);

        if (transform.position.x >= limit)
        {
            dirRight = false;
        }

        if (transform.position.x <= -limit)
        {
            dirRight = true;
        }
    }
}
