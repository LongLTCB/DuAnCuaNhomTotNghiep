using UnityEngine;
using UnityEngine.UI; // Hoặc dùng TMPro nếu bạn xài TextMeshPro
using Photon.Pun;
using TMPro;

public class PlayerSetup : MonoBehaviourPun
{
    [Header("Bảng Tên Trên Đầu")]
    public TextMeshProUGUI nameText; // Kéo thả cái UI Text tên của nhân vật vào đây
    public Transform nameCanvasTransform; // Kéo thả CẢ CÁI CANVAS chứa tên vào đây (Quan trọng!)

    void Start()
    {
        // 1. Dán tên vào thẻ Text
        if (photonView.Owner != null)
        {
            nameText.text = photonView.Owner.NickName;
        }

        // --- TUYỆT CHIÊU SỬA LỖI TÊN LẬT NGƯỢC ---
        // Khi vừa sinh ra, ta ra lệnh cho cái Canvas chứa tên: "Hãy tách cha ra đi!"
        // Bằng cách này, khi nhân vật lật (quay mặt), cái Canvas đang trôi nổi không bị lật theo.
        if (nameCanvasTransform != null)
        {
            nameCanvasTransform.SetParent(null); // Tách cha! Chuyển khẩu ra khỏi nhân vật.
        }
    }

    // --- CHỐNG TỨC TỰ TỬ: DÙ ĐÃ TÁCH CHA, CÁI TÊN VẪN PHẢI BAY THEO ---
    void LateUpdate()
    {
        // Ta dùng LateUpdate để Camera chạy xong rồi ta mới tính vị trí cho mượt
        if (nameCanvasTransform != null)
        {
            // Canvas (đã tách cha) vẫn phải di chuyển theo vị trí của cục cha cũ
            nameCanvasTransform.position = transform.position + new Vector3(0, 1.5f, 0); // Cộng offset cho mượt
        }
    }

    // --- CHỐNG "HỒN LÌA KHỎI XÁC": KHI XÁC CHẾT THÌ HỒN (CÁI TÊN) PHẢI BIẾN MẤT ---
    void OnDestroy()
    {
        // Nếu người chơi chết (Destory), ta phải tự tay xóa cái tên đang trôi nổi đi
        if (nameCanvasTransform != null)
        {
            Destroy(nameCanvasTransform.gameObject);
        }
    }
}