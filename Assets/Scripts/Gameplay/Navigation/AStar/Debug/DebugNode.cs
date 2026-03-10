using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation.AStar
{    
    public enum DebugNodeType
    {
        Start,
        Target,
        Visited,
        Potential,
        Obstacle
    }
    public class DebugNode : MonoBehaviour
    {
        public Node nodeData;
        public SpriteRenderer spriteRenderer;

        public void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetPosition(Vector2 worldPos)
        {
            transform.position = worldPos;
        }

        public void UpdateVisual(DebugNodeType nodeType)
        {
            switch(nodeType)
            {
                case DebugNodeType.Start:
                    spriteRenderer.color = Color.blue;
                    break;
                case DebugNodeType.Target:
                    spriteRenderer.color = Color.magenta;
                    break;
                case DebugNodeType.Visited:
                    spriteRenderer.color = Color.green;
                    break;
                case DebugNodeType.Potential:
                    spriteRenderer.color = Color.yellow;
                    break;
                case DebugNodeType.Obstacle:
                    spriteRenderer.color = Color.red;
                    break;
                default:
                    spriteRenderer.color = Color.white;
                    break;
            }
        }
    }
}