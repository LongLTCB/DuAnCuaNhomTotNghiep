using UnityEngine;
using Photon.Pun;
using UnityEngine.EventSystems; // BẮT BUỘC CÓ ĐỂ CHỐNG CLICK XUYÊN UI

public class TargetSelector : MonoBehaviour
{
    public LayerMask targetLayers; 
    public static GameObject currentTarget; 

    void Update()
    {
        // --- SỬA LỖI 2: Tự động ẩn bảng mục tiêu nếu con quái đã chết (bị xóa mất xác) ---
        if (currentTarget == null && UIManager.instance.targetPanel.activeSelf)
        {
            UIManager.instance.HideTarget();
            return; // Dừng lại luôn, không chạy các code check máu ở dưới nữa
        }

        // 1. KHI BẤM CHUỘT TRÁI CHỌN MỤC TIÊU
        if (Input.GetMouseButtonDown(0))
        {
            // --- SỬA LỖI 3: Nếu chuột đang chỉ vào Giao Diện (Nút bấm, Panel...) thì lờ đi, giữ nguyên mục tiêu! ---
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos, targetLayers);

            if (hit != null)
            {
                currentTarget = hit.gameObject;

                if (currentTarget.CompareTag("Player"))
                {
                    PlayerHealth ph = currentTarget.GetComponent<PlayerHealth>();
                    PhotonView pv = currentTarget.GetComponent<PhotonView>();
                    
                    if (ph != null && pv != null)
                    {
                        // Lấy máu người chơi
                        UIManager.instance.ShowTarget("Player " + pv.Owner.ActorNumber, ph.GetCurrentHealth(), ph.GetMaxHealth()); 
                    }
                }
                else if (currentTarget.CompareTag("Enemy"))
                {
                    EnemyHealth eh = currentTarget.GetComponent<EnemyHealth>();
                    if (eh != null)
                    {
                        // --- SỬA LỖI 1: Gọi thẳng biến eh.maxHealth thay vì dùng hàm GetMaxHealth() ---
                        UIManager.instance.ShowTarget("Quái vật", eh.GetCurrentHealth(), eh.maxHealth);
                    }
                }
            }
            else
            {
                // Chỉ khi thực sự click ra đất trống mới hủy mục tiêu
                currentTarget = null;
                UIManager.instance.HideTarget();
            }
        }

        // 2. LIÊN TỤC CẬP NHẬT MÁU KHI ĐANG NHÌN MỤC TIÊU
        if (currentTarget != null && UIManager.instance.targetPanel.activeSelf)
        {
            if (currentTarget.CompareTag("Player"))
            {
                PlayerHealth ph = currentTarget.GetComponent<PlayerHealth>();
                if (ph != null) UIManager.instance.UpdateTargetHealth(ph.GetCurrentHealth());
            }
            else if (currentTarget.CompareTag("Enemy"))
            {
                EnemyHealth eh = currentTarget.GetComponent<EnemyHealth>();
                if (eh != null) UIManager.instance.UpdateTargetHealth(eh.GetCurrentHealth());
            }
        }
    }
}