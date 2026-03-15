using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VenderInventoryUIController : MonoBehaviour
{
    // Vendor reference
    public Vendor vendor;

    // CallBacks
    public Action<CommodityInstance, CommodityInstance> onCommodityChanged; // (oldCommodity, newCommodity)
    public void Initialize(Vendor vendor)
    {
        this.vendor = vendor;
        this.vendor.onCommodityChanged += HandleOnCommodityChanged;

        // Initialize the UI with the vendor's current commodities
    }
    private void HandleOnCommodityChanged(CommodityInstance oldCommodity, CommodityInstance newCommodity)
    {
        onCommodityChanged?.Invoke(oldCommodity, newCommodity);
    }

    public void OnDestroy()
    {
        vendor.onCommodityChanged -= HandleOnCommodityChanged;
    }

    public void HandleBuyButtonClicked(int commodityIndex, int quantity, PlayerController player)
    {
        vendor.SellItem(commodityIndex, quantity, player);
    }
}
