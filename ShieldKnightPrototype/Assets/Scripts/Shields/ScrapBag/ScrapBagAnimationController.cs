using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapBagAnimationController : MonoBehaviour
{
    public Animator anim;
    [SerializeField] string currentState;

    //Animation States
    public string idle = "ScrapBag_Idle";
    public string suck = "ScrapBag_Suck";
    public string expel = "ScrapBag_Expel";
    public string parachute = "ScrapBag_Parachute";

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
