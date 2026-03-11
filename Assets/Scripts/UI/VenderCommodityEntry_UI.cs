using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VenderCommodityEntry_UI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI priceText;

    public void UpdateEntry(Commodity commodity)
    {
        ItemDefinition itemDef = InventorySubsystem.Instance.GetItemDefinition(commodity.itemID);

        itemIcon.sprite = itemDef.itemIcon;

        string itemQuantityText = commodity.quantity == int.MaxValue ? "" : " × " + commodity.quantity.ToString();
        itemNameText.text = itemDef.itemName + itemQuantityText;
        
        priceText.text = commodity.price.ToString();
    }
}
