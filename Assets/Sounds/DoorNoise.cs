using System;
using Unity.VisualScripting;
using UnityEngine;

public class DoorNoise : StateMachineBehaviour
{

    private AudioSource[] doorSounds;
    private AudioSource openingSound;
    private AudioSource closingSound;
    

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
