using UnityEngine;
using UnityEngine.AI;

public class SkeletonAnimation : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("speed");
    private Animator skeletonAnimator;
    private NavMeshAgent agent;

    void Start()
    {
        skeletonAnimator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (skeletonAnimator == null || agent == null) return;
        //bool isMoving = agent.velocity.magnitude > 0.01f;



        //skeletonAnimator.speed = agent.velocity.magnitude;
        //agent.velocity.magnitude <= 1 ? skeletonAnimator.speed = agent.velocity.magnitude = 1: skeletonAnimator.speed = 1;

        skeletonAnimator.SetFloat("Speed", (agent.velocity.magnitude) <= 0.3f ? agent.velocity.magnitude : 0.3f);

        

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == GameManager.Player)
        {
            skeletonAnimator.SetTrigger("Attack");
            GetComponent<EnemyNoises>().OnDamage();
        }
    }



}
