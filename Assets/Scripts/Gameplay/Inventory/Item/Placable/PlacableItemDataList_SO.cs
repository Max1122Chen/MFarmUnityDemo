using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlacableItemData
{
    public int itemID = -1;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "PlacableItemDataList_SO", menuName = "Inventory/PlacableItemDataList_SO")]
public class PlacableItemDataList_SO : ScriptableObject
{
    public List<PlacableItemData> placableItemDataList;
}
