using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class SetUpCameraBounds : MonoBehaviour
{
    void Start()
    {
        GameMapSubsystem.Instance.onNewSceneLoaded += SetUpConfinerShape;
    }
    private void SetUpConfinerShape()
    {
        PolygonCollider2D confinerShape = GameObject.FindGameObjectWithTag("CameraBounds").GetComponent<PolygonCollider2D>();

        CinemachineConfiner confiner = GetComponent<CinemachineConfiner>();
        confiner.m_BoundingShape2D = confinerShape;

        // Call this if the shape changes at runtime
        confiner.InvalidatePathCache();
    }
}
