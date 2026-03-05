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


        [Header("Registered Inventory Components")]
        public List<InventoryComponent> registeredInventoryComponents;

        [Header("Dragged Item UI")]
        public DraggedItem_UI draggedItemUI;

        [Header("Item Tips UI")]
        public ItemToolTip_UI itemToolTipsUI;

        [Header("Container UI Prefab")]
        public GameObject containerUIPrefab;


        public GameObject inventoryUIRoot;
        [SerializeField] private List<GameObject> currentOpenContainerUIs;

        public Action onInventoryToggled; // Event triggered when inventory is toggled (opened or closed)

        public void Initialize()
        {
            foreach (var itemDef in itemDataList_SO.itemDataList)
            {
                itemData[itemDef.itemID] = itemDef;
            }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            currentOpenContainerUIs = new List<GameObject>();
            inventoryUIRoot = GameObject.FindWithTag("InventoryUIRoot");
            if(inventoryUIRoot == null)
            {
                Debug.LogError("InventorySubsystem: No GameObject with tag 'InventoryUIRoot' found in the scene. Please add one to serve as the parent for inventory UIs.");
            }
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
            // TODO: here we hardcoded the position of the container UI, we can improve this in the future by making the container UI follow the container or player around, or we can make the container UI appear at the mouse position when player interacts with the container.
            GameObject newUI = GameInstance.Instance.CreateUI(containerUIPrefab, new Vector2(0, 180), inventoryUIRoot.transform);
            currentOpenContainerUIs.Add(newUI);

            // Initialize the container UI with the container's inventory's size
            newUI.GetComponent<ContainerInventory_UI>().Initialize(containerInventory);
            InventoryUIController uiController = newUI.GetComponent<InventoryUIController>();

            uiController.InventoryComponent = containerInventory;
            containerInventory.ToggleInventory();
            
        }

        // Save and Load
        public void SaveAllContainersInventory()
        {
            foreach(var containerInvComp in registeredInventoryComponents)
            {
                if(containerInvComp.InventoryType == InventoryType.Container)
                {
                    
                }
            }
        }

        public void LoadAllContainersInventory()
        {
            
        }
    }
}