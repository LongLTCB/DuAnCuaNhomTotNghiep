using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Collections; // Thư viện bắt buộc để xài tính năng Nghỉ Nhịp (Coroutine)

public class FakeDataGenerator : MonoBehaviour
{
    string[] fakeNames = { "Knight_Hero", "Gosu_No1", "Dark_Lord", "Manh_Bot", "Pro_Player", "Solo_King" };

    public void GenerateFakeUsers(int count)
    {
        Debug.Log("<color=yellow>Đã nhận lệnh! Bắt đầu bơm từ từ " + count + " bots để không bị block...</color>");
        
        // Kích hoạt máy bơm chạy theo nhịp
        StartCoroutine(PumpBotsSlowly(count)); 
    }

    // Hàm tạo độ trễ (Coroutine)
    IEnumerator PumpBotsSlowly(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Trộn ID và Tên ngẫu nhiên để không bị trùng
            string customId = "FakeUser_" + Random.Range(10000, 99999);
            string displayName = fakeNames[Random.Range(0, fakeNames.Length)] + "_" + Random.Range(10, 99);
            int randomScore = Random.Range(100, 5000);

            CreateAndSubmit(customId, displayName, randomScore);

            // BÍ QUYẾT LÀ ĐÂY: Tạo xong 1 con, ép code nghỉ 1.5 giây rồi mới chạy vòng lặp tiếp
            yield return new WaitForSeconds(5f); 
        }
        
        Debug.Log("<color=yellow>ĐÃ BƠM XONG TOÀN BỘ BOT!</color>");
    }

    void CreateAndSubmit(string customId, string name, int score)
    {
        var loginReq = new LoginWithCustomIDRequest { CustomId = customId, CreateAccount = true };

        PlayFabClientAPI.LoginWithCustomID(loginReq, 
            loginResult => {
                PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = name }, null, null);
                
                var scoreReq = new UpdatePlayerStatisticsRequest {
                    Statistics = new List<StatisticUpdate> { new StatisticUpdate { StatisticName = "HighScore", Value = score } }
                };
                PlayFabClientAPI.UpdatePlayerStatistics(scoreReq, 
                    res => Debug.Log("<color=green>Đã bơm thành công: " + name + " - Điểm: " + score + "</color>"), 
                    null
                );
            }, 
            error => Debug.LogError("Lỗi Đăng nhập Bot: " + error.ErrorMessage)
        );
    }
}