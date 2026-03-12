using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VenderInventoryUIController : MonoBehaviour
{
    // Vendor reference
    private Vendor Vendor;

    // CallBacks
    public Action<CommodityInstance, CommodityInstance> onCommodityChanged; // (oldCommodity, newCommodity)
    public void Initialize(Vendor vendor)
    {
        this.Vendor = vendor;
        Vendor.onCommodityChanged += HandleOnCommodityChanged;

        // Initialize the UI with the vendor's current commodities
    }
    private void HandleOnCommodityChanged(CommodityInstance oldCommodity, CommodityInstance newCommodity)
    {
        onCommodityChanged?.Invoke(oldCommodity, newCommodity);
    }
}
