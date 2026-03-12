using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VendorInventory_UI : MonoBehaviour
{


    [Header("UI References")]
    public GameObject commoditiesRoot;
    public Dictionary<int, VenderCommodityEntry_UI> commodityEntryUIDict = new Dictionary<int, VenderCommodityEntry_UI>();
    public Image VendorProtrait;
    public TextMeshProUGUI VendorWords;

    // Controller reference
    private VenderInventoryUIController controller;

    public void Awake()
    {
        controller = GetComponent<VenderInventoryUIController>();
    }

    public void Start()
    {
        
    }

    public void Initialize(Vendor vendor)
    {
        // Bind callbacks
        controller.onCommodityChanged += HandleOnCommodityChanged;

        // Set vendor's protriat and words
        VendorData vendorData = EconomySubsystem.Instance.GetVendorDataByID(vendor.vendorID);
        NPCData npcData = NPCSubsystem.Instance.GetNPCDataByName(vendorData.shopKeeperName);

        if(npcData != null)
        {
            VendorProtrait.sprite = npcData.npcPortrait;
        }

    }

    public void CreateEntry(CommodityInstance commodity)
    {
        GameObject entryGO = GameInstance.Instance.CreateUI(EconomySubsystem.Instance.commodityEntryPrefab, new Vector2(0, 0), commoditiesRoot.transform);
        VenderCommodityEntry_UI entryUI = entryGO.GetComponent<VenderCommodityEntry_UI>();
        entryUI.Initialize(commodity);
        commodityEntryUIDict[commodity.itemID] = entryUI;
    }

    public void HandleOnCommodityChanged(CommodityInstance oldCommodity, CommodityInstance newCommodity)
    {        
        // Find the corresponding entry UI based on the commodity index
        if (commodityEntryUIDict.TryGetValue(newCommodity.itemID, out VenderCommodityEntry_UI entryUI))
        {
            entryUI.UpdateEntry(newCommodity);
        }
        else
        {
            CreateEntry(newCommodity);
        }
    }
}
