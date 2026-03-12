using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimeSystem;

[System.Serializable]
public class CommodityDefinition
{
    public int itemID = -1;
    public int price = 0;
    public int quantity = int.MaxValue;    // INT_MAX for infinite quantity
    public GameTime saleStartTime = new GameTime(0, 0, 1, 1, 1);   // Default to always on sale
    public GameTime saleEndTime = new GameTime(59, 23, 31, 12, 9999);   // Default to always on sale

    // For more complex sale patterns, we will consider the restock time interval after selling any item.
    // For now, we just assume the item will be restocked at the next day after selling out, which means the item will be available again at the start of the next day.
    // public int restockTimeIntervalInDays = 1;    // Number of in-game days it takes to restock after selling out. 1 means it will restock at the next day.
}

[System.Serializable]
public class VendorData
{
    public int vendorID = -1;    // -1 for invalid ID, should be set in the inspector
    public string shopKeeperName;
    public List<CommodityDefinition> commodityList = new List<CommodityDefinition>();
}

[CreateAssetMenu(fileName = "VendorDataList_SO", menuName = "VendorDataList_SO")]
public class VendorDataList_SO : ScriptableObject
{
    public List<VendorData> vendorList = new List<VendorData>();
}
