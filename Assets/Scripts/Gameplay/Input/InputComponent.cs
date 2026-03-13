using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputComponent : MonoBehaviour
{
    private List<InputActionAsset> IMCs = new List<InputActionAsset>();  // "Input Mapping Context" in unreal, "Input Action Asset" in Unity

    public void AddMappingContext(InputActionAsset imc)
    {
        if(imc == null)
        {
            Debug.LogError("InputActionAsset is null. Please assign a valid InputActionAsset.");
            return;
        }
        IMCs.Add(imc);
        imc.Enable();
    }

    public void OnEnable()
    {
        foreach(var imc in IMCs)
        {
            imc.Enable();
        }
    }

    public void OnDisable()
    {
        foreach(var imc in IMCs)
        {
            imc.Disable();
        }
    }

    public void BindAction(InputAction IA, InputActionPhase phase, Action<InputAction.CallbackContext> callback)
    {
        switch (phase)
        {
            case InputActionPhase.Started:
                IA.started += callback;
                break;
            case InputActionPhase.Performed:
                IA.performed += callback;
                break;
            case InputActionPhase.Canceled:
                IA.canceled += callback;
                break;
        }
    }
}
