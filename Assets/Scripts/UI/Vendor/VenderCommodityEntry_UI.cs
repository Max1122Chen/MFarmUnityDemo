using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VenderCommodityEntry_UI : MonoBehaviour
{
    // Controller reference
    private VenderInventoryUIController controller;

    private int commodityIndex; // This is the index of the commodity in the vendor's commodity list, used for quick reference and UI updates

    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    public void Start()
    {
        buyButton.onClick.AddListener(OnBuyButtonClicked);
    }

    public void Initialize(VenderInventoryUIController controller, CommodityInstance commodity)
    {
        this.controller = controller;
        UpdateEntry(commodity);
    }
    public void UpdateEntry(CommodityInstance commodity)
    {
        ItemDefinition itemDef = InventorySubsystem.Instance.GetItemDefinition(commodity.itemID);

        itemIcon.sprite = itemDef.itemIcon;

        string itemQuantityText = commodity.quantity == int.MaxValue ? "" : " × " + commodity.quantity.ToString();
        itemNameText.text = itemDef.itemName + itemQuantityText;
        
        priceText.text = commodity.price.ToString();
    }

    public void OnBuyButtonClicked()
    {       
        // Call the controller's buy button handler with the commodity index and quantity of 1 (you can modify this to allow the player to choose quantity)
        Debug.Log($"Buy button clicked for commodity index {commodityIndex}");
        controller.HandleBuyButtonClicked(commodityIndex, 1, controller.vendor.currentInteractingPlayer);
    }
}
