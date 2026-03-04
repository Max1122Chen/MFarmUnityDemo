using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;
using UnityEngine.UI;

public class PlayerInventoryUIController : InventoryUIController
{
    private Button inventoryToggleButton;
    public override void Start()
    {
        // Find the player's inventory component.
        foreach(var invComp in InventorySubsystem.Instance.registeredInventoryComponents)
        {
            if(invComp.InventoryType == InventoryType.Player)
            {
                inventoryComponent = invComp;
                break;
            }
        }

        // Bind the toggle button.
        inventoryToggleButton = GameObject.Find("InventoryToggleButton").GetComponent<Button>();
        if(inventoryToggleButton != null)
        {
            inventoryToggleButton.onClick.AddListener(inventoryComponent.ToggleInventory);
        }
        else
        {
            Debug.LogError("InventoryToggleButton not found in scene.");
        }

        // Bind hotbar index change to update UI selection.
        (inventoryComponent as PlayerInventoryComponent).onSelectedHotBarIndexChanged += HandleSelectedHotBarIndexChanged;

        base.Start();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if(inventoryToggleButton != null)
        {
            inventoryToggleButton.onClick.RemoveListener(inventoryComponent.ToggleInventory);
        }
        if(inventoryComponent != null)
        {
            (inventoryComponent as PlayerInventoryComponent).onSelectedHotBarIndexChanged -= HandleSelectedHotBarIndexChanged;
        }
    }

    private void HandleSelectedHotBarIndexChanged(int oldIndex, int newIndex)
    {
        if(oldIndex == newIndex) return;
        if(newIndex < 0 || newIndex >= (inventoryComponent as PlayerInventoryComponent).HotBarSize) return;   // Ensure new index is valid.
        
        if(oldIndex >= 0)   // Deselect old index if it was valid. This check prevents errors when oldIndex is -1 (no selection).
        {
            slotUIs[oldIndex].UpdateSelectionState(false);
        }
        if(newIndex >= 0)
        {
            slotUIs[newIndex].UpdateSelectionState(true);
        }
    }
}
