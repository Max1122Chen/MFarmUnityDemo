using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlacablePrefabData
{
    public int itemID = -1;
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "PlacablePrefab_SO", menuName = "Inventory/PlacablePrefab_SO")]
public class PlacablePrefab_SO : ScriptableObject
{
    public List<PlacablePrefabData> placablePrefabDataList;
}
