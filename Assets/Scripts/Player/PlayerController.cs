using System;
using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Header("Player Portrait Data")]
    public List<CharacterPortrait> portraits = new List<CharacterPortrait>();  // This is a list of character portraits for the player, which can be used to show different portraits based on the player's relative position to the NPC during dialogue. The index of the portrait in the list should correspond to the CharacterPortraitType enum value for easy lookup.

    // Player Data
    public PlayerSaveData playerSaveData = null;
    private PlayerIMC playerIMC;
    private InputAction moveAction;
    private PlayerInputComponent inputComponent;
    private Rigidbody2D rb;
    private PlayerAnimationBlueprint abp;
    public PlayerInventoryComponent inventoryComponent { get; private set; }

    [SerializeField] private SpriteRenderer heldItemSpriteRenderer;

    public Action onLateUpdate;

    public Action<ItemDefinition> onHeldItemChanged;

    public Action<ItemDefinition> onUseTool;
    public Action<int, int> onPlayerMoneyChanged;   // <old money, new money>

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

        playerIMC = new PlayerIMC();
        inputComponent.AddMappingContext(playerIMC.asset);
        BindInputCallbacks();
    }

    void Start()
    {
        // Bind scene switch callback
        GameMapSubsystem.Instance.onNewSceneLoaded += (string sceneName) => {  isActionDisabled = false; };
        GameMapSubsystem.Instance.onOldSceneStartUnloading += (string sceneName) => {  isActionDisabled = true; };
        

        // Bind inventory hotbar index change callback to update held item info in ABP.
        inventoryComponent.onSelectedHotBarIndexChanged += HandleHotBarIndexChanged;
    }

    public void Initialze(PlayerSaveData saveData)
    {
        // This function will be called by the GameInstance after loading the game save data, to initialize the player data (e.g. position, inventory, etc.) based on the loaded game save data.
        playerSaveData = saveData;
        
        transform.position = saveData.position;
        inventoryComponent.LoadInventoryFromSaveData(saveData.playerInventory);
    }

    public void FixedUpdate()
    {
        if(isActionDisabled) return;
        if(canMove)
        {
            rb.MovePosition(rb.position + movementInput * moveSpeed * Time.deltaTime);
            if(movementInput.y > 0)
            {
                heldItemSpriteRenderer.sortingOrder = 0;
            }
            else
            {
                heldItemSpriteRenderer.sortingOrder = 2;
            }
        }
    }

    // Input Callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
        inputX = movementInput.x;
        inputY = movementInput.y;
    }

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        Vector2 mousePos = context.ReadValue<Vector2>();
        GetMouseDirectionInput(mousePos);
    }

    public void OnMousScroll(InputAction.CallbackContext context)
    {
        float scrollValue = context.ReadValue<float>();

        if(isUsingTool) return;

        if(scrollValue > 0f)
        {
            inventoryComponent.RollHotBarIndex(-1);
        }
        else if(scrollValue < 0f)
        {
            inventoryComponent.RollHotBarIndex(1);
        }
    }

    public void OnToggleInventory(InputAction.CallbackContext context)
    {
        inventoryComponent.ToggleInventory();
    }

    public void OnLeftMouseButtonClicked(InputAction.CallbackContext context)
    {
        if(isActionDisabled) return;

        TryUseItemInHand(0);
    }

    public void OnRightMouseButtonClicked(InputAction.CallbackContext context)
    {
        if(isActionDisabled) return;

        TryUseItemInHand(1);
    }

    public void OnNumberKeyPressed(InputAction.CallbackContext context)
    {
        if(isActionDisabled) return;

        int hotkeyNumber = (int)context.ReadValue<float>();
        (inventoryComponent as PlayerInventoryComponent).SelectHotBarSlotByHotkey((hotkeyNumber + 9) % 10); // Convert 1-0 number keys to 0-9 hotbar index.
    }

    public void OnEscape(InputAction.CallbackContext context)
    {
        
    }

    void BindInputCallbacks()
    {
        inputComponent.BindAction(playerIMC.Normal.Move, InputActionPhase.Performed, OnMove);
        inputComponent.BindAction(playerIMC.Normal.Move, InputActionPhase.Canceled, OnMove);
        inputComponent.BindAction(playerIMC.Normal.MouseMove, InputActionPhase.Performed, OnMouseMove);
        inputComponent.BindAction(playerIMC.Normal.MouseScroll, InputActionPhase.Performed, OnMousScroll);
        inputComponent.BindAction(playerIMC.Normal.ToggleInventory, InputActionPhase.Performed, OnToggleInventory);
        inputComponent.BindAction(playerIMC.Normal.LeftMouseButtonClicked, InputActionPhase.Performed, OnLeftMouseButtonClicked);
        inputComponent.BindAction(playerIMC.Normal.RightMouseButtonClicked, InputActionPhase.Performed, OnRightMouseButtonClicked);
        inputComponent.BindAction(playerIMC.Normal.NumberKeyPressed, InputActionPhase.Performed, OnNumberKeyPressed);

    }

    void LateUpdate()
    {
        onLateUpdate?.Invoke();
    }
    
    // Handle Input

    private void GetMouseDirectionInput(Vector2 mousePos)
    {
        float mouseWorldX = Camera.main.ScreenToWorldPoint(mousePos).x;
        float mouseWorldY = Camera.main.ScreenToWorldPoint(mousePos).y;
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
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
            if(interactable != null)
            {
                return interactable.Interact(gameObject, mouseButton);
            }
        }
        return false;

    }

    // Money related
    public void AddMoney(int amount)
    {
        int oldMoney = playerSaveData.money;
        playerSaveData.money += amount;
        onPlayerMoneyChanged?.Invoke(oldMoney, playerSaveData.money);
    }

    public void ReduceMoney(int amount)
    {
        int oldMoney = playerSaveData.money;
        playerSaveData.money -= amount; // TODO: Add check to prevent money from going below 0 if needed?
        onPlayerMoneyChanged?.Invoke(oldMoney, playerSaveData.money);
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


        // Debug.Log($"Using item in hand: {selectedItemInstance.ItemDefinition.itemName}");

        // Left mouse button case
        if(mouseButton == 0)
        {
            // Try to detect interactable object first before using item in hand, to allow interaction with objects even when holding an item that can be used.
            bool interactResult = DetectInteractable(mouseButton);   
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
            Debug.Log($"Tilled soil at {tileInfo.gridPos}!");
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
            Debug.Log($"Watered soil at {tileInfo.gridPos}!");
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
            Debug.Log($"Placed item {itemDef.itemName} at {tileInfo.gridPos}!");
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
                Debug.Log($"Planted seed {itemDef.itemName} at {tileInfo.gridPos}!");
                ResourceSubsystem.Instance.SpawnResource(tileInfo, itemDef.itemID, 0, 0);  
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

        if(itemDef != null && itemDef.IsValidItem() && itemDef.IsHoldable())
        {
            onHeldItemChanged?.Invoke(itemDef);

        }
        else
        {
            onHeldItemChanged?.Invoke(new ItemDefinition());
        }
    }
}
