using UnityEngine;
using Photon.Pun;

public class CameraFollowLimited : MonoBehaviour
{
    [Header("Mục tiêu theo dõi")]
    public Transform targetPlayer; 
    public float smoothSpeed = 0.125f; 
    public Vector3 offset = new Vector3(0, 0, -10f); 

    // --- NÂNG CẤP: DÙNG HÀNG RÀO TÀNG HÌNH THAY VÌ ẢNH NỀN ---
    [Header("Ranh giới Bản đồ (Khung tàng hình)")]
    public BoxCollider2D mapBounds; 

    private float minX, maxX, minY, maxY;
    private bool boundsSet = false;

    void Start()
    {
        CalculateBounds();
    }

    void FindMyPlayer()
    {
        // Quét tất cả các vật thể có Tag "Player" trên màn hình
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            PhotonView pv = p.GetComponent<PhotonView>();
            // Chỉ bắt lấy nhân vật CỦA MÌNH
            if (pv != null && pv.IsMine)
            {
                targetPlayer = p.transform;
                Debug.Log("<color=green>Camera: Đã tóm được nhân vật!</color>");
                break;
            }
        }
    }

    void CalculateBounds()
    {
        // Kiểm tra xem đã gắn BoxCollider2D vào chưa
        if (mapBounds == null)
        {
            Debug.LogError("<color=red>Camera: Chưa kéo thả khung giới hạn (GioiHan_Camera) vào ô Map Bounds!</color>");
            return;
        }

        // Lấy ranh giới từ Hàng rào tàng hình
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

    void LateUpdate()
    {
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