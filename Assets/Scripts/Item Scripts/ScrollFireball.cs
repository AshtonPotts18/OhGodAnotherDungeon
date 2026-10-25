using UnityEngine;

public class ScrollFireball : UsableItem
{
    [SerializeField] private GameObject fireBall_prefab;
    [SerializeField] private float distanceFromObject = 1f;
    [SerializeField] private float fireballSpeed = 15f;

    public override void SetHeld(bool isHeld)
    {
        base.SetHeld(isHeld);
        Debug.Log($"{name} {isHeld} <-- Scroll");
    }
    public override void UseItem()
    {
        if(fireBall_prefab == null)
        {
            Debug.LogError($"{name}: Fireball prefab is not assigned.", this);
            return;
        }

        if (GameManager.Player == null)
        {
            Debug.LogError($"{name}: GameManager.Player is not assigned.", this);
            return;
        }

        Vector3 direction = GameManager.Player.transform.forward.normalized;
        Vector3 spawnPosition = transform.position + direction * distanceFromObject;

        GameObject fireball = Instantiate(fireBall_prefab, spawnPosition, Quaternion.LookRotation(direction));

        Rigidbody rb = fireball.GetComponent<Rigidbody>();

        if(rb == null)
        {
            Debug.LogError("Fireball prefab needs a Rigidbody.", fireball);
            Destroy(fireball);
            return;
        }

        rb.AddForce(transform.forward * fireballSpeed, ForceMode.Impulse);

        //GameObject fireball = Instantiate(fireBall_prefab, transform.position + (transform.forward * distanceFromObject), Quaternion.identity);
        ////fireball.transform.SetParent(transform, true);
        //Rigidbody rb = fireball.GetComponent<Rigidbody>();
        //rb.AddForce(transform.forward * fireballSpeed, ForceMode.Impulse);


        //Debug.Log("Use Fireball");

    }
}
