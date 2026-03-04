using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using InventorySystem;
public class ItemToolTip_UI : MonoBehaviour
{
    private GameObject[] children;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemValueText;

    void Awake()
    {
        children = new GameObject[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i).gameObject;
        }
        SetVisibility(false);
    }
    void Start()
    {
        InventorySubsystem.Instance.itemToolTipsUI = this;
    }
    public void UpdateItemTips(ItemDefinition itemDef, Vector3 position, bool visible)
    {
        if(!visible)
        {
            SetVisibility(false);
            return;
        }

        SetVisibility(true);
        if(itemDef != null && itemDef.IsValidItem())
        {
            itemNameText.text = itemDef.itemName;
            itemTypeText.text = itemDef.itemType.ToString();
            itemDescriptionText.text = itemDef.itemDescription;
            itemValueText.text = itemDef.value.ToString();
        }
        else    // This should not happen, but just in case
        {
            itemNameText.text = "Invalid Item";
            itemTypeText.text = "N/A";
            itemDescriptionText.text = "This is an invalid item.";
            itemValueText.text = "-1";
        }
        
        // Position the tooltip near the mouse cursor
        SetPosition(position);
    }

    public void SetPosition(Vector3 position)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.position = position;
    }

    private void SetVisibility(bool visible)
    {
        foreach(var child in children)
        {
            child.SetActive(visible);
        }
        backgroundImage.enabled = visible;
    }

}
