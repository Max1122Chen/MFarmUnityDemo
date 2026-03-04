using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemInstance itemInstance;
        public int slotIndex; 

        public InventorySlot()
        {
            itemInstance = new ItemInstance();
        }
        public InventorySlot(ItemInstance itemInstance, int slotIndex)
        {
            this.itemInstance = itemInstance;
            this.slotIndex = slotIndex;
        }

        public InventorySlot(int slotIndex)
        {
            this.slotIndex = slotIndex;
            itemInstance = new ItemInstance();
        }
    }

    [CreateAssetMenu(fileName = "Inventory_SO", menuName = "Inventory/Inventory_SO")]
    public class Inventory_SO : ScriptableObject
    {
        public int inventorySize;
        public List<InventorySlot> slots;

        public void Initialize(int newSize)
         {
             if (slots == null)
             {
                 slots = new List<InventorySlot>(newSize);
             }
             if (slots.Count < newSize)
             {
                 for (int i = slots.Count; i < newSize; i++)
                 {
                     InventorySlot newSlot = new InventorySlot();
                     newSlot.slotIndex = i;
                     slots.Add(newSlot);
                 }
             }
             inventorySize = newSize;   
        }

        public void ClearInventory()
        {
            slots = new List<InventorySlot>();
            for (int i = 0; i < inventorySize; i++)
            {
                InventorySlot newSlot = new InventorySlot();
                newSlot.slotIndex = i;
                slots.Add(newSlot);
            }
        }

    }
}