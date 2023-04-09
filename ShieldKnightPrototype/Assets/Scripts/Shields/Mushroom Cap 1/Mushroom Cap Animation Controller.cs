using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomCapAnimationController : MonoBehaviour
{
    public Animator anim;
    [SerializeField] string currentState;

    //Animation States
    public string idle = "MushroomCap_Idle";
    public string hit = "MushroomCap_Hit";
    public string slam = "MushroomCap_Slam";
    public string bounce = "MushroomCap_Bounce";

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {

    }

    public void ChangeAnimationState(string newState)
    {
        //Stop the animation from interrupting itself.
        if (currentState == newState) return;

        //Play the animation.
        anim.Play(newState);

        //Reassign the current state.
        newState = currentState;
    }
}
