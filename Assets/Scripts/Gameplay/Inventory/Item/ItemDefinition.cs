
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
        Furniture,

        // Tools
        Hoe,
        Axe,
        Pickaxe,
        WateringCan,
        Sickle

    }

    [System.Serializable]
    public class ItemDefinition
    {
        [SerializeField] public int itemID = -1; // -1 is invalid ID
        [SerializeField] public string itemName;
        [SerializeField] public int maxStackCount = 1;
        [SerializeField] public int value = 0;
        [SerializeField] public ItemType itemType;
        [SerializeField] public Sprite itemIcon;
        [SerializeField] public Sprite itemInWorldSprite;
        [SerializeField] public string itemDescription;
        [SerializeField] public bool isPlacable = false;
        [SerializeField] public bool isHoldable = false;
        [SerializeField] public Sprite heldSprite;   // Optional sprite to use when the item is held by the player.
        

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

    }
}


