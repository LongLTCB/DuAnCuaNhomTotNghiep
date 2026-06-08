using UnityEngine;

public class HealthBarFix : MonoBehaviour
{
    private Vector3 originalScale;

    void Start()
    {
        // Lưu lại kích thước chuẩn ban đầu (ví dụ 0.02, 0.02, 0.02)
        originalScale = transform.localScale; 
    }

    void Update()
    {
        // Luôn giữ Scale X ở số dương, bất chấp nhân vật mẹ có quay đi đâu
        transform.localScale = new Vector3(
            Mathf.Sign(transform.parent.localScale.x) * originalScale.x, 
            originalScale.y, 
            originalScale.z
        );
    }
}