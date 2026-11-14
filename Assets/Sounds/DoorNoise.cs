using System;
using Unity.VisualScripting;
using UnityEngine;

public class DoorNoise : StateMachineBehaviour
{

    private AudioSource[] doorSounds;
    private AudioSource openingSound;
    private AudioSource closingSound;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        doorSounds = animator.gameObject.GetComponents<AudioSource>();
        if (doorSounds.Length > 0)
        {
            openingSound = doorSounds[0];
        }
        if (doorSounds.Length > 1)
        {
            closingSound = doorSounds[1];
        }

        

        //animator.gameObject.TryGetComponent<AudioSource>(out openingSound);
        if (openingSound != null && stateInfo.IsTag("OpenAnimation"))
        {
            openingSound.Play();
        }
        else if (closingSound != null && stateInfo.IsTag("CloseAnimation"))
        {
            closingSound.Play();
        }
    }
}
