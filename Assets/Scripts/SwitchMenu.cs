using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchMenu : MonoBehaviour
{
    public static SwitchMenu inputs;

    public bool switching {  get; private set; }

    private PlayerInput Press;
    private InputAction switcher;

    private void Awake()
    {
        if(inputs == null)
        {
            inputs = this;
        }

        Press = GetComponent<PlayerInput>();
        switcher = Press.actions["Open and Close"];
    }

    private void Update()
    {
        switching = switcher.WasPressedThisFrame();
    }
}
