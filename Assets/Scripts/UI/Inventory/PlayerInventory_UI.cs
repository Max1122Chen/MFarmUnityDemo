using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInventory_UI : MonoBehaviour, IDragHandler, IUIClosable
{
    private TextMeshProUGUI moneyText;

    public void UpdataMoneyText(int money)
    {
        moneyText.text = $"{money}";
    }
    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = eventData.position;
    }

    public void CloseUI()
    {
        // Close the player inventory UI
        this.gameObject.SetActive(false);
    }
}

