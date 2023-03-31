using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomCapController : MonoBehaviour
{
    [SerializeField] ShieldKnightController sk;
    TargetSelector ts;

    [Header("Throw")]
    public float throwForce;
    public GameObject target;
    TrailRenderer trail;
    public bool thrown, canThrow, hasTarget;

    [Header("Recall")]
    Transform shieldHoldPos;
    Quaternion shieldHoldRot;
    float lerpTime = 1f;
    [SerializeField] MeshCollider meshCol;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
