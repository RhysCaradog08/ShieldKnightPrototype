using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basics.ObjectPool;

public class Turret : MonoBehaviour
{
    GameObject player;
    Vector3 lookPos;

    Animator anim;

    public Transform shootpoint;
    public float shotForce;
    float shotDelay;

    public float range;

    GameObject cannonBall;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        lookPos = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
        transform.LookAt(lookPos);

        shotDelay -= Time.deltaTime;

        float distToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (distToPlayer <= range && shotDelay <= 0)
        {
            anim.SetTrigger("Fire");
            shotDelay = 3.5f;
        }
    }

    void Shoot()
    {
        cannonBall = ObjectPoolManager.instance.CallObject("Projectile", null, shootpoint.position, Quaternion.identity, 3);

        Rigidbody projRb = cannonBall.GetComponent<Rigidbody>();

        projRb.velocity = Vector3.zero;
        projRb.angularVelocity = Vector3.zero;

        projRb.AddForce(transform.forward * shotForce, ForceMode.Impulse);
    }
}
