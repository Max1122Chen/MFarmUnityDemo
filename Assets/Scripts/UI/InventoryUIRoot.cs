using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using UnityEngine;

public class InventoryUIRoot : MonoBehaviour
{
    void Start()
    {
        InventorySubsystem.Instance.InventoryUIRoot = this.gameObject;
    }

}
