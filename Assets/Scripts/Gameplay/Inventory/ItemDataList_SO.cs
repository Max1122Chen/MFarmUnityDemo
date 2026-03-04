using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(fileName = "ItemDataList_SO", menuName = "Inventory/ItemDataList_SO")]
    public class ItemDataList_SO : ScriptableObject
    {
        public List<ItemDefinition> itemDataList;
    }
}