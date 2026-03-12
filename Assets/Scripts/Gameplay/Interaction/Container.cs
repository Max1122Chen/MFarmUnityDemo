using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using UnityEngine;

public class Container : Interactable
{
    private InventoryComponent inventoryComponent;
    public void Awake()
    {
        inventoryComponent = GetComponent<InventoryComponent>();
        if(inventoryComponent == null)
        {
            Debug.LogError("Container is missing InventoryComponent.");
        }
    }

    public override bool Interact(GameObject interactor, int mouseButton)
    {
        if(mouseButton == 1)    // Right click to interact with container
        {
            InventorySubsystem.Instance.InteractWithContainer(interactor, inventoryComponent);
            return true;
        }
        return false;
    }

}
