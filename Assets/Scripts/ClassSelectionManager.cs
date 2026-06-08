using UnityEngine;
using TMPro; // Dùng để đổi chữ TextMeshPro

public class ClassSelectionManager : MonoBehaviour
{
    [Header("Các Bảng UI")]
    public GameObject loginPanel;           // Kéo LoginPanel vào đây
    public GameObject classSelectionPanel;  // Kéo ClassSelectionPanel vào đây
    public GameObject mannequinsGroup;      // Kéo Group_Mannequins vào đây

    [Header("Hiển thị (Tùy chọn)")]
    public TextMeshProUGUI selectedClassText; // Kéo chữ báo trạng thái vào đây

    void Start()
    {
        // Khi vừa mở game lên: Bật Login, Tắt mấy con mẫu đi
        loginPanel.SetActive(true);
        classSelectionPanel.SetActive(false);
        if (mannequinsGroup != null) mannequinsGroup.SetActive(false);
    }

    // Nút "Chọn Nhân Vật" ở sảnh sẽ gọi hàm này
    public void OpenClassSelection()
    {
        loginPanel.SetActive(false); // Giấu sảnh
        classSelectionPanel.SetActive(true); // Hiện 3 nút tàng hình
        if (mannequinsGroup != null) mannequinsGroup.SetActive(true); // Hiện 3 con mẫu
    }

    // 3 nút tàng hình sẽ gọi hàm này
    public void ChooseClass(string prefabName)
    {
        // 1. Ghi nhớ Class đã chọn
        PlayerPrefs.SetString("MySelectedClass", prefabName);
        PlayerPrefs.Save();

        // 2. Tắt màn hình chọn, Quay lại sảnh Đăng Nhập
        classSelectionPanel.SetActive(false);
        if (mannequinsGroup != null) mannequinsGroup.SetActive(false);
        loginPanel.SetActive(true);

        // 3. Cập nhật dòng chữ hiển thị ở sảnh
        if (selectedClassText != null)
        {
            if (prefabName == "Class_Warrior") selectedClassText.text = "Class: KNIGHT";
            else if (prefabName == "Class_Archer") selectedClassText.text = "Class: ARCHER";
            else if (prefabName == "Class_Mage") selectedClassText.text = "Class: MAGE";
        }
    }
}  