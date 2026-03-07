using System;
using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    private PlayerInputComponent inputComponent;
    private Rigidbody2D rb;
    private PlayerAnimationBlueprint abp;
    public PlayerInventoryComponent inventoryComponent { get; private set; }

    [SerializeField] private SpriteRenderer heldItemSpriteRenderer;

    public Action onLateUpdate;

    public Action<ItemDefinition> onHeldItemChanged;

    public Action<ItemDefinition> onUseTool;

    public float toolUsingRadius = 1.5f;
    public float placingRadius = 2f;
    public float moveSpeed = 5f;

    public bool isActionDisabled = false;   // This is used to disable player actions (e.g. movement, using tools/items, etc.) in certain situations, such as during cutscenes, dialogue, etc. It can be set to true or false by other scripts based on the game state.
    public bool isUsingTool = false;
    public bool canMove = true;

    public float inputX;
    public float inputY;
    public float mouseX;
    public float mouseY;
    public Vector2 movementInput;


    void Awake()
    {
        inputComponent = GetComponent<PlayerInputComponent>();
        rb = GetComponent<Rigidbody2D>();
        abp = GetComponent<PlayerAnimationBlueprint>();
        inventoryComponent = GetComponent<PlayerInventoryComponent>();
    }

    void Start()
    {
        BindInputCallbacks();

        GameMapSubsystem.Instance.onNewSceneLoaded += (string sceneName) => {  isActionDisabled = false; };
        GameMapSubsystem.Instance.onOldSceneStartUnloading += (string sceneName) => {  isActionDisabled = true; };

        // Bind scene switch callback

        // Bind inventory hotbar index change callback to update held item info in ABP.
        inventoryComponent.onSelectedHotBarIndexChanged += HandleHotBarIndexChanged;
    }

    void BindInputCallbacks()
    {
        inputComponent.onMouseScroll += (scrollValue) =>
        {
            if(isUsingTool) return;

            if(scrollValue > 0f)
            {
                inventoryComponent.RollHotBarIndex(-1);
            }
            else if(scrollValue < 0f)
            {
                inventoryComponent.RollHotBarIndex(1);
            }
        };
        inputComponent.onMouseButtonDown += HandleMouseDown;
        inputComponent.onMouseButtonHeld += HandleMouseDown;   // For simplicity, treat held mouse button the same as mouse button down. Can be changed later if needed.

        inputComponent.onToggleInventoryInput += () =>
        {
            inventoryComponent.ToggleInventory();
        };
        inputComponent.onESCInput += () =>
        {
            inventoryComponent.ToggleInventory(true);   // Force close inventory when ESC is pressed.
        };
    }

    // Update
    void Update()
    {
        if(isActionDisabled) return;

        GetMovementInput();
        GetMouseDirectionInput();

        if(movementInput.y > 0)
        {
            heldItemSpriteRenderer.sortingOrder = 0;
        }
        else
        {
            heldItemSpriteRenderer.sortingOrder = 2;
        }
    }


    void LateUpdate()
    {
        onLateUpdate?.Invoke();
    }

    void FixedUpdate()
    {
        if(canMove && !isActionDisabled)
        {
            Movement();
        }
    }
    
    // Handle Input
    private void GetMovementInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputY = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(inputX, inputY);
        movementInput = inputX != 0 && inputY != 0 ? movementInput.normalized : movementInput;
    }

    private void GetMouseDirectionInput()
    {
        float mouseWorldX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        float mouseWorldY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
        mouseX = mouseWorldX - transform.position.x;
        mouseY = mouseWorldY - transform.position.y;
        
        if(Mathf.Abs(mouseX) > Mathf.Abs(mouseY))
        {
            mouseY = 0;
        }
        else
        {
            mouseX = 0;
        }
    }

    private void Movement()
    {
        rb.MovePosition(rb.position + movementInput * moveSpeed * Time.deltaTime);
    }

    public void HandleMouseDown(int mouseButton)
    {
        if(isActionDisabled) return;

        TryUseItemInHand(mouseButton);
    }

    // Interaction related
    public bool DetectInteractable(int mouseButton)
    {
        // Cast a ray from the center of the camera to detect interactable objects in front of the player.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 15f, LayerMask.GetMask("Interactable"));

        // Debug.DrawRay(ray.origin, ray.direction * 15f, Color.red, 1f);
        // Debug.Log("Raycast hit: " + hit.collider?.name);

        if(hit.collider != null)
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if(interactable != null)
            {
                return interactable.Interact(gameObject, mouseButton);
            }
        }
        return false;

    }

    // Inventory related

    private void TryUseItemInHand(int mouseButton)
    {
        // If no item is selected in hotbar, do nothing.
        int selectedHotBarIndex = (inventoryComponent as PlayerInventoryComponent).selectedHotBarIndex;

        ItemInstance selectedItemInstance = null;

        if(selectedHotBarIndex == -1)
        {
            // Create an empty item instance to represent empty hand, so that we can still allow player to interact with the world using empty hand (e.g. gather resources, etc.) without having to add extra logic for empty hand case in DetectInteractable and other interaction logic.
            selectedItemInstance = new ItemInstance() { ItemDefinition = new ItemDefinition() }; 
        } 
        else
        {
            selectedItemInstance = inventoryComponent.InventorySlots[selectedHotBarIndex].itemInstance;
        }


        Debug.Log($"Using item in hand: {selectedItemInstance.ItemDefinition.itemName}");

        // Left mouse button case
        if(mouseButton == 0)
        {
            bool interactResult = DetectInteractable(mouseButton);   // Try to detect interactable object first before using item in hand, to allow interaction with objects even when holding an item that can be used.
            if(interactResult) return;

            switch(selectedItemInstance.ItemDefinition.itemType)
            {
                // Tools:
                case ItemType.Axe:
                case ItemType.Hoe:
                case ItemType.Pickaxe:
                case ItemType.WateringCan:
                case ItemType.Sickle:
                    if(isUsingTool) return;
                    StartCoroutine(UseToolCoroutine(selectedItemInstance.ItemDefinition, Input.mousePosition));
                    break;
                case ItemType.Placable:
                    UsePlacable(selectedItemInstance.ItemDefinition, Input.mousePosition);
                    break;
                case ItemType.Seed:
                    UseSeed(selectedItemInstance.ItemDefinition , Input.mousePosition);
                        break;
                default:
                    break;
            }
        }
        else
        {
            // Try to interact with the world using the item in hand, e.g. place furniture, plant seed, etc. 
            // Try to detect interactable first before using item in hand, to allow interaction with objects even when holding an item that can be used.
            bool interactResult = DetectInteractable(mouseButton);

            // After trying to interact with the world, if there is no interactable object detected or the interaction failed, then try to use the item in hand (e.g. place furniture, plant seed, etc.).
            //  This allows player to interact with objects even when they are holding an item that can be used, and also allows player to use items in hand by clicking on empty space in the world.
            if(!interactResult)
            {
                bool itemUsed = false;
                switch(selectedItemInstance.ItemDefinition.itemType)
                {
                    case ItemType.Placable:
                        itemUsed = UsePlacable(selectedItemInstance.ItemDefinition, Input.mousePosition);
                        break;
                    case ItemType.Seed:
                        itemUsed = UseSeed(selectedItemInstance.ItemDefinition , Input.mousePosition);
                        break;
                    default:
                        break;
                }
                if(!itemUsed)
                {
                    TryUseEmptyHand(Input.mousePosition);
                }
            }
        }

        
    }
    private void TryUseEmptyHand(Vector2 mousePos)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        if(Vector2.Distance(transform.position, mouseWorldPos) > toolUsingRadius) return;

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 15f, LayerMask.GetMask("Resource"));
        if(hit.collider != null)
        {
            Debug.Log("Hit resource with empty hand");
            Resource resource = hit.collider.GetComponent<Resource>();
            if(resource != null)
            {
                ItemDefinition emptyHandToolDef = new ItemDefinition()
                {
                    itemID = -2,
                    itemName = "Empty Hand",
                    itemType = ItemType.None
                };
                resource.BeingGathered(gameObject, emptyHandToolDef);
            }
        }
    }

    private IEnumerator UseToolCoroutine(ItemDefinition itemDef, Vector2 mousePos)
    {
        canMove = false;
        isUsingTool = true;

        onUseTool.Invoke(itemDef);
        
        yield return new WaitForSeconds(0.5f);

        // Execute tool effect here (e.g. damage enemy, till soil, etc.)
        switch (itemDef.itemType)
        {
            case ItemType.Axe:
                UseAxe(mousePos);
                break;
            case ItemType.Hoe:
                UseHoe(mousePos);
                break;
            case ItemType.Pickaxe:
                break;
            case ItemType.WateringCan:
                UseWateringCan(mousePos);
                break;
            case ItemType.Sickle:
                break;
            default:
                break;
        }

        yield return new WaitForSeconds(0.25f);

        isUsingTool = false;
        canMove = true;
    }

    private void UseAxe(Vector2 mousePos)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        if(Vector2.Distance(transform.position, mouseWorldPos) > toolUsingRadius) return;

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 15f, LayerMask.GetMask("Resource"));
        if(hit.collider != null)
        {

            Resource resource = hit.collider.GetComponent<Resource>();
            if(resource != null)
            {
                int selectedHotBarIndex = inventoryComponent.selectedSlotIndex;
                resource.BeingGathered(gameObject, inventoryComponent.InventorySlots[selectedHotBarIndex].itemInstance.ItemDefinition);
            }
        }
    }
    private void UseHoe(Vector2 mousePos)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        if(Vector2.Distance(transform.position, mouseWorldPos) > toolUsingRadius) return;

        TileInfo tileInfo = GameMapSubsystem.Instance.GetTileInfoByWorldPos(mouseWorldPos);

        // TODO: when there is furniture, need to check if the furniture is in the way before allowing player to till soil or place items.
        if(tileInfo != null && tileInfo.diggable)
        {
            Debug.Log($"Tilled soil at {tileInfo.position}!");
            GameMapSubsystem.Instance.UpdateDugTile(tileInfo, true, 1);
        }
    }

    private void UseWateringCan(Vector2 mousePos)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // Check if the tile is within watering radius before watering. TODO: whether this logic is okay?
        if(Vector2.Distance(transform.position, mouseWorldPos) > toolUsingRadius) return;

        TileInfo tileInfo = GameMapSubsystem.Instance.GetTileInfoByWorldPos(mouseWorldPos);

        if(tileInfo != null && tileInfo.diggable && tileInfo.daySinceDug >= 0)   // Only allow watering on tilled soil.
        {
            Debug.Log($"Watered soil at {tileInfo.position}!");
            GameMapSubsystem.Instance.UpdateWateredTile(tileInfo, true);
        }
    }

    
    private bool UsePlacable(ItemDefinition itemDef, Vector2 mousePos)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // TODO: need to check if the tile is in the way before allowing player to till soil or place items.
        //  if(Vector2.Distance(transform.position, mouseWorldPos) > placingRadius) return;

        TileInfo tileInfo = GameMapSubsystem.Instance.GetTileInfoByWorldPos(mouseWorldPos);

        if(tileInfo != null && tileInfo.thingPlacable && tileInfo.isOccupied == false)
        {
            Debug.Log($"Placed item {itemDef.itemName} at {tileInfo.position}!");
            GameMapSubsystem.Instance.PlaceFurniture(tileInfo, itemDef);
            inventoryComponent.RemoveItemInSlot(inventoryComponent.selectedSlotIndex, 1);
            return true;
        }
        return false;
    }

    private bool UseSeed(ItemDefinition itemDef, Vector2 mousePos)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        TileInfo tileInfo = GameMapSubsystem.Instance.GetTileInfoByWorldPos(mouseWorldPos);

        if(tileInfo != null && tileInfo.diggable && tileInfo.isOccupied == false)
        {
            ResourceDefinition resourceDef = ResourceSubsystem.Instance.GetResourceDefinition(itemDef.itemID);

            if((resourceDef.resourceType == ResourceType.Crop && tileInfo.daySinceDug >= 0) || resourceDef.resourceType == ResourceType.Tree)   // If planting crop, only allow planting on tilled soil. If planting non-crop resource, allow planting on untilled soil as well.
            {
                Debug.Log($"Planted seed {itemDef.itemName} at {tileInfo.position}!");
                ResourceSubsystem.Instance.GenerateResource(tileInfo, itemDef.itemID, 0, 0);  
                inventoryComponent.RemoveItemInSlot(inventoryComponent.selectedSlotIndex, 1);
                return true;
            }
            
        }
        return false;
    }   

    private void HandleHotBarIndexChanged(int oldIndex, int newIndex)
    {
        if(newIndex == -1)
        {
            onHeldItemChanged?.Invoke(new ItemDefinition());
            return;
        }
        ItemDefinition itemDef = inventoryComponent.InventorySlots[newIndex].itemInstance.ItemDefinition;

        if(itemDef != null && itemDef.IsValidItem() && itemDef.isHoldable == true)
        {
            onHeldItemChanged?.Invoke(itemDef);

        }
        else
        {
            onHeldItemChanged?.Invoke(new ItemDefinition());
        }
    }
}
