using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogBox_UI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogBoxRoot;
    public TextMeshProUGUI dialogText;
    public Image leftProtraitImage;
    public Image rightProtraitImage;

    bool isDialogBoxActive = false;

    public void ShowDialog(Sprite left, Sprite right, string dialog)
    {
        if(!isDialogBoxActive)
        {
            SetActive(true);
        }
        UpdateDialogBox(left, right, dialog);
    }

    private void SetActive(bool active)
    {
        dialogBoxRoot.SetActive(active);
        isDialogBoxActive = active;
    }

    public void UpdateDialogBox(Sprite left, Sprite right, string dialog)
    {
        leftProtraitImage.sprite = left;
        rightProtraitImage.sprite = right;
        dialogText.text = dialog;
    }
}
