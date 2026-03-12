using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContainerInventory_UI : MonoBehaviour, IDragHandler
{
    [SerializeField] private Transform SlotsParent;
    [SerializeField] private GameObject SlotPrefab;
    public void Initialize(InventoryComponent containerInventory)
    {
        // Create UI slots based on the container inventory size
        for(int i = 0; i < containerInventory.InventorySize; i++)
        {
            Instantiate(SlotPrefab, SlotsParent);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Allow dragging the container UI around the screen
        this.transform.position = eventData.position;
    }


}
