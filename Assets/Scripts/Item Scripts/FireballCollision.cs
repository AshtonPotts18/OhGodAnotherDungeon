using UnityEngine;

public class FireballCollision : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log(gameObject + " Destroyed on Collsion");
        Destroy(gameObject);
    }
}
