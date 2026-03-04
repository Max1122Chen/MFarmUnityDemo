using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    public class DropppedItem : MonoBehaviour
    {
        public int itemID;
        private ItemDefinition itemDefinition;
        public int itemCount = -1;
        private SpriteRenderer spriteRenderer;
        private BoxCollider2D boxCollider;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            boxCollider = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            if(itemID != 0)
            {
                Initialize(itemID);
            }
        }

        public void Initialize(int ID)
        {
            itemID = ID;
            itemDefinition = InventorySubsystem.Instance.GetItemDefinition(itemID);
            
            if(itemDefinition != null)
            {
                spriteRenderer.sprite = itemDefinition.itemInWorldSprite == null ? itemDefinition.itemIcon : itemDefinition.itemInWorldSprite;
                Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                boxCollider.size = spriteSize;
                boxCollider.offset = new Vector2(0, spriteRenderer.sprite.bounds.center.y);
            }
            else
            {
                Debug.LogError("ItemDefinition not found for itemID: " + itemID);
            }

            GameMapSubsystem.Instance.RegisterDroppedItem(this);
        }

        public void OnDestroy()
        {
            GameMapSubsystem.Instance.UnregisterDroppedItem(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.CompareTag("Player"))
            {
                if(itemCount <= 0) return;      // In case DroppedItem has not been destroy after itemCount reaches 0

                itemCount = other.GetComponent<InventoryComponent>().TryAddItem(itemDefinition, itemCount);
                if(itemCount == 0)
                {
                    this.GetComponent<Collider2D>().enabled = false;   // Disable collider to prevent multiple pickups before destruction
                    Destroy(gameObject);
                }
            }
        }
    }

}
