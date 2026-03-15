using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;
using UnityEngine.UI;
using System;

public class PlayerInventoryUIController : InventoryUIController
{
    PlayerController pc;
    // UI references
    private Button inventoryToggleButton;

    // <old money, new money>
    public Action<int, int> onPlayerMoneyChanged;


    public override void Start()
    {
        // Find the player's inventory component.
        foreach(var invComp in InventorySubsystem.Instance.registeredInventoryComponents)
        {
            if(invComp.InventoryType == InventoryType.Player)
            {
                inventoryComponent = invComp;
                pc = (inventoryComponent as PlayerInventoryComponent).pc;
                break;
            }
        }

        // Bind the money change event to update the UI.
        if(pc != null)
        {
            pc.onPlayerMoneyChanged += (oldMoney, newMoney) => onPlayerMoneyChanged?.Invoke(oldMoney, newMoney);
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
