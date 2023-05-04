using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Animator anim;
    [SerializeField] string currentState;

    [Header("Standard Animations")]
    public string idle = "ShieldKnight_Idle";
    public string move = "ShieldKnight_Move";
    public string jump = "ShieldKnight_Jump";
    public string fall = "ShieldKnight_Fall";
    public string throwing = "ShieldKnight_Throw";
    public string barge = "ShieldKnight_Barge";
    public string guard = "ShieldKnight_Guard";
    public string parry = "ShieldKnight_Parry";
    public string slam = "ShieldKnight_Slam";

    [Header("Scrap Bag Animations")]
    public string scrapBagAim = "ScrapBag_Aim";
    public string scrapBagFloat = "ScrapBag_Float";


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
