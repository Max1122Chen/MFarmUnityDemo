using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace InventorySystem
{
    public class PlayerInventoryComponent : InventoryComponent
    {
        public PlayerController pc;


        [Header("Hotbar Settings")]
        [SerializeField] private int hotBarSize = 0;
        public int selectedHotBarIndex = -1;   // -1 means no hotbar slot is selected.

        public Action<int, int> onSelectedHotBarIndexChanged;

        public int HotBarSize
        {
            get { return hotBarSize; }
        }

        public override void Awake()
        {
            base.Awake();
            pc = GetComponent<PlayerController>();
        }
        public override void Start()
        {
            base.Start();

        }

        public override void SetSelectedSlotIndex(int newIndex)
        {
            base.SetSelectedSlotIndex(newIndex);
            SetSelectedHotBarIndex(newIndex);
        }
        
        private void SetSelectedHotBarIndex(int newIndex)
        {
            if(-1 < newIndex && newIndex < hotBarSize)
            {
                int oldIndex = selectedHotBarIndex;
                selectedHotBarIndex = oldIndex == newIndex ? -1 : newIndex;   // Toggle selection if the same index is selected again.
                // Debug.Log($"Selected hotbar index changed, from {oldIndex} to {selectedHotBarIndex}");
                onSelectedHotBarIndexChanged?.Invoke(oldIndex, selectedHotBarIndex);
            }
            else
            {
                int oldIndex = selectedHotBarIndex;
                selectedHotBarIndex = -1;
                // Debug.Log($"Selected hotbar index changed, from {oldIndex} to -1 (invalid index)");
                onSelectedHotBarIndexChanged?.Invoke(oldIndex, -1);
            }
        }

        public int RollHotBarIndex(int direction)
        {
            // TODO: Add condition to prevent changing hotbar index when player is performing certain actions (e.g. attacking, casting spell, etc.)

             // Do not change hotbar index if inventory is open.
            if(IsOpen == true)
            {
                return selectedHotBarIndex;
            }

            if(selectedHotBarIndex == -1)
            {
                if(direction > 0)
                {
                    SetSelectedSlotIndex(0);
                }
                else
                {
                    SetSelectedSlotIndex(hotBarSize - 1);
                }
            }
            else
            {
                int newIndex = selectedSlotIndex + direction;
                newIndex = (newIndex + hotBarSize) % hotBarSize; // Wrap around the hotbar index.
                SetSelectedSlotIndex(newIndex);
            }
            
            return selectedHotBarIndex;
        }

        public void SelectHotBarSlotByHotkey(int hotkeyNumber)
        {
            if(hotkeyNumber >= 0 && hotkeyNumber < hotBarSize)
            {
                SetSelectedSlotIndex(hotkeyNumber);
            }
        }
    }

}
