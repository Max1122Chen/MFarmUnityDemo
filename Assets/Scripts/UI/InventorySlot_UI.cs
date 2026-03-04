using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using InventorySystem;
using System;


public class InventorySlot_UI : 
    MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerMoveHandler, IPointerExitHandler
{
    public InventoryUIController inventoryUIController;
    private InventorySlot slotRef;

    [Header("UI References")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemCountText;
    [SerializeField] private Image selectionHighlightImage;
    [SerializeField] private Button button;

    bool isSelected = false;

    public Action<int> onSlotClicked;

    public void Start()
    {
        isSelected = false;
        UpdateSelectionState(isSelected);
    }

    public void UpdateSlot(InventorySlot slot)
    {
        ItemInstance itemInstance = slot.itemInstance;
        ItemDefinition itemDef = itemInstance != null ? itemInstance.ItemDefinition : null;
        slotRef = slot;
        if(itemInstance != null && itemDef != null && itemDef.IsValidItem())
        {
            itemIconImage.sprite = itemDef.itemIcon;
            itemIconImage.enabled = true;

            // Modify the image size for some specific items if needed.
            if(itemDef.itemType == ItemType.Furniture)
            {
                itemIconImage.rectTransform.sizeDelta = new Vector2(20, 20); // Example: set furniture icons to 20x20
            }

            if(itemInstance.stackCount > 1)
            {
                itemCountText.text = itemInstance.stackCount.ToString();
            }
            else
            {
                itemCountText.text = "";
            }
        }
        else
        {
            itemIconImage.sprite = null;
            itemIconImage.enabled = false;
            itemCountText.text = "";
        }
    }

    public void Refresh()
    {
        UpdateSlot(slotRef);
    }

    public void UpdateSelectionState(bool selected)
    {
        isSelected = selected;
        selectionHighlightImage.enabled = isSelected;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        onSlotClicked?.Invoke(slotRef.slotIndex);
    }
    

    public void OnBeginDrag(PointerEventData eventData)
    {
        ItemInstance itemInstance = slotRef.itemInstance;
        if(itemInstance.stackCount > 0)
        {
            InventorySubsystem.Instance.draggedItemUI.UpdateDraggedItemUI(itemInstance, eventData.position, true);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        onSlotClicked?.Invoke(-1);   // Deselect the slot while dragging

        ItemInstance itemInstance = slotRef.itemInstance;
        if(itemInstance.stackCount > 0)
        {
            InventorySubsystem.Instance.draggedItemUI.UpdateDraggedItemUI(slotRef.itemInstance, eventData.position, true);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ItemInstance itemInstance = slotRef.itemInstance;
        if(itemInstance.stackCount > 0)
        {
            InventorySubsystem.Instance.draggedItemUI.UpdateDraggedItemUI(null, eventData.position, false);
        }
        else
        {
            return; // No item to drag
        }

        if(eventData.pointerEnter != null)  // Dropped on UI element
        {
            if(eventData.pointerEnter.TryGetComponent<InventorySlot_UI>(out InventorySlot_UI targetSlotUI))
            {
                // Handle item swap or move between slots.
                InventoryComponent sourceInventory = inventoryUIController.InventoryComponent;
                InventoryComponent targetInventory = targetSlotUI.inventoryUIController.InventoryComponent;

                if(sourceInventory != null && targetInventory != null)
                {
                    sourceInventory.SwapItemsBetweenSlots(slotRef.slotIndex, targetInventory, targetSlotUI.slotRef.slotIndex);

                    // Deselect source slot after the swap, then select the target slot.
                    this.onSlotClicked?.Invoke(-1);
                    targetSlotUI.onSlotClicked?.Invoke(targetSlotUI.slotRef.slotIndex);
                }
            }
        }
        else    // Dropped outside any UI
        {
            // handle dropping item into the game world here.

            // If dropping position is too far from the source inventory (e.g. player),
            // do not throw the item into the game world, and maybe show a warning message to the player.
            InventoryComponent sourceInventory = inventoryUIController.InventoryComponent;
            var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

            Transform sourceTransform = sourceInventory != null ? sourceInventory.transform : null;

            if(sourceTransform != null && 
               Vector3.Distance(sourceTransform.position, pos) > 5f)    // TODO: The max drop distance should be determined by the player interaction range, and should also consider the player's current state (e.g. if player is currently performing certain actions, the max drop distance should be reduced to prevent abuse). 
            {
                onSlotClicked?.Invoke(slotRef.slotIndex);   // Reselect the slot if the drop action is invalid due to distance.
                return;
            }


            // Directly call the SpawnItemInWorld method from InventorySubSystem
            GameMapSubsystem.Instance.SpawnDroppedItemInWorld(slotRef.itemInstance, pos);

            sourceInventory.RemoveAllItemsInSlot(slotRef.slotIndex);

            Debug.Log("Dropped item into the game world from slot index: " + slotRef.slotIndex);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ItemInstance itemInstance = slotRef.itemInstance;
        if(itemInstance.stackCount <= 0)
        {
            return;
        }
        ItemDefinition itemDef = itemInstance != null ? itemInstance.ItemDefinition : null;

        var position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z);

        InventorySubsystem.Instance.itemToolTipsUI.UpdateItemTips(itemDef, position, true);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        ItemInstance itemInstance = slotRef.itemInstance;
        if(itemInstance.stackCount <= 0)
        {
            return;
        }
        InventorySubsystem.Instance.itemToolTipsUI.SetPosition(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if(slotRef.itemInstance.stackCount > 0)
        {
            InventorySubsystem.Instance.itemToolTipsUI.UpdateItemTips(null, Vector3.zero, false);
        }
    }


}
