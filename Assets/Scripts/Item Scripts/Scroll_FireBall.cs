using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scroll_FireBall : Item
{

    [SerializeField] InputActionMap inputMap = new InputActionMap();
    private InputAction useItem = new InputAction();

    public void OnUseItem(InputAction.CallbackContext context)
    {
        FireBall();
    }


    protected override void Awake()
    {
        base.Awake();
        useItem = inputMap.FindAction("UseItem");
    }


    public override void SetHeld(bool isHeld)
    {
        base.SetHeld(isHeld);
        Debug.Log(name + " " + isHeld + "<--ScrollFireball");
    }
    private void FireBall()
    {
        Debug.Log(name + " Use Item <--ScrollFireball");




    }



}
