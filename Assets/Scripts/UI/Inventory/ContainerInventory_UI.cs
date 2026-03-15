using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContainerInventory_UI : MonoBehaviour, IDragHandler, IUIClosable
{
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject slotPrefab;
    public void Initialize(InventoryComponent containerInventory)
    {
        // Create UI slots based on the container inventory size
        for(int i = 0; i < containerInventory.InventorySize; i++)
        {
            GameInstance.Instance.CreateUI(slotPrefab, Vector2.zero, slotsParent);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Allow dragging the container UI around the screen
        this.transform.position = eventData.position;
    }

    public void CloseUI()
    {
        // Close the container UI
        this.gameObject.SetActive(false);
    }


}
