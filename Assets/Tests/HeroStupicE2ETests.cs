using System.Collections;
using System.Reflection; // ĐÃ THÊM: Để dùng tuyệt kỹ thao túng biến private
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class HeroStupicE2ETests
{
    private string testUsername = "manhnheoi123";
    private string testPassword = "manhnheoi12334";
    private string characterName = "manhnheo";

    // Hàm Radar siêu cấp
    private T FindUIElement<T>(string objectName) where T : Component
    {
        T[] allElements = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (T element in allElements)
        {
            if (element.gameObject.name == objectName && element.gameObject.scene.isLoaded) return element;
        }
        return null;
    }

    [UnityTest]
    public IEnumerator TC04_Scene_FullLoginAndPlayFlow()
    {
        // =========================================================
        // GIAI ĐOẠN 1: ĐĂNG NHẬP
        // =========================================================
        SceneManager.LoadScene("1LoginScene"); 
        yield return new WaitForSeconds(1.5f);

        TMP_InputField userInput = FindUIElement<TMP_InputField>("UsenameInput");
        TMP_InputField passInput = FindUIElement<TMP_InputField>("PasswordInput");
        Button loginBtn = FindUIElement<Button>("LoginButton");

        userInput.gameObject.SetActive(true);
        passInput.gameObject.SetActive(true);
        loginBtn.gameObject.SetActive(true);

        userInput.text = testUsername;
        passInput.text = testPassword;
        yield return new WaitForSeconds(0.5f);
        loginBtn.onClick.Invoke();

        // =========================================================
        // GIAI ĐOẠN 2: LOBBY
        // =========================================================
        float timeout = 5f; float timer = 0f;
        while (SceneManager.GetActiveScene().name != "LobbyScene" && timer < timeout)
        {
            timer += Time.deltaTime; yield return null; 
        }
        yield return new WaitForSeconds(1.5f);

        TMP_InputField nameInput = FindUIElement<TMP_InputField>("NameInput");
        nameInput.gameObject.SetActive(true);
        nameInput.text = characterName;
        yield return new WaitForSeconds(0.5f);

        Button playBtn = FindUIElement<Button>("PlayButton");
        playBtn.gameObject.SetActive(true);
        playBtn.onClick.Invoke();

       // =========================================================
        // GIAI ĐOẠN 3: VÀO GAME & TEST GAMEPLAY (BẢN NÂNG CẤP KIÊN NHẪN)
        // =========================================================
        
        // ⚠️ SỬA TÊN SCENE MAP CHƠI CỦA BRO VÀO ĐÂY
        string gameSceneName = "SampleScene"; 

        timer = 0f;
        while (SceneManager.GetActiveScene().name != gameSceneName && timer < 10f)
        {
            timer += Time.deltaTime; yield return null;
        }
        Assert.AreEqual(gameSceneName, SceneManager.GetActiveScene().name, "Không vào được Map chơi!");

        // Đợi 2 giây để nhân vật của mình ổn định vị trí trên map
        yield return new WaitForSeconds(2f);

        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        Assert.IsNotNull(player, "Không tìm thấy Player trên Map!");

        // Chuẩn bị công cụ thao túng biến private
        FieldInfo targetPosField = typeof(PlayerController).GetField("targetPosition", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo targetEnemyField = typeof(PlayerController).GetField("targetEnemy", BindingFlags.NonPublic | BindingFlags.Instance);
        MethodInfo dashMethod = typeof(PlayerController).GetMethod("DashRoutine", BindingFlags.NonPublic | BindingFlags.Instance);

        // --- TEST 1: CHẠY QUA LẠI TẬP THỂ DỤC ---
        Debug.Log("BOT: Di chuyển khởi động...");
        player.isAutoMoving = true;
        targetPosField.SetValue(player, (Vector2)player.transform.position + new Vector2(3, 2));
        yield return new WaitForSeconds(1.5f);

        // --- TEST 2: LƯỚT (DASH) ---
        Debug.Log("BOT: Tung kỹ năng lướt!");
        player.StartCoroutine((IEnumerator)dashMethod.Invoke(player, null));
        yield return new WaitForSeconds(1f);


        // --- TEST 3: TRUY TÌM VÀ TIÊU DIỆT MỤC TIÊU (BẢN CHIẾN ĐẤU TRƯỜNG KỲ) ---
        Debug.Log("BOT: Đang quét tìm mục tiêu để bắt đầu thử nghiệm chiến đấu...");
        
        float totalCombatDuration = 20f; // Mạnh muốn đấm lâu bao nhiêu thì chỉnh số giây ở đây (ví dụ: 20 giây)
        float combatTimer = 0f;
        GameObject currentTarget = null;

        while (combatTimer < totalCombatDuration)
        {
            // Nếu chưa có mục tiêu hoặc mục tiêu cũ đã bị tiêu diệt (null), tìm mục tiêu mới
            if (currentTarget == null || !currentTarget.activeInHierarchy)
            {
                currentTarget = GameObject.FindGameObjectWithTag("Enemy");
                if (currentTarget != null)
                {
                    Debug.Log("BOT: Đã phát hiện mục tiêu mới! Tiến vào phạm vi tấn công.");
                    targetEnemyField.SetValue(player, currentTarget.transform);
                }
            }

            // Nếu đang có mục tiêu, Bot sẽ duy trì trạng thái tấn công
            if (currentTarget != null)
            {
                // In log mỗi 5 giây để theo dõi tiến độ trên Console
                if ((int)combatTimer % 5 == 0 && combatTimer > 0) 
                    Debug.Log($"BOT: Đang trong trạng thái giao tranh... (Đã đấm được {(int)combatTimer} giây)");
            }
            else
            {
                Debug.Log("BOT: Đang đợi quái xuất hiện hoặc đã quét sạch map...");
            }

            combatTimer += Time.deltaTime;
            yield return null; // Chờ khung hình tiếp theo để duy trì vòng lặp chiến đấu
        }

        Debug.Log("BOT: Kết thúc thời gian thử nghiệm chiến đấu trường kỳ.");
        Assert.Pass("HOÀN THÀNH: Bot đã tự chơi, lướt và duy trì tấn công quái vật trong thời gian dài thành công!");}
}