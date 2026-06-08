using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Mục tiêu theo dõi")]
    public Transform target; // Nhân vật mà camera sẽ bám theo

    [Header("Cài đặt Camera")]
    public float smoothSpeed = 5f; // Tốc độ bám (càng nhỏ càng trễ/mượt)
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Phải luôn lùi lại -10 ở trục Z để nhìn thấy game 2D

    // Dùng LateUpdate thay vì Update để đảm bảo Camera luôn di chuyển SAU KHI nhân vật đã đi xong
    void LateUpdate()
    {
        // Nếu chưa có mục tiêu thì đứng im
        if (target == null) return;

        // Tính toán vị trí camera cần đi tới
        Vector3 desiredPosition = target.position + offset;
        
        // Dùng Lerp để tạo cảm giác camera trượt đi mượt mà chứ không bị giật cục
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}