using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class UsableItem : Item
{

    [SerializeField] InputActionAsset inputMap = null;
    private InputAction useItem = new InputAction();

    //public virtual void OnUseItem(InputValue value)
    //{
    //    UseItem();
    //}

    private void OnEnable()
    {
        inputMap.FindAction("UseItem").Enable();
    }
    private void OnDisable()
    {
        inputMap.FindAction("UseItem").Disable();
    }
    protected override void Awake()
    {
        base.Awake();
        useItem = inputMap.FindAction("UseItem");
    }


    public override void SetHeld(bool isHeld)
    {
        base.SetHeld(isHeld);
        Debug.Log(name + " " + isHeld + "<--UsableItem");
    }
    public override void UseItem()
    {
        Debug.Log(name + " Use Item <--UsableItem");

    }



}
