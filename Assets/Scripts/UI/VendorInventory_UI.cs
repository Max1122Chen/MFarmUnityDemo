using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VendorInventory_UI : MonoBehaviour
{
    [Header("Commodity Entry Prefab")]
    public GameObject commodityEntryPrefab;

    [Header("UI References")]
    public GameObject commoditiesRoot;
    public List<VenderCommodityEntry_UI> commodityEntryUIList = new List<VenderCommodityEntry_UI>();
    public Image VendorProtrait;
    public TextMeshProUGUI VendorWords;

    public void Start()
    {
        
    }

    // public void Initialize(VendorData_SO vendorData)
    // {
    //     // Set the vendor portrait and words
    //     VendorProtrait.sprite = vendorData.vendorPortrait;
    //     VendorWords.text = vendorData.vendorWords;

    //     // Create commodity entry UI for each commodity in the vendor data
    //     foreach(var commodity in vendorData.commodityList)
    //     {
    //         GameObject entryGO = Instantiate(commodityEntryPrefab, commoditiesRoot.transform);
    //         VenderCommodityEntry_UI entryUI = entryGO.GetComponent<VenderCommodityEntry_UI>();
    //         entryUI.Initialize(commodity);
    //         commodityEntryUIList.Add(entryUI);
    //     }
    // }
}
