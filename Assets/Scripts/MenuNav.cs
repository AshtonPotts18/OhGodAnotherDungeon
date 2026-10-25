using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assemblies;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MenuNav : MonoBehaviour
{

    public int numberofbuttons = 2;
    private int currentbutton = 0;

    [SerializeField] private InputActionAsset actionmap;
    [SerializeField] private GameObject[] mainbutton;

    [SerializeField] private float timer = 0.25f;

    [SerializeField] private float starttimer = 0.25f;

    private InputAction up;
    private InputAction down;

    private void Awake()
    {
        up = actionmap.FindAction("Up");
        down = actionmap.FindAction("Down");
    }




    void Start()
    {
        EventSystem.current.SetSelectedGameObject(mainbutton[0]);
    }

    // Update is called once per frame
    void Update()
    {
        if (up.IsPressed() && timer == 0)
        {
            if (currentbutton - 1 < 0)
            {
                currentbutton = numberofbuttons - 1;
                EventSystem.current.SetSelectedGameObject(mainbutton[currentbutton]);

            }
            else
            {
                currentbutton -= 1;
                EventSystem.current.SetSelectedGameObject(mainbutton[currentbutton]);

            }
            timer = starttimer;
        }
        else if (down.IsPressed() && timer == 0)
        {
            if (currentbutton + 1 == numberofbuttons)
            {
                currentbutton = 0;
                EventSystem.current.SetSelectedGameObject(mainbutton[currentbutton]);

            }
            else
            {
                currentbutton += 1;
                EventSystem.current.SetSelectedGameObject(mainbutton[currentbutton]);

            }
            timer = starttimer;
        }
        else
        {
            timer -= Time.deltaTime;
        }

    }
}
