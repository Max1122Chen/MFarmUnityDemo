using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomySubsystem : Singleton<EconomySubsystem>
{
    [Header("Vender Data")]
    public VendorDataList_SO vendorDataList;
    private Dictionary<int, VendorData> vendorDataDict = new Dictionary<int, VendorData>();

    private List<Vendor> registeredVendorList = new List<Vendor>();

    [Header("Commodity UI Prefabs")]
    public GameObject commodityEntryPrefab;
    public GameObject vendorInventoryUIPrefab;

    [Header("Inventory UI Root")]
    public GameObject inventoryUIRoot;

    protected override void Awake()
    {
        base.Awake(); 
    }

    void Start()
    {
        inventoryUIRoot = GameObject.FindWithTag("InventoryUIRoot");
        if(inventoryUIRoot == null)
        {
            Debug.LogError("InventorySubsystem: No GameObject with tag 'InventoryUIRoot' found in the scene. Please add one to serve as the parent for inventory UIs.");
        }
    }

    public void Initialize()
    {
        // Initialize vendor data dictionary for quick lookup
        foreach(var vendorData in vendorDataList.vendorList)
        {
            if(!vendorDataDict.ContainsKey(vendorData.vendorID))
            {
                vendorDataDict.Add(vendorData.vendorID, vendorData);
            }
            else
            {
                Debug.LogWarning($"Duplicate vendor ID {vendorData.vendorID} found in VendorDataList_SO. Please ensure all vendor IDs are unique.");
            }
        }
    }

    public void RegisterVendor(Vendor vendor)
    {
        if(!registeredVendorList.Contains(vendor))
        {
            registeredVendorList.Add(vendor);
        }
    }

    public void UnregisterVendor(Vendor vendor)
    {
        if(registeredVendorList.Contains(vendor))
        {
            registeredVendorList.Remove(vendor);
        }
    }


    public VendorData GetVendorDataByID(int id)
    {
        if(vendorDataDict.ContainsKey(id))
        {
            return vendorDataDict[id];
        }
        return null;
    }

    public void InteractWithVendor(GameObject interator, Vendor vendor)
    {
        // Open the vendor inventory UI and initialize it with the vendor's commodities
        // Currently we create a new inventory UI for each interaction, but we could optimize this by pooling the UI or reusing a single instance if we only allow one vendor interaction at a time.
        VendorInventory_UI inventoryUI = GameInstance.Instance.CreateUI(vendorInventoryUIPrefab, new Vector2(0, 0), inventoryUIRoot.transform).GetComponent<VendorInventory_UI>();
        VenderInventoryUIController uiController = inventoryUI.GetComponent<VenderInventoryUIController>();
        uiController.Initialize(vendor);
        inventoryUI.Initialize(vendor);
    }
}
