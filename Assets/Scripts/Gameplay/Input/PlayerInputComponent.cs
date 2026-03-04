using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInputComponent : InputComponent
{

    public bool AttackInput { get; private set; }
    public bool ToggleInventory { get; private set; }
    public bool ESCInput { get; private set; }

    // Mouse Input callbacks
    public Action<int> onMouseButtonDown;
    public Action<int> onMouseButtonHeld;
    public Action<int> onMouseButtonUp;
    public Action<float> onMouseScroll;
    public Action<int> onNumberKeyDown;
    public Action onAttackInput;
    
    // Keyboard Input callbacks
    public Action onToggleInventoryInput;
    public Action onESCInput;

    // Poll for player input every frame
    void Update()
    {
        AttackInput = Input.GetButtonDown("Fire1");
        ToggleInventory = Input.GetKeyDown(KeyCode.B);
        ESCInput = Input.GetKeyDown(KeyCode.Escape);

        if(Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            onMouseScroll?.Invoke(Input.GetAxis("Mouse ScrollWheel"));
        }

        if(Input.GetMouseButtonDown(0))
        {
            onMouseButtonDown?.Invoke(0);
        }
        if(Input.GetMouseButtonDown(1))
        {
            onMouseButtonDown?.Invoke(1);
        }
        if(Input.GetMouseButtonUp(0))
        {
            onMouseButtonUp?.Invoke(0);
        }
        if(Input.GetMouseButtonUp(1))
        {
            onMouseButtonUp?.Invoke(1);
        }
        if(Input.GetMouseButton(0))
        {
            onMouseButtonHeld?.Invoke(0);
        }
        if(Input.GetMouseButton(1))
        {
            onMouseButtonHeld?.Invoke(1);
        }

        if(AttackInput)
        {
            onAttackInput?.Invoke();
        }

        if(ToggleInventory)
        {
            onToggleInventoryInput?.Invoke();
        }

        if(ESCInput)
        {
            onESCInput?.Invoke();
        }

        int numberKey = GetNumberKeyDown();
        if(numberKey != -1)
        {
            onNumberKeyDown?.Invoke(numberKey);
        }

    }

    private int GetNumberKeyDown()
    {
        for(int i = 0; i <= 9; i++)
        {
            if(Input.GetKeyDown(i.ToString()))
            {
                return i;
            }
        }
        return -1; // No number key is pressed
    }
}
