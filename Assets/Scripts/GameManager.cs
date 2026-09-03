using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    // Cài đặt để GameManager này biến thành Singleton (Dễ gọi từ bất cứ đâu)
    public static GameManager instance;

    [Header("Menu UI")]
    public GameObject pauseMenuPanel;
    public GameObject deathMenuPanel;

    private bool isPaused = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // 1. Nhấn ESC để bật/tắt Menu Cài Đặt (Chỉ bật khi chưa chết)
        if (Input.GetKeyDown(KeyCode.Escape) && !deathMenuPanel.activeInHierarchy)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // --- CÁC HÀM XỬ LÝ MENU ---

    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true);
        isPaused = true;
        // Chú ý: TUYỆT ĐỐI KHÔNG dùng Time.timeScale = 0 ở đây vì là game Multi!
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        isPaused = false;
    }

    // Hàm này sẽ được PlayerHealth gọi khi máu = 0
    public void ShowDeathMenu()
    {
        // Tắt menu pause đi (trường hợp đang pause mà bị quái đánh chết)
        pauseMenuPanel.SetActive(false); 
        
        deathMenuPanel.SetActive(true);
    }

    // --- CÁC HÀM NÚT BẤM (Gắn vào OnClick) ---

    public void LeaveRoom()
    {
        // Lệnh chuẩn của Photon để thoát khỏi phòng chơi
        PhotonNetwork.LeaveRoom();
    }

    // Hàm này sẽ tự động được gọi SAU KHI LeaveRoom() thực hiện xong
    public override void OnLeftRoom()
    {
        // Load lại Scene Sảnh chờ (Thay tên "LobbyScene" bằng tên Scene thực tế của bro)
        SceneManager.LoadScene("LobbyScene"); 
    }
    public bool IsPaused()
{
    return isPaused;
}
public void ExitGame()
{
    Debug.Log("Thoát game...");

    // Nếu đang chạy trong Unity Editor
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    // Khi build ra file .exe
    Application.Quit();
#endif
}

}