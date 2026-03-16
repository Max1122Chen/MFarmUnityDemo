
using UnityEngine;

namespace InventorySystem
{
    [System.Serializable]
    public enum ItemType
    {
        None,
        Any,    // A special type that can be used to indicate that an item can be used as "any" type in certain contexts (e.g. a recipe that accepts any type of material). This type should not be assigned to any actual item.
        Material,

        Seed,
        Placable,

        // Tools
        // For more complex situations, we might want to have a separate ToolType enum and have itemType be Tool, but for now this is simpler.
        Hoe,
        Axe,
        Pickaxe,
        WateringCan,
        Sickle

    }

    [System.Serializable]
    public class ItemDefinition
    {
        public int itemID = -1; // -1 is invalid ID
        public string itemName;
        public int maxStackCount = 1;
        public int value = 0;
        public ItemType itemType;
        public string itemDescription;
        public string itemIconKey;
        public string itemInWorldSpriteKey;
        public string heldSpriteKey;
        public Sprite itemIcon;
        public Sprite itemInWorldSprite;
        public Sprite heldSprite;   // Optional sprite to use when the item is held by the player.
        

        public ItemDefinition(int id, string name, int maxStack, int value, ItemType type, Sprite icon, Sprite worldSprite, string description)
        {
            itemID = id;
            itemName = name;
            maxStackCount = maxStack;
            this.value = value;
            itemType = type;
            itemIcon = icon;
            itemInWorldSprite = worldSprite;
            itemDescription = description;
        }

        public ItemDefinition()
        {
            itemID = -1;
            itemName = "Invalid Item";
            maxStackCount = 1;
            value = 0;
            itemType = ItemType.Material;
            itemIcon = null;
            itemInWorldSprite = null;
            itemDescription = "This is an invalid item.";
        }

        public bool IsValidItem()
        {
            return itemID > 0;
        }

        public bool IsHoldable()
        {
            return itemType == ItemType.Hoe || itemType == ItemType.Axe || itemType == ItemType.Pickaxe || itemType == ItemType.WateringCan || itemType == ItemType.Sickle;
        }

    }
}


