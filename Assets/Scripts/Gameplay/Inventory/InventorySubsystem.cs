using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace InventorySystem
{
    public class InventorySubsystem : Singleton<InventorySubsystem>
    {
        [Header("Item Data")]
        public ItemDataList_SO itemDataList_SO;
        public Dictionary<int, ItemDefinition> itemData = new Dictionary<int, ItemDefinition>();

        [Header("Placable Prefab Data")]
        public PlacablePrefab_SO placablePrefab_SO;
        public Dictionary<int, GameObject> placablePrefabData = new Dictionary<int, GameObject>();

        [Header("Registered Inventory Components")]
        public List<InventoryComponent> registeredInventoryComponents;

        [Header("Dragged Item UI")]
        public DraggedItem_UI draggedItemUI;

        [Header("Item Tips UI")]
        public ItemToolTip_UI itemToolTipsUI;

        [Header("Container UI Prefab")]
        public GameObject containerUIPrefab;


        public GameObject InventoryUIRoot;
        [SerializeField] private List<GameObject> currentOpenContainerUIs;

        public Action onInventoryToggled; // Event triggered when inventory is toggled (opened or closed)

        private void Initialize()
        {
            foreach (var itemDef in itemDataList_SO.itemDataList)
            {
                itemData[itemDef.itemID] = itemDef;
            }
            foreach (var placableData in placablePrefab_SO.placablePrefabDataList)
            {
                placablePrefabData[placableData.itemID] = placableData.prefab;
            }
        }

        protected override void Awake()
        {
            Initialize();
            base.Awake();
        }

        void Start()
        {
            currentOpenContainerUIs = new List<GameObject>();
        }

        public void RegisterInventoryComponent(InventoryComponent inventoryComponent)
        {
            if(!registeredInventoryComponents.Contains(inventoryComponent))
            {
                registeredInventoryComponents.Add(inventoryComponent);
                inventoryComponent.onToggleInventory += () => onInventoryToggled?.Invoke(); // Subscribe to the inventory component's toggle event and invoke the subsystem's event
            }
        }

        public void UnregisterInventoryComponent(InventoryComponent inventoryComponent)
        {
            if(registeredInventoryComponents.Contains(inventoryComponent))
            {
                registeredInventoryComponents.Remove(inventoryComponent);
                inventoryComponent.onToggleInventory -= () => onInventoryToggled?.Invoke(); // Unsubscribe from the inventory component's toggle event
            }
        }

        public ItemDefinition GetItemDefinition(int itemID)
        {
            return itemData.ContainsKey(itemID) ? itemData[itemID] : null;
        }

        public GameObject GetPlacablePrefab(int itemID)
        {
            return placablePrefabData.ContainsKey(itemID) ? placablePrefabData[itemID] : null;
        }



        public void InteractWithContainer(GameObject interactor, InventoryComponent containerInventory)
        {   
            // Allow player open mutiple different container UIs, but prevent opening multiple UIs for the same container inventory.
            foreach(var currentUI in currentOpenContainerUIs)
            {
                // Handle case where player interacts with the same container again
                if(currentUI != null && currentUI.GetComponent<InventoryUIController>().InventoryComponent  == containerInventory)
                {
                    // To prevent open too much container UIs, we will close the container UI if player interacts with the same container again
                    currentOpenContainerUIs.Remove(currentUI);
                    Destroy(currentUI);
                    return;
                }

            }
            // Create new container UI and set it up with the container's inventory
            GameObject newUI = GameInstance.Instance.CreateUI(containerUIPrefab, InventoryUIRoot.transform);
            currentOpenContainerUIs.Add(newUI);

            // Initialize the container UI with the container's inventory's size
            newUI.GetComponent<ContainerInventory_UI>().Initialize(containerInventory);
            InventoryUIController uiController = newUI.GetComponent<InventoryUIController>();

            uiController.InventoryComponent = containerInventory;
            containerInventory.ToggleInventory();
            
        }
    }
}