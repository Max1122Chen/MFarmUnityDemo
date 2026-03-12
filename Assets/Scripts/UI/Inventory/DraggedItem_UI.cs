using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem;
using UnityEngine.UI;

public class DraggedItem_UI : MonoBehaviour
{
    [SerializeField] private Image itemIconImage;
    void Awake()
    {
        itemIconImage = GetComponent<Image>();
    }
    void Start()
    {
        InventorySubsystem.Instance.draggedItemUI = this;
        
        InventorySubsystem.Instance.onInventoryToggled += HandleInventoryToggle;    
    }

    public void UpdateDraggedItemUI(ItemInstance itemInstance, Vector3 position, bool visible)
    {
        if(visible)
        {
            this.transform.position = position;

            if(itemInstance == null) return;    // In case of no item, do not update UI

            // Update UI elements to show itemInstance details
            itemIconImage.sprite = itemInstance.ItemDefinition.itemIcon;
            itemIconImage.SetNativeSize();
            itemIconImage.enabled = true;
        }
        else
        {
            itemIconImage.enabled = false;
        }
    }

    private void HandleInventoryToggle()
    {
        // When inventory is toggled, hide the dragged item UI to prevent it from lingering on screen.
        UpdateDraggedItemUI(null, Vector3.zero, false);
    }
}
