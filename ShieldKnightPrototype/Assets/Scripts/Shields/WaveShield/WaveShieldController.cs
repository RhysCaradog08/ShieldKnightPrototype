using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveShieldController : MonoBehaviour
{
    PlayerController pc;
    ShieldSelect select;

    public ParticleSystem waves, wavesFlipped;

    public bool isAttacking, isSurfing;

    private void Awake()
    {
        pc = FindObjectOfType<PlayerController>();
        select = FindObjectOfType<ShieldSelect>();
    }

    private void Start()
    {
        isSurfing = false;
        isAttacking = false;
    }

    private void Update()
    {

        if (Input.GetButton("Barge") && pc.attackDelay <= 0 && !pc.waveGuarding)
        {
            isSurfing = true;
        }
        else isSurfing = false;

        if(isSurfing)
        {
            FindGrindPoints();
        }


        if (isSurfing || pc.waveGuarding)
        {
            waves.Play();
            wavesFlipped.Play();

            select.canChange = false;
        }
        else
        {
            waves.Stop();
            wavesFlipped.Stop();
            select.canChange = true;
        }
    }

    void FindGrindPoints()
    {
        Collider[] grindObjects = Physics.OverlapSphere(transform.position, 50f);

        foreach(Collider col in grindObjects)
        {
            if (col.tag == "Grind")
            {
                Debug.DrawLine(transform.position, col.transform.position, Color.red);

                float distance = Vector3.Distance(col.transform.position, transform.position);

                if (distance < 10f)
                {
                    Debug.Log("Grind Object:  " + col.name);
                    Debug.DrawLine(transform.position, col.transform.position, Color.green);


                }
            }
        }           
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(transform.position, 50f);
    }
}
