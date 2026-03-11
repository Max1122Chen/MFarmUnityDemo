using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    // Component References
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private NPCAnimationBlueprint npcABP;

    // NPC Data Reference
    [SerializeField] private NPCData npcData;

    // Movement Parameters
    [SerializeField] private float moveSpeed = 2f;

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        npcABP = GetComponentInChildren<NPCAnimationBlueprint>();
    }

    public void Initialize(NPCData data)
    {
        npcData = data;
    }

    public void SetActiveInWorld(bool isActive)
    {
        spriteRenderer.enabled = isActive;
        npcABP.enabled = isActive;
    }

    public void Movement(Vector2 direction)
    {
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
    }
}
