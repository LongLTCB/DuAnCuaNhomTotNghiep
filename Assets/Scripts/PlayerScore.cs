using UnityEngine;
using TMPro; 
using Photon.Pun;

public class PlayerScore : MonoBehaviourPun
{
    public int currentScore = 0;
    
    [Header("Giao diện UI")]
    public TextMeshProUGUI scoreText;

    void Start()
    {
        // Tự động tìm Text điểm trên màn hình (Bro nhớ tạo 1 cái TextMeshPro tên "MainScoreText" ngoài Scene)
        if (photonView.IsMine)
        {
            GameObject uiDiem = GameObject.Find("MainScoreText");
            if (uiDiem != null) scoreText = uiDiem.GetComponent<TextMeshProUGUI>();
            UpdateScoreUI();
        }
    }

    public void CongDiem(int diemThuong)
{
    if (!photonView.IsMine) return; 

    currentScore += diemThuong;
    UpdateScoreUI();

    // ĐÃ THÊM: Cứ có điểm mới là gửi lên Server luôn
    NopDiemTruocKhiChet(); 
}
    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Điểm: " + currentScore;
    }

    public void NopDiemTruocKhiChet()
    {
        if (photonView.IsMine)
            FindObjectOfType<LeaderboardManager>().SubmitScore(currentScore);
    }
}