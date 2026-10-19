using UnityEngine;
using UnityEngine.AI;

public class Detection : MonoBehaviour
{
    public Transform player;
    public Transform visionPoint;
    public Enemy_Movement enemy;
    public float viewDistance = 10f;
    public float viewAngle = 90f;

    public LayerMask obstacleMask;

    bool playerDetected = false;

    void Update()
    {
        CheckForPlayer();

        if (playerDetected)
        {
            FollowPlayer();
        }
    }

    void CheckForPlayer()
    {
        Vector3 directionToPlayer = player.position - visionPoint.position;

        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
        {
            playerDetected = false;
            enemy.isChasing = false;

            return;
        }

        float angleToPlayer = Vector3.Angle(visionPoint.forward, directionToPlayer);

        if (angleToPlayer > viewAngle / 2)
        {
            playerDetected = false;
            enemy.isChasing = false;

            return;
        }

        if (Physics.Raycast(visionPoint.position, directionToPlayer.normalized, out RaycastHit hit, distanceToPlayer, obstacleMask))
        {
            playerDetected = false;
            enemy.isChasing = false;
            return;
        }

        playerDetected = true;
        enemy.isChasing = true;

    }

    void FollowPlayer()
    {
        enemy.agent.SetDestination(player.position);
    }

   
}
