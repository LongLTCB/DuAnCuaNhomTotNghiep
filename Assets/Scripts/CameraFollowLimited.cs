using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class CameraFollowLimited : MonoBehaviour
{
    [Header("Mục tiêu theo dõi")]
    public Transform targetPlayer; 
    public float smoothSpeed = 0.125f; 
    public Vector3 offset = new Vector3(0, 0, -10f); 

    // --- NÂNG CẤP: DÙNG HÀNG RÀO TÀNG HÌNH THAY VÌ ẢNH NỀN ---
    [Header("Ranh giới Bản đồ (Khung tàng hình)")]
    public BoxCollider2D mapBounds; 

    [SerializeField]
    private float edgePadding = 1f;

    private float minX, maxX, minY, maxY;
    private bool boundsSet = false;

    void Start()
    {
        CalculateBounds();
    }

    void FindMyPlayer()
    {
        if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.TagObject is GameObject taggedPlayer)
        {
            PhotonView taggedView = taggedPlayer.GetComponent<PhotonView>();
            if (taggedView != null && taggedView.IsMine)
            {
                targetPlayer = taggedPlayer.transform;
                Debug.Log("<color=green>Camera: Đã tóm được nhân vật!</color>");
                return;
            }
        }

        // Quét tất cả PhotonView trong scene và chọn đúng nhân vật của mình
        PhotonView[] players = FindObjectsOfType<PhotonView>();
        foreach (PhotonView pv in players)
        {
            if (pv != null && pv.IsMine && pv.CompareTag("Player"))
            {
                targetPlayer = pv.transform;
                Debug.Log("<color=green>Camera: Đã tóm được nhân vật!</color>");
                break;
            }
        }
    }

    void CalculateBounds()
    {
        if (TryCalculateBoundsFromGroundPositions())
        {
            boundsSet = true;
            return;
        }

        // Fallback nếu chưa có ground positions: dùng BoxCollider2D như cũ
        if (mapBounds == null)
        {
            Debug.LogError("<color=red>Camera: Chưa có ground bounds và cũng chưa kéo thả khung giới hạn (GioiHan_Camera) vào ô Map Bounds!</color>");
            return;
        }

        Bounds bgBounds = mapBounds.bounds;
        
        Camera cam = Camera.main;
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect; 

        float camHalfWidth = camWidth / 2f;
        float camHalfHeight = camHeight / 2f;

        minX = bgBounds.min.x + camHalfWidth;
        maxX = bgBounds.max.x - camHalfWidth;
        minY = bgBounds.min.y + camHalfHeight;
        maxY = bgBounds.max.y - camHalfHeight;

        // Xử lý chống lỗi nếu bản đồ vẽ quá nhỏ so với Camera
        if (minX > maxX) minX = maxX = bgBounds.center.x;
        if (minY > maxY) minY = maxY = bgBounds.center.y;

        boundsSet = true;
    }

    private bool TryCalculateBoundsFromGroundPositions()
    {
        if (GroundPositionManager.groundPositions == null || GroundPositionManager.groundPositions.Count == 0)
        {
            return false;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            return false;
        }

        Vector3 min = GroundPositionManager.groundPositions[0];
        Vector3 max = GroundPositionManager.groundPositions[0];

        foreach (Vector3 position in GroundPositionManager.groundPositions)
        {
            min = Vector3.Min(min, position);
            max = Vector3.Max(max, position);
        }

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        float camHalfWidth = camWidth / 2f;
        float camHalfHeight = camHeight / 2f;

        minX = min.x + edgePadding + camHalfWidth;
        maxX = max.x - edgePadding - camHalfWidth;
        minY = min.y + edgePadding + camHalfHeight;
        maxY = max.y - edgePadding - camHalfHeight;

        if (minX > maxX) minX = maxX = (min.x + max.x) * 0.5f;
        if (minY > maxY) minY = maxY = (min.y + max.y) * 0.5f;

        return true;
    }

    void LateUpdate()
    {
        if (!boundsSet)
        {
            CalculateBounds();
            if (!boundsSet) return;
        }

        // --- TÌM LIÊN TỤC MỖI KHUNG HÌNH NẾU CHƯA CÓ MỤC TIÊU ---
        if (targetPlayer == null)
        {
            FindMyPlayer();
            // Nếu tìm xong vẫn chưa có (do mạng chưa đẻ kịp) thì thoát, khung hình sau tìm tiếp
            if (targetPlayer == null) return; 
        }

        if (!boundsSet) return;

        Vector3 desiredPosition = targetPlayer.position + offset;

        // Ép vị trí Camera không được vượt qua ranh giới đã tính
        float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);

        Vector3 clampedPosition = new Vector3(clampedX, clampedY, desiredPosition.z);

        // Tạo độ mượt (Smooth) khi camera chạy theo
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, clampedPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}