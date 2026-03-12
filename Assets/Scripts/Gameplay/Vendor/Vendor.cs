using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimeSystem;
using System;

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

    // Key: itemID, Value: CommodityDefinition. This is used for quick lookup of the commodity definition when we need to check if a commodity is on sale or not.
    private Dictionary<int, CommodityDefinition> commodityDict = new Dictionary<int, CommodityDefinition>();
    private List<CommodityInstance> commoditiesOnSale = new List<CommodityInstance>();
    private int currentCommodityIndex = 0; // This is used to assign a unique index to each commodity instance for UI reference

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
        Debug.Log($"Interacting with vendor {vendorID} using mouse button {mouseButton}. Opening vendor inventory UI.");
        EconomySubsystem.Instance.InteractWithVendor(interactor, this);
        return true;
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

    public void SellItem(int itemID, int quantity, PlayerController player)
    {
    
    }
}
