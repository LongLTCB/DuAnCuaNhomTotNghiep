using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI Bảng Xếp Hạng")]
    public GameObject leaderboardPanel; // Cái Panel chứa danh sách
    public Transform container;         // Cái Content của ScrollView để chứa các dòng điểm
    public GameObject rowPrefab;        // Một cái Prefab dòng (Tên - Điểm)
    public TextMeshProUGUI statusText;

    // ==========================================
    // 1. HÀM GỬI ĐIỂM LÊN SERVER
    // ==========================================
    public void SubmitScore(int score)
    {
        var request = new UpdatePlayerStatisticsRequest { 
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate {
                    StatisticName = "HighScore", // Phải khớp 100% với tên trên Web
                    Value = score
                }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, 
            result => { Debug.Log("Đã cập nhật điểm số thành công!"); },
            error => { Debug.LogError("Lỗi gửi điểm: " + error.ErrorMessage); }
        );
    }

    // ==========================================
    // 2. HÀM LẤY BẢNG XẾP HẠNG VỀ
    // ==========================================
    public void GetLeaderboard()
    {
        // Hiện bảng lên và dọn dẹp danh sách cũ
        leaderboardPanel.SetActive(true);
        foreach (Transform child in container) { Destroy(child.gameObject); }

        var request = new GetLeaderboardRequest {
            StatisticName = "HighScore",
            StartPosition = 0,
            MaxResultsCount = 10 // Lấy Top 10 người thôi
        };

        PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderboardSuccess, OnError);
    }

    void OnGetLeaderboardSuccess(GetLeaderboardResult result)
    {
        foreach (var item in result.Leaderboard)
        {
            // Tạo một dòng mới trong danh sách UI
            GameObject newRow = Instantiate(rowPrefab, container);
            
            // Tìm các Text bên trong Prefab để điền thông tin
            // Giả sử Prefab có 2 Text: RankText và ScoreText
            TextMeshProUGUI[] texts = newRow.GetComponentsInChildren<TextMeshProUGUI>();
            
            // Điền hạng (bắt đầu từ 0 nên +1), Tên hiển thị và Điểm
            string displayName = string.IsNullOrEmpty(item.DisplayName) ? "Ẩn danh" : item.DisplayName;
            texts[0].text = (item.Position + 1).ToString() + ". " + displayName;
            texts[1].text = item.StatValue.ToString();
        }
    }

    void OnError(PlayFabError error) { statusText.text = "Lỗi: " + error.ErrorMessage; }

    public void CloseLeaderboard() { leaderboardPanel.SetActive(false); }
}