using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using UnityEngine;

public class Container : Interactable
{
    private InventoryComponent inventoryComponent;
    public override void Awake()
    {
        base.Awake();

        inventoryComponent = GetComponent<InventoryComponent>();
        if(inventoryComponent == null)
        {
            Debug.LogError("Container is missing InventoryComponent.");
        }
    }
    public override bool Interact(GameObject interactor, int mouseButton)
    {
        if(base.Interact(interactor, mouseButton) && mouseButton == 1)    // Right click to interact with container
        {
            InventorySubsystem.Instance.InteractWithContainer(interactor, inventoryComponent);
            return true;
        }
        return false;
    }

}
