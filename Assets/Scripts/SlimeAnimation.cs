using UnityEngine;
using UnityEngine.AI;

public class SlimeAnimation : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    [SerializeField] private AudioSource MovingSoundSource = null;
    [SerializeField] private AudioClip movingSound = null;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponentInChildren<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if(animator == null || agent == null) return;

        bool isMoving = agent.velocity.magnitude > 0.05f;

        if(isMoving )
        {
            PlaySound();
        }
        
        animator.SetBool("IsMoving", isMoving);
    }

    public void PlayAttack()
    {
        if(animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
    public void StopAttack()
    {
        
    }

    private void PlaySound()
    {
        if(MovingSoundSource != null && movingSound != null)
        {
            if (!MovingSoundSource.isPlaying)
            {
                MovingSoundSource.clip = movingSound;
                MovingSoundSource.Play();
            }
        }
        else if (MovingSoundSource != null && MovingSoundSource.isPlaying)
        {
            MovingSoundSource.Stop();
        }
    }
}
