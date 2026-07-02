using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapFullMapAutoFit : MonoBehaviour
{
    [Header("Fit Settings")]
    [SerializeField]
    private float padding = 2f;

    [SerializeField]
    private float cameraZ = -10f;

    [Header("Wait Ground Data")]
    [SerializeField]
    private float retryDelay = 0.1f;

    [SerializeField]
    private int maxRetries = 120;

    private Camera miniMapCamera;
    [Header("Player Marker")]
    [SerializeField]
    private bool showPlayerMarker = true;

    [SerializeField]
    private Color markerColor = Color.red;

    [SerializeField]
    private float markerSize = 0.25f;

    private Transform playerTarget;
    private GameObject playerMarker;
    private Sprite markerSprite;

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

        if (showPlayerMarker)
        {
            CreatePlayerMarker();
        }
    }

    private IEnumerator WaitForGroundPositions()
    {
        int retry = 0;

        while ((GroundPositionManager.groundPositions == null || GroundPositionManager.groundPositions.Count == 0)
               && retry < maxRetries)
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

    private void LateUpdate()
    {
        if (!showPlayerMarker) return;

        if (playerTarget == null)
        {
            FindLocalPlayer();
            if (playerTarget == null) return;
        }

        if (playerMarker != null)
        {
            Vector3 pos = playerTarget.position;
            playerMarker.transform.position = new Vector3(pos.x, pos.y, cameraZ + 0.01f);
        }
    }

    private void FindLocalPlayer()
    {
        // Try Photon local player TagObject pattern if available
        try
        {
            var photonType = System.Type.GetType("Photon.Pun.PhotonNetwork, PhotonUnityNetworking");
            if (photonType != null)
            {
                var localPlayerProp = photonType.GetProperty("LocalPlayer");
                if (localPlayerProp != null)
                {
                    var localPlayer = localPlayerProp.GetValue(null, null);
                    if (localPlayer != null)
                    {
                        var tagObjProp = localPlayer.GetType().GetProperty("TagObject");
                        if (tagObjProp != null)
                        {
                            var tagged = tagObjProp.GetValue(localPlayer, null) as GameObject;
                            if (tagged != null)
                            {
                                playerTarget = tagged.transform;
                                return;
                            }
                        }
                    }
                }
            }
        }
        catch { }

        // Fallback: find a PhotonView that is mine
        var photonViews = FindObjectsOfType<UnityEngine.MonoBehaviour>();
        foreach (var mb in photonViews)
        {
            var pv = mb.GetComponent("Photon.Pun.PhotonView") as Component;
            if (pv == null) continue;
            var isMineProp = pv.GetType().GetProperty("IsMine");
            if (isMineProp != null && (bool)isMineProp.GetValue(pv, null))
            {
                playerTarget = (pv as Component).transform;
                return;
            }
        }
    }

    private void CreatePlayerMarker()
    {
        if (playerMarker != null) return;
        int size = 8;
        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        Color[] cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = markerColor;
        tex.SetPixels(cols);
        tex.Apply();
        markerSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);

        playerMarker = new GameObject("MiniMap_PlayerMarker");
        var sr = playerMarker.AddComponent<SpriteRenderer>();
        sr.sprite = markerSprite;
        sr.color = markerColor;
        sr.sortingOrder = 1000;
        playerMarker.transform.localScale = Vector3.one * markerSize;

        playerMarker.transform.position = new Vector3(transform.position.x, transform.position.y, cameraZ + 0.01f);

        int mask = miniMapCamera.cullingMask;
        if (mask != 0)
        {
            int layer = 0;
            for (int i = 0; i < 32; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    layer = i;
                    break;
                }
            }
            playerMarker.layer = layer;
        }
    }
}