using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    public enum DroppedItemSource
    {
        FromWorld,  // e.g. dropped by resources, enemies, found in the world, etc.
        FromInventory,
        FromSaveData,   // when loading save data
        FromGameDesign  // designers desperate design
    }

    public class DropppedItem : MonoBehaviour
    {
        public int itemID;
        private ItemDefinition itemDefinition;
        public int itemCount = -1;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private string ANIM_PRAMETER_DROPPING = "Dropping";
        private BoxCollider2D boxCollider;
        private CircleCollider2D collectableRangeTrigger;

        [Header("Pick up Settings")]
        [SerializeField] private float pickupCD;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
            boxCollider = GetComponent<BoxCollider2D>();
            collectableRangeTrigger = GetComponent<CircleCollider2D>();
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

        public void ScanForPlayerWhenDropping()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
            foreach(Collider2D collider in colliders)
            {
                OnTriggerCovering(collider);
            }
        }

        public void PlayDroppingAnim()
        {
            animator.SetTrigger(ANIM_PRAMETER_DROPPING);

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
            OnTriggerCovering(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            OnTriggerCovering(other);
        }

        private void OnTriggerCovering(Collider2D other)
        {
            if(other.CompareTag("CollectingRange"))
            {
                if(pickupCD > 0f)
                {
                    return; // Still in pickup cooldown, cannot be picked up
                }

                if(Vector2.Distance(transform.position, other.transform.position) >= 0.5f) // Check if player is close enough to pick up the item
                {
                    StartCoroutine(FlyToPlayerCoroutine(other.gameObject));
                }
                else
                {
                    TryAddItemToPlayer(other.gameObject);
                }
            }
        }

        private IEnumerator FlyToPlayerCoroutine(GameObject player)
        {
            Vector2 playerPos = player.transform.position;

            while(Vector2.Distance(transform.position, playerPos) > 0.1f)
            {
                playerPos = player.transform.position; // Update player position in case player is moving
                transform.position = Vector2.MoveTowards(transform.position, playerPos, GameInstance.Instance.gameSettings.itemFlyingSpeed * Time.deltaTime);
                yield return null;
            }

            TryAddItemToPlayer(player);
        }

        private void TryAddItemToPlayer(GameObject player)
        {
            InventoryComponent inventoryComponent = player.GetComponent<InventoryComponent>();
            if(inventoryComponent == null)
            {
                inventoryComponent = player.GetComponentInParent<InventoryComponent>();
            }

            if(inventoryComponent != null)
            {
                int remainingCount = inventoryComponent.TryAddItem(itemID, itemCount);
                itemCount = remainingCount;

                if(itemCount <= 0)
                {
                    this.GetComponent<Collider2D>().enabled = false;   // Disable collider to prevent multiple pickups before destruction
                    
                    GameMapSubsystem.Instance.ReleaseDroppedItemToPool(this);
                }
            }
        }
    
        public void PickupCoolingDown()
        {
            StartCoroutine(PickupCoolingDownCoroutine());
        }

        private IEnumerator PickupCoolingDownCoroutine()
        {
            while(pickupCD > 0f)
            {
                pickupCD -= Time.deltaTime;
                yield return null;
            }

            ScanForPlayerWhenDropping(); // After cooldown, immediately check if player is in range to pick up the item
        }


        public void ResetPickupCD(DroppedItemSource source)
        {
            switch(source)
            {
                case DroppedItemSource.FromWorld:
                    pickupCD = GameInstance.Instance.gameSettings.pickupCD_FromWorld;
                    break;
                case DroppedItemSource.FromInventory:
                    pickupCD = GameInstance.Instance.gameSettings.pickupCD_FromInventory; 
                    break;
                default:
                    pickupCD = 0;
                    break;
            }
        }
    }

}
