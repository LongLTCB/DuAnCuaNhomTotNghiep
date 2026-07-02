using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapFullMapAutoFit : MonoBehaviour
{
    [Header("Fit Settings")]
    [SerializeField] private float padding = 2f;
    [SerializeField] private float cameraZ = -10f;

    [Header("Wait Ground Data")]
    [SerializeField] private float retryDelay = 0.1f;
    [SerializeField] private int maxRetries = 120;

    private Camera miniMapCamera;

    private IEnumerator Start()
    {
        miniMapCamera = GetComponent<Camera>();
        if (miniMapCamera == null)
        {
            Debug.LogError("MiniMapFullMapAutoFit: Khong tim thay Camera tren object nay.");
            yield break;
        }

        if (!miniMapCamera.orthographic)
        {
            miniMapCamera.orthographic = true;
        }

        yield return WaitForGroundPositions();
        FitToGroundNow();
    }

    private IEnumerator WaitForGroundPositions()
    {
        int retry = 0;

        while ((GroundPositionManager.groundPositions == null || GroundPositionManager.groundPositions.Count == 0) &&
               retry < maxRetries)
        {
            GroundPositionManager manager = FindObjectOfType<GroundPositionManager>();
            if (manager != null)
            {
                manager.RefreshGroundPositions();
            }

            retry++;
            yield return new WaitForSeconds(retryDelay);
        }

        if (GroundPositionManager.groundPositions == null || GroundPositionManager.groundPositions.Count == 0)
        {
            Debug.LogError("MiniMapFullMapAutoFit: Khong co ground positions de tinh minimap.");
        }
    }

    [ContextMenu("Fit MiniMap To Ground Now")]
    public void FitToGroundNow()
    {
        if (miniMapCamera == null)
        {
            miniMapCamera = GetComponent<Camera>();
        }

        List<Vector3> grounds = GroundPositionManager.groundPositions;
        if (grounds == null || grounds.Count == 0)
        {
            Debug.LogWarning("MiniMapFullMapAutoFit: Ground positions rong, khong the fit minimap.");
            return;
        }

        Vector3 min = grounds[0];
        Vector3 max = grounds[0];

        for (int i = 1; i < grounds.Count; i++)
        {
            min = Vector3.Min(min, grounds[i]);
            max = Vector3.Max(max, grounds[i]);
        }

        Vector3 center = (min + max) * 0.5f;
        float width = (max.x - min.x) + padding * 2f;
        float height = (max.y - min.y) + padding * 2f;

        transform.position = new Vector3(center.x, center.y, cameraZ);

        float sizeByHeight = height * 0.5f;
        float sizeByWidth = (width * 0.5f) / Mathf.Max(0.0001f, miniMapCamera.aspect);
        miniMapCamera.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
    }
}