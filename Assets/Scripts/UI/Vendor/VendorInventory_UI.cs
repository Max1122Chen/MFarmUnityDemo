using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VendorInventory_UI : MonoBehaviour, IUIClosable
{
    // Vendor reference
    private Vendor currentVendor;

    [Header("UI References")]
    public GameObject commoditiesRoot;
    public Dictionary<int, VenderCommodityEntry_UI> commodityEntryUIDict = new Dictionary<int, VenderCommodityEntry_UI>();
    public Image vendorPortrait;
    public TextMeshProUGUI vendorWords;
    public Button closeButton;

    // Controller reference
    private VenderInventoryUIController controller;

    public void Awake()
    {
        controller = GetComponent<VenderInventoryUIController>();
        closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    public void Start()
    {
        
    }

    public void Initialize(Vendor vendor)
    {
        currentVendor = vendor;

        // Bind callbacks
        controller.onCommodityChanged += HandleOnCommodityChanged;

        // Set vendor's protriat and words
        VendorData vendorData = EconomySubsystem.Instance.GetVendorDataByID(vendor.vendorID);
        NPCData npcData = NPCSubsystem.Instance.GetNPCDataByName(vendorData.shopKeeperName);

        if(npcData != null)
        {
            // vendorPortrait.sprite = npcData.npcPortrait;
        }

        // Initialize the UI with the current commodities on sale
        foreach(var commodity in vendor.CommoditiesOnSale)
        {
            CreateEntry(commodity);
        }
    }

    public void CreateEntry(CommodityInstance commodity)
    {
        GameObject entryGO = GameInstance.Instance.CreateUI(EconomySubsystem.Instance.commodityEntryPrefab, new Vector2(0, 0), commoditiesRoot.transform);
        VenderCommodityEntry_UI entryUI = entryGO.GetComponent<VenderCommodityEntry_UI>();
        entryUI.Initialize(controller, commodity);
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

    private void OnCloseButtonClicked()
    {
        // Simply destroy the vendor inventory UI when close button is clicked. The controller will handle the cleanup of the data and unbinding of callbacks.
        CloseUI();
    }

    public void CloseUI()
    {
        currentVendor.uiOpened = false;
        GameObject.Destroy(this.gameObject);
    }
}
