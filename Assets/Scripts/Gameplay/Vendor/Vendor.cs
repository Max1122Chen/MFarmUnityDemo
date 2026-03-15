using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimeSystem;
using System;
using InventorySystem;

[System.Serializable]
public class CommodityInstance
{
    public int index; // This is the index of the commodity in the vendor's commodity list, used for quick reference and UI updates
    public int itemID;
    public int quantity;
    public int price;

    public CommodityInstance(CommodityDefinition def)
    {
        index = -1; // This will be set when the commodity is added to the vendor's commodity list
        this.itemID = def.itemID;
        this.quantity = def.quantity;
        this.price = def.price;
    }

    public CommodityInstance(CommodityInstance other)
    {
        this.index = other.index;
        this.itemID = other.itemID;
        this.quantity = other.quantity;
        this.price = other.price;
    }
}

public class Vendor : Interactable
{
    public int vendorID = -1;    // -1 for invalid ID, should be set in the inspector

    // Cached reference to the vendor data for quick access
    private VendorData vendorData;

    // Commodity Data
    // Key: itemID, Value: CommodityDefinition. This is used for quick lookup of the commodity definition when we need to check if a commodity is on sale or not.
    private Dictionary<int, CommodityDefinition> commodityDict = new Dictionary<int, CommodityDefinition>();
    private List<CommodityInstance> commoditiesOnSale = new List<CommodityInstance>();
    public List<CommodityInstance> CommoditiesOnSale => commoditiesOnSale;
    private int currentCommodityIndex = 0; // This is used to assign a unique index to each commodity instance for UI reference

    [Header("Shop Keeper Info")]
    public Transform shopKeeperStandPoint;
    public bool hasShopKeeper;
    public PlayerController currentInteractingPlayer; // This is the player who is currently interacting with the vendor, used for processing buy/sell requests
    public bool uiOpened = false;

    // Callbacks
    public Action<CommodityInstance, CommodityInstance> onCommodityChanged; // (oldCommodity, newCommodity)

    public void Start()
    {
        EconomySubsystem.Instance.RegisterVendor(this);
        Initialize(vendorID);

    }

    public void Initialize(int vendorID)
    {
        // TODO: Do we need this?
        this.vendorID = vendorID;
        vendorData = EconomySubsystem.Instance.GetVendorDataByID(vendorID);
        if(vendorData == null)
        {
            Debug.LogError($"Vendor with ID {vendorID} not found in EconomySubsystem. Please ensure the vendor data is properly set up in VendorDataList_SO.");
            return;
        }

        // Initialize commodity dictionary for quick lookup
        foreach(CommodityDefinition commodity in vendorData.commodityList)
        {
            if(!commodityDict.ContainsKey(commodity.itemID))
            {
                commodityDict.Add(commodity.itemID, commodity);
            }
        }

        // Initialize the commodities on sale based on the current game time
        UpdateCommoditiesOnSale();

        // Subscribe to time passed events to update the commodities on sale when time changes
        TimeSubsystem.Instance.onDayPassed += (day) => UpdateCommoditiesOnSale();
    }

    public override bool Interact(GameObject interactor, int mouseButton)
    {
        if(mouseButton != 1) // Only respond to right-click interactions
        {
            return false;
        }

        if(!CheckIfPlayerInInteractionRange(interactor))
        {
            return false;
        }

        // TODO: We might want to check for the presence of the shop keeper NPC at the stand point before allowing interaction with the vendor. This is to ensure that the player can only interact with the vendor when the shop keeper is present.
        // if(!hasShopKeeper)
        // {
        //     return false;
        // }

        if(uiOpened)
        {
            return false; // If the UI is already opened, we don't want to open it again
        }
        uiOpened = true;

        currentInteractingPlayer = interactor.GetComponent<PlayerController>();
        EconomySubsystem.Instance.InteractWithVendor(interactor, this);
        return true;
    }

    private bool CheckForShopKeeperPresence()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(shopKeeperStandPoint.position, new Vector2(0.5f, 0.5f), 0f);
        foreach(var collider in colliders)
        {
            NPCController npc = collider.GetComponent<NPCController>();
            if(npc != null && npc.NPCName == vendorData.shopKeeperName)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckIfPlayerInInteractionRange(GameObject player)
    {
        float distance = Vector2.Distance(player.transform.position, transform.position);
        return distance <= 2f; // Assuming 2 units is the interaction range
    }
    public void UpdateCommoditiesOnSale()
    {
        // Get the current game time from the TimeSubsystem
        GameTime currentGameTime = TimeSubsystem.Instance.GetCurrentGameTime();

        List<CommodityInstance> commodityToUnlist = new List<CommodityInstance>(); // List of itemIDs to unlist from the shop
        HashSet<int> commodityOnList = new HashSet<int>();

        // Unlist the commodities that are no longer on sale and update the quantity of the commodities that are still on sale
        foreach(var commodity in commoditiesOnSale)
        {
            // Check if the commodity is still on sale based on the current game time. If not, we will unlist it from the shop.
            if(!IsCommodityOnSale(commodity.itemID, currentGameTime))
            {
                commodityToUnlist.Add(commodity);
            }
            // If the commodity is still on sale, we can also update the quantity if needed (e.g., restock after selling out)
            else
            {
                // If the commodity is still on sale, we can also update the quantity if needed (e.g., restock after selling out)
                // For now, we just assume the item will be restocked at the next day after selling out.
                CommodityInstance oldCommodity = new CommodityInstance(commodity);

                commodity.quantity = GetCommodityDefinition(commodity.itemID)?.quantity ?? 0;
                commodityOnList.Add(commodity.itemID);

                onCommodityChanged?.Invoke(oldCommodity, commodity); // Notify the UI to update the entry for this commodity
            }
        }

        // Add the commodities that are now on sale but not listed yet
        foreach(var commodity in vendorData.commodityList)
        {
            // Check if the commodity is on sale based on the current game time. If it is on sale and not already listed, we will list it in the shop.
            if(IsCommodityOnSale(commodity.itemID, currentGameTime))
            {
                if(!commodityOnList.Contains(commodity.itemID))
                {
                    CommodityInstance newCommodity = new CommodityInstance(commodity);
                    newCommodity.index = currentCommodityIndex++; // Assign a unique index for UI reference

                    commoditiesOnSale.Add(newCommodity);
                    onCommodityChanged?.Invoke(null, newCommodity); // Notify the UI to add a new entry for this commodity
                }
            }
        }

        foreach(CommodityInstance commodity in commodityToUnlist)
        {
            CommodityInstance oldCommodity = new CommodityInstance(commodity);
            commoditiesOnSale.RemoveAll(c => c.index == commodity.index); // Remove the commodity from the list of commodities on sale
            onCommodityChanged?.Invoke(oldCommodity, null); // Notify the UI to remove the entry for this commodity
        }
    }

    public CommodityDefinition GetCommodityDefinition(int itemID)
    {
        if(commodityDict.ContainsKey(itemID))
        {
            return commodityDict[itemID];
        }
        return null;
    }

    public bool IsCommodityOnSale(int itemID, GameTime currentGameTime)
    {
        // Check if the current game time is within the sale start and end time of the commodity
        return currentGameTime.CompareTo(commodityDict[itemID].saleStartTime) >= 0 &&
               currentGameTime.CompareTo(commodityDict[itemID].saleEndTime) <= 0;
    }

    public void SellItem(int commodityIndex, int quantity, PlayerController player)
    {
        CommodityInstance commodity = commoditiesOnSale.Find(c => c.index == commodityIndex);
        if(commodity == null) return;

        // Check the available space of the player's inventory first.
        int availableSpace = player.inventoryComponent.CheckForAvailableSpaceForItem(commodity.itemID);

        // Check if the player has enough money to buy the item
        int playerMoney = player.playerSaveData.money;
        int playerAffordableQuantity = playerMoney / commodity.price;

        int quantityToSell = Math.Min(quantity, playerAffordableQuantity);
        quantityToSell = Math.Min(quantityToSell, availableSpace);

        if(quantityToSell <= 0)
        {
            if(playerAffordableQuantity <= 0)
            {
                Debug.Log($"Player cannot afford to buy {commodity.itemID}. Required money: {commodity.price}, Player money: {playerMoney}");
            }
            else if(availableSpace <= 0)
            {
                Debug.Log($"Player does not have enough inventory space to buy {commodity.itemID}. Required space: 1, Available space: {availableSpace}");
            }
            return;
        }

        // Process the transaction: add the item to the player's inventory and deduct the quantity from the vendor's commodity list
        ItemDefinition itemDef = InventorySubsystem.Instance.GetItemDefinition(commodity.itemID);
        ItemInstance itemInstance = new ItemInstance(itemDef, quantityToSell);
        player.inventoryComponent.TryAddItem(itemInstance);

        // TODO: for now we just simply subtract player's money based on the quantity they want to buy.
        player.ReduceMoney(quantityToSell * commodity.price);

        Debug.Log($"Player bought {quantityToSell} of {itemDef.itemName} for {quantityToSell * commodity.price} money.");
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("NPC"))
        {
            NPCController npc = collision.GetComponent<NPCController>();
            if(npc != null && npc.NPCName == vendorData.shopKeeperName)
            {
                hasShopKeeper = true;
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("NPC"))
        {
            NPCController npc = collision.GetComponent<NPCController>();
            if(npc != null && npc.NPCName == vendorData.shopKeeperName)
            {
                hasShopKeeper = false;
            }
        }
    }
}
