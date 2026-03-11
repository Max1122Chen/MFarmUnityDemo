using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Commodity
{
    public int itemID = -1;
    public int price = 0;
    public int quantity = int.MaxValue;    // INT_MAX for infinite quantity
}

[System.Serializable]
public class VendorData
{
    public string shopKeeperName;
    public List<Commodity> commodityList;
}

[CreateAssetMenu(fileName = "VendorDataList_SO", menuName = "VendorDataList_SO")]
public class VendorDataList_SO : ScriptableObject
{
    public List<VendorData> vendorList;
}
