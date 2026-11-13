using UnityEngine;
using UnityEngine.AI;

public class SlimeAnimation : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;

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
}
