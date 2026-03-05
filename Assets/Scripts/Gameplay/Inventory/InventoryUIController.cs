using System;
using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    [SerializeField] protected GameObject[] inventoryUIs;
    [SerializeField] protected InventorySlot_UI[] slotUIs;
    [SerializeField] protected InventoryComponent inventoryComponent;

    private bool firstTimeOpenInventory = true;

    public InventoryComponent InventoryComponent
    {
        get { return inventoryComponent; }
        set { inventoryComponent = value; }
    }

    void Awake()
    {
    }

    public virtual void Start()
    {
        if(inventoryComponent == null)
        {
            Debug.LogError("InventoryComponent is not assigned in InventoryController.");
        }
        else
        {
            InitializeInventorySlotsUI();
            inventoryComponent.onInventorySlotChanged += HandleInventorySlotChanged;
            inventoryComponent.onSelectedSlotIndexChanged += HandleSelectedSlotIndexChanged;
            inventoryComponent.onToggleInventory += ToggleInventoryVisibility;
        }
    }

    public virtual void OnDestroy()
    {
        if(inventoryComponent != null)
        {
            inventoryComponent.onInventorySlotChanged -= HandleInventorySlotChanged;
            inventoryComponent.onSelectedSlotIndexChanged -= HandleSelectedSlotIndexChanged;
            inventoryComponent.onToggleInventory -= ToggleInventoryVisibility;
        }
    }

    public void InitializeInventorySlotsUI()
    {
        slotUIs = GetComponentsInChildren<InventorySlot_UI>(true);

        for(int i = 0; i < inventoryComponent.InventorySize; i++)
        {
            slotUIs[i].inventoryUIController = this;

            slotUIs[i].onSlotClicked += (slotIndex) =>
            {
                inventoryComponent.SetSelectedSlotIndex(slotIndex);
            };

            InventorySlot slotData = inventoryComponent.InventorySlots[i];
            slotUIs[i].UpdateSlot(slotData);
        }
    }

    public void ToggleInventoryVisibility()
    {
        if(firstTimeOpenInventory)
        {
            firstTimeOpenInventory = false;
            RefreshAllSlotsUI();
        }

        foreach(var invUI in inventoryUIs)
        {
            invUI.SetActive(!invUI.activeSelf);
        }
    }

    private void RefreshAllSlotsUI()
    {
        for(int i = 0; i < inventoryComponent.InventorySize; i++)
        {
            slotUIs[i].Refresh();
        }
    }

    private void HandleInventorySlotChanged(InventorySlotChangedInfo changedInfo)
    {
        int slotIndex = changedInfo.slotIndex;

        if(slotIndex >= 0 && slotIndex < slotUIs.Length)     // Ensure index is valid.
        {
            InventorySlot slotData = inventoryComponent.InventorySlots[slotIndex];
            slotUIs[slotIndex].UpdateSlot(slotData);
        }
    }

    private void HandleSelectedSlotIndexChanged(int oldIndex, int newIndex)
    {
        if(oldIndex >= 0 && oldIndex < slotUIs.Length)
        {
            slotUIs[oldIndex].UpdateSelectionState(false);
        }
        if(newIndex >= 0 && newIndex < slotUIs.Length)
        {
            slotUIs[newIndex].UpdateSelectionState(true);
        }
    }

}
