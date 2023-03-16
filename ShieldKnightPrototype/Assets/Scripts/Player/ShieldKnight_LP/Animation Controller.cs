using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] string currentState;

    //Animation States
    public string idle = "ShieldKnight_Idle";
    public string move = "ShieldKnight_Move";
    public string throwing = "ShieldKnight_Throw";
    public string jump = "ShieldKnight_Jump";
    public string barge = "ShieldKnight_Barge";
    public string guard = "ShieldKnight_Guard";
    public string parry = "ShieldKnight_Parry";
    public string slam = "ShieldKnight_Slam";


    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        if (moveX != 0 || moveZ != 0)
        {
            ChangeAnimationState(move);
        }
        else
        {
            ChangeAnimationState(idle);
        }

        if (Input.GetButton("Throw"))
        {
            ChangeAnimationState(throwing);
        }

        if (Input.GetButton("Jump"))
        {
            ChangeAnimationState(jump);
        }

        if (Input.GetButton("Barge"))
        {
            ChangeAnimationState(barge);
        }

        if (Input.GetButton("Guard"))
        {
            ChangeAnimationState(guard);
        }*/
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
