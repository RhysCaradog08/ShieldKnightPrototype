using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveShieldController : MonoBehaviour
{
    PlayerController pc;
    public ParticleSystem waves, wavesFlipped;

    public bool isAttacking, isSurfing;

    private void Awake()
    {
        pc = FindObjectOfType<PlayerController>();
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


        if (isSurfing || pc.waveGuarding)
        {
            waves.Play();
            wavesFlipped.Play();
        }
        else
        {
            waves.Stop();
            wavesFlipped.Stop();
        }
    }
}
