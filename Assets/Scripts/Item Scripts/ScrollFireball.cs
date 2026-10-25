using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scroll_Fireball : UsableItem
{
    [SerializeField] private GameObject fireBall_prefab;
    [SerializeField] private float distanceFromObject = 1f;
    [SerializeField] private float fireballSpeed = 0;

    //public override void OnUseItem(InputValue value)
    //{
    //    UseItem();
    //}


    public override void SetHeld(bool isHeld)
    {
        base.SetHeld(isHeld);
        Debug.Log(name + " " + isHeld + "<--Scroll");
    }
    public override void UseItem()
    {
        Vector3 forceDirection = GameManager.Player.transform.forward;

        GameObject fireball = Instantiate(fireBall_prefab, transform.position + (transform.forward * distanceFromObject), Quaternion.identity);
        //fireball.transform.SetParent(transform, true);
        Rigidbody rb = fireball.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * fireballSpeed, ForceMode.Impulse);


        Debug.Log("Use Fireball");

    }
}
