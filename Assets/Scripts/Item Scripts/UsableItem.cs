using UnityEngine;
using UnityEngine.InputSystem;

public class UsableItem : Item
{

    [SerializeField] InputActionAsset inputMap = null;
    private InputAction useItem = new InputAction();
     [SerializeField] protected int numOfUses = 1;

    [Header("Item Sound Effects")]
    [SerializeField] protected AudioSource itemUseSound = null;

    //public virtual void OnUseItem(InputValue value)
    //{
    //    UseItem();
    //}

    private void OnEnable()
    {
        inputMap.FindAction("UseItem").Enable();
    }


    public override void SetHeld(bool isHeld)
    {
        base.SetHeld(isHeld);
        Debug.Log(name + " " + isHeld + "<--UsableItem");
    }
    public override void UseItem()
    {
        if (itemUseSound != null)
        {
            itemUseSound.Play();
        }
        Debug.Log(name + " Use Item <--UsableItem");

    }



}
