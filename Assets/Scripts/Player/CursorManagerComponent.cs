using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorManagerComponent : MonoBehaviour
{
    public Sprite defaultCursor, tool, dialogue, interact;
    private Sprite currentCursor;
    private Image cursorImage;

    void Awake()
    {
    }
    void Start()
    {
        Cursor.visible = false; // Hide the default system cursor

        // Find the cursor GameObject and get the Image component of its child
        GameObject cursor = GameObject.FindWithTag("Cursor");
        cursorImage = cursor.GetComponent<Image>();

        // Set the default cursor image
        UpdateCursorImage(defaultCursor);
    }

    void UpdateCursorImage(Sprite newCursor)
    {
        currentCursor = newCursor;
        cursorImage.sprite = currentCursor;
    }

    void UpdateCursorPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        cursorImage.transform.position = mousePosition;
    }

    void Update()
    {
        UpdateCursorPosition();
    } 
}
