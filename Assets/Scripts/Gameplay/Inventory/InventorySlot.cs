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

}