using UnityEngine;
using TMPro; // Thư viện để xài TextMeshPro
using UnityEngine.UI;
using Photon.Pun; // Thư viện mạng

public class PlayerUI : MonoBehaviourPun
{
    [Header("Kéo thả UI trên đỉnh đầu vào đây")]
    public TextMeshProUGUI nameText;
    public Image healthFill;

    void Start()
    {
        // Kiểm tra xem nhân vật này có chủ nhân trên mạng không
        if (photonView.Owner != null)
        {
            // Lấy Nickname từ hệ thống Photon gán thẳng vào cái bảng tên
            nameText.text = photonView.Owner.NickName;
        }
        else
        {
            nameText.text = "NPC Ẩn Danh";
        }
    }

    // Hàm này bro gọi khi bị quái cắn
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        // Tính phần trăm máu (từ 0 đến 1)
        healthFill.fillAmount = currentHealth / maxHealth;
    }
}