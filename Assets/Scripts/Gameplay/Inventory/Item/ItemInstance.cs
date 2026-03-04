using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventorySystem
{
    [System.Serializable]
    public class ItemInstance
    {
        [SerializeField] private ItemDefinition itemDefinition;

        public int stackCount = 0;

        public ItemDefinition ItemDefinition
        {
            get => itemDefinition;

            set
            {
                itemDefinition = value;
            }
        }

        public ItemInstance()
        {
            this.ItemDefinition = new ItemDefinition();
            this.stackCount = 0;
        }
        public ItemInstance(ItemDefinition itemDefinition, int stackCount)
        {
            this.itemDefinition = itemDefinition;
            this.stackCount = stackCount;
        }

        public ItemInstance(ItemInstance other)
        {
            this.itemDefinition = other.itemDefinition;
            this.stackCount = other.stackCount;
        }
    }
    
}