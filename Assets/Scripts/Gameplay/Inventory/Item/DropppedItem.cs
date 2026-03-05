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
#if UNITY_EDITOR
            // For testing purposes in the editor, we can set the itemID in the inspector and initialize the dropped item with that itemID when the game starts. 
            // In the actual game, the itemID will be set when spawning the dropped item in the world, so this initialization will not be used.
            if(itemID != 0)
            {
                Initialize(itemID, itemCount);
            }
#endif
        }

        public void Initialize(int itemID, int count)
        {
            this.itemID = itemID;
            this.itemCount = count;
            itemDefinition = InventorySubsystem.Instance.GetItemDefinition(this.itemID);
            
            if(itemDefinition != null)
            {
                spriteRenderer.sprite = itemDefinition.itemInWorldSprite == null ? itemDefinition.itemIcon : itemDefinition.itemInWorldSprite;
                Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                boxCollider.enabled = true;
                boxCollider.size = spriteSize;
                boxCollider.offset = new Vector2(0, spriteRenderer.sprite.bounds.center.y);
                GameMapSubsystem.Instance.RegisterDroppedItem(this);
            }
            else
            {
                Debug.LogError("ItemDefinition not found for itemID: " + this.itemID);
            }


        }

        public void OnDisable()
        {
            GameMapSubsystem.Instance.UnregisterDroppedItem(this);
        }

        public void OnDestroy()
        {
            if(GameMapSubsystem.Instance != null)
            {
                GameMapSubsystem.Instance.UnregisterDroppedItem(this);
            }
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
                    
                    GameMapSubsystem.Instance.ReleaseDroppedItemToPool(this);
                }
            }
        }
    }

}
