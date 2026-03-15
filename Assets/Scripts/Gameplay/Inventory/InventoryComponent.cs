using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace InventorySystem
{
    public struct InventorySlotChangedInfo
    {
        public int slotIndex;
        
        public ItemInstance oldData;

        public ItemInstance newData;

        public ItemInstance OldData
        {
            get { return oldData; }
        }

        public ItemInstance NewData
        {
            get { return newData; }
        }

        public InventorySlotChangedInfo(int slotIndex, ItemInstance oldData, ItemInstance newData)
        {
            this.slotIndex = slotIndex;
            this.oldData = oldData;
            this.newData = newData;
        }
    }

    public enum InventoryType
    {
        None,
        Player,
        Container,

    }

    public class InventoryComponent : MonoBehaviour
    {
        [SerializeField] private List<InventorySlot> inventorySlots;
        public List<InventorySlot> InventorySlots
        {
            get { return inventorySlots; }
        }

        [SerializeField] private int inventorySize = 10;

        [SerializeField] private InventoryType inventoryType;

        public bool IsOpen { get; set; } = false;

        public int selectedSlotIndex = -1;   // -1 means no slot is selected
        public Action onToggleInventory;
        public Action<InventorySlotChangedInfo> onInventorySlotChanged;

        public Action<int, int> onSelectedSlotIndexChanged;

        public InventoryType InventoryType
        {
            get { return inventoryType; }
        }
        public int InventorySize
        {
            get { return inventorySize; }
        }

        public virtual void Awake()
        {
            Initialize(inventorySize);
        }

        public virtual void Start()
        {
            InventorySubsystem.Instance.RegisterInventoryComponent(this);
        }

        public void Initialize(int newSize)
        {
            inventorySlots = new List<InventorySlot>(newSize);
            for(int i = 0; i < newSize; i++)
            {
                inventorySlots.Add(new InventorySlot(i));
            }
        }

        // public void Initialize(List<ItemInstance> items)
        // {
        //     inventorySlots = new List<InventorySlot>(inventorySize);
        //     for(int i = 0; i < inventorySize; i++)
        //     {
        //         if(i < items.Count)
        //         {
        //             InventorySlot slot = new InventorySlot(i);
        //             slot.itemInstance = items[i];
        //             inventorySlots.Add(slot);
        //         }
        //         else
        //         {
        //             inventorySlots.Add(new InventorySlot(i));
        //         }
        //     }
        // }

        public virtual void OnDestroy()
        {
            if(InventorySubsystem.Instance != null)
            {
                InventorySubsystem.Instance.UnregisterInventoryComponent(this);
            }
        }

        public int TryAddItem(ItemDefinition itemDefinition, int count)
        {
            // Try to add to existing stacks first
            for(int i = 0; i < inventorySlots.Count; i++)
            {
                var slot = inventorySlots[i];

                // Check for existing stackable item
                ItemInstance itemInstance = slot.itemInstance;
                if(itemInstance != null && itemInstance.ItemDefinition != null && itemInstance.ItemDefinition.itemID == itemDefinition.itemID && itemInstance.stackCount < itemDefinition.maxStackCount)
                {
                    InventorySlotChangedInfo changeInfo = new InventorySlotChangedInfo(i, new ItemInstance(itemInstance), null);

                    int spaceLeft = itemDefinition.maxStackCount - itemInstance.stackCount;
                    int toAdd = Mathf.Min(spaceLeft, count);
                    itemInstance.stackCount += toAdd;
                    count -= toAdd;

                    changeInfo.newData = new ItemInstance(itemInstance);
                    onInventorySlotChanged?.Invoke(changeInfo);     // Notify about the change

                    if(count <= 0)
                    {
                        return 0; // All items added
                    }
                }
            }

            // Try to add to empty slots
            for(int i = 0; i < inventorySlots.Count; i++)
            {
                var slot = inventorySlots[i];

                ItemInstance itemInstance = slot.itemInstance;
                if(itemInstance.ItemDefinition == null || itemInstance.ItemDefinition.itemID == -1 || itemInstance.ItemDefinition.itemID == 0)
                {
                    InventorySlotChangedInfo changeInfo = new InventorySlotChangedInfo(i, new ItemInstance(itemInstance), null);

                    int toAdd = Mathf.Min(itemDefinition.maxStackCount, count);
                    itemInstance.ItemDefinition = itemDefinition;
                    itemInstance.stackCount = toAdd;
                    count -= toAdd;

                    changeInfo.newData = new ItemInstance(itemInstance);
                    onInventorySlotChanged?.Invoke(changeInfo);     // Notify about the change

                    if(count <= 0)
                    {
                        return 0; // All items added
                    }
                }
            }
            return count;
        }

        public int TryAddItem(int itemID, int count)
        {
            ItemDefinition itemDefinition = InventorySubsystem.Instance.GetItemDefinition(itemID);
            return TryAddItem(itemDefinition, count);
        }

        public int TryAddItem(ItemInstance itemInstance)
        {
            if(itemInstance == null || itemInstance.ItemDefinition == null)
            {
                Debug.LogError("TryAddItem: Invalid item instance.");
                return 0;
            }
            return TryAddItem(itemInstance.ItemDefinition, itemInstance.stackCount);
        }
        public int TryRemoveItem(ItemDefinition itemDefinition, int count)
        {
            for(int i = 0; i < inventorySlots.Count; i++)
            {
                var slot = inventorySlots[i];

                ItemInstance itemInstance = slot.itemInstance;
                if(itemInstance != null && itemInstance.ItemDefinition != null && itemInstance.ItemDefinition.itemID == itemDefinition.itemID)
                {
                    count = RemoveItemInSlot(count, i);

                    if(count <= 0)
                    {
                        return 0; // All items removed
                    }
                }
            }
            return count;
        }

        public int TryRemoveItem(int itemID, int count)
        {
            ItemDefinition itemDefinition = InventorySubsystem.Instance.GetItemDefinition(itemID);
            return TryRemoveItem(itemDefinition, count);
        }
        public int RemoveItemInSlot(int count, int index)
        {
            var slot = inventorySlots[index];

            InventorySlotChangedInfo changeInfo = new InventorySlotChangedInfo(index, new ItemInstance(slot.itemInstance), null);

            int toRemove = Mathf.Min(slot.itemInstance.stackCount, count);
            slot.itemInstance.stackCount -= toRemove;
            count -= toRemove;

            changeInfo.newData = new ItemInstance(slot.itemInstance);
            onInventorySlotChanged?.Invoke(changeInfo);     // Notify about the change

            return count;
            
        }

        public void RemoveAllItemsInSlot(int index)
        {
            RemoveItemInSlot(inventorySlots[index].itemInstance.stackCount, index);
        }

        public void SwapItemsBetweenSlots(int firstIndex, InventoryComponent targetInventory, int secondIndex)
        {
            if(firstIndex < 0 || firstIndex >= inventorySlots.Count || secondIndex < 0 || secondIndex >= targetInventory.inventorySlots.Count)
            {
                Debug.LogWarning("SwapItemsBetweenSlots: Invalid slot indices.");
                return;
            }

            var firstSlot = inventorySlots[firstIndex];
            var secondSlot = targetInventory.inventorySlots[secondIndex];

            // Create change info for first slot
            InventorySlotChangedInfo firstChangeInfo = new InventorySlotChangedInfo(firstIndex, new ItemInstance(firstSlot.itemInstance), null);
            // Create change info for second slot
            InventorySlotChangedInfo secondChangeInfo = new InventorySlotChangedInfo(secondIndex, new ItemInstance(secondSlot.itemInstance), null);

            // Swap the items
            var tempItem = firstSlot.itemInstance;
            firstSlot.itemInstance = secondSlot.itemInstance;
            secondSlot.itemInstance = tempItem;

            // Update change info new data
            firstChangeInfo.newData = new ItemInstance(firstSlot.itemInstance);
            secondChangeInfo.newData = new ItemInstance(secondSlot.itemInstance);

            // Invoke the change events
            onInventorySlotChanged?.Invoke(firstChangeInfo);
            targetInventory.onInventorySlotChanged?.Invoke(secondChangeInfo);
        }

        public virtual void SetSelectedSlotIndex(int newIndex)
        {
            // if newIndex is -1, it means deselect
            if(newIndex >= inventorySlots.Count)
            {
                Debug.LogError("SetSelectedSlotIndex: Invalid slot index.");
                return;
            }
            Debug.Log($"Selected slot index changed, from {selectedSlotIndex} to {newIndex}");

            int oldIndex = selectedSlotIndex;
            selectedSlotIndex = oldIndex == newIndex ? -1 : newIndex;   // Toggle selection if same index is selected, otherwise select new index
            onSelectedSlotIndexChanged?.Invoke(oldIndex, selectedSlotIndex);
        }

        // Return the available space for a given item
        public int CheckForAvailableSpaceForItem(ItemDefinition itemDefinition)
        {
            int availableSpace = 0;
            foreach(var slot in inventorySlots)
            {
                if(slot.itemInstance == null || slot.itemInstance.ItemDefinition == null || slot.itemInstance.ItemDefinition.itemID == -1)
                {
                    availableSpace += itemDefinition.maxStackCount;
                }
                else if(slot.itemInstance.ItemDefinition == itemDefinition && slot.itemInstance.stackCount < slot.itemInstance.ItemDefinition.maxStackCount)
                {
                    availableSpace += slot.itemInstance.ItemDefinition.maxStackCount - slot.itemInstance.stackCount;
                }
            }
            return availableSpace;
        }

        public int CheckForAvailableSpaceForItem(int itemID)
        {
            ItemDefinition itemDefinition = InventorySubsystem.Instance.GetItemDefinition(itemID);
            if(itemDefinition == null)
            {
                Debug.LogError($"CheckForAvailableSpaceForItem: Invalid item ID {itemID}.");
                return 0;
            }
            return CheckForAvailableSpaceForItem(itemDefinition);
        }

        // For hotkey input
        public void ToggleInventory(bool forceClose = false)
        {
            int oldSelectedIndex = selectedSlotIndex;
            selectedSlotIndex = -1;    // Deselect any selected slot when toggling inventory
            onSelectedSlotIndexChanged?.Invoke(oldSelectedIndex, -1);   // Notify about deselection
            if(forceClose)
            {
                if(IsOpen)
                {
                    IsOpen = false;
                    onToggleInventory?.Invoke();
                }
            }
            else
            {
                IsOpen = !IsOpen;
                onToggleInventory?.Invoke();
            }
        }

        // For UI button input, which should always toggle (not force close)
        public void ToggleInventory()
        {
            ToggleInventory(false);
        }

        // Save and Load
        public List<InventoryItemSaveData> GetInventorySaveData()
        {
            List<InventoryItemSaveData> inventoryData = new List<InventoryItemSaveData>();
            foreach(var slot in inventorySlots)
            {
                if(slot.itemInstance != null && slot.itemInstance.ItemDefinition != null && slot.itemInstance.ItemDefinition.itemID > 0)
                {
                    InventoryItemSaveData itemData = new InventoryItemSaveData
                    {
                        slotIndex = slot.slotIndex,
                        itemID = slot.itemInstance.ItemDefinition.itemID,
                        stackCount = slot.itemInstance.stackCount
                    };
                    inventoryData.Add(itemData);
                }
            }
            return inventoryData;
        }

        public void LoadInventoryFromSaveData(List<InventoryItemSaveData> inventoryData)
        {
            // Clear current inventory
            for(int i = 0; i < inventorySlots.Count; i++)
            {
                inventorySlots[i].itemInstance = new ItemInstance();
            }

            // Load from save data
            foreach(var itemData in inventoryData)
            {
                if(itemData.slotIndex >= 0 && itemData.slotIndex < inventorySlots.Count)
                {
                    ItemDefinition itemDefinition = InventorySubsystem.Instance.GetItemDefinition(itemData.itemID);
                    if(itemDefinition != null)
                    {
                        inventorySlots[itemData.slotIndex].itemInstance = new ItemInstance(itemDefinition, itemData.stackCount);
                        onInventorySlotChanged?.Invoke(new InventorySlotChangedInfo(itemData.slotIndex, null, new ItemInstance(inventorySlots[itemData.slotIndex].itemInstance)));
                    }
                    else
                    {
                        Debug.LogWarning($"LoadInventoryFromSaveData: Invalid item ID {itemData.itemID} in save data.");
                    }
                }
                else
                {
                    // Just ignore invalid slot index in save data, we will not load the item if the slot index is invalid.
                }
            }
        }
    }


}