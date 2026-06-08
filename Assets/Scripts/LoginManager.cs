using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public class LoginManager : MonoBehaviourPunCallbacks
{
    [Header("Giao diện Đăng nhập")]
    public TMP_InputField nameInput;
    public TMP_Dropdown serverDropdown;
    public Button playButton;
    public TextMeshProUGUI statusText;

    [Header("Tên Scene Game Của Bạn")]
    // ⚠️ QUAN TRỌNG: Gõ chính xác tên Scene chơi game có bãi cỏ của bạn vào đây
    public string gameSceneName = "SampleScene"; 

    void Start()
    {
        // Lệnh bắt buộc: Tự động đồng bộ Scene cho tất cả mọi người trong phòng
        PhotonNetwork.AutomaticallySyncScene = true;
        statusText.text = "Vui lòng nhập tên và chọn Server!";
    }

    public void OnClickPlayButton()
    {
        string playerName = nameInput.text.Trim();

        // 1. CHỐNG GIAN LẬN: Bắt buộc phải nhập tên
        if (string.IsNullOrEmpty(playerName))
        {
            statusText.text = "<color=red>Lỗi: Bạn chưa nhập tên!</color>";
            return;
        }

        // Khóa nút lại để không bị bấm đúp
        playButton.interactable = false;
        statusText.text = "Đang kết nối tới Server...";

        // 2. LƯU TÊN NGƯỜI CHƠI VÀO HỆ THỐNG PHOTON
        PhotonNetwork.NickName = playerName;

        // 3. PHÂN CHIA SERVER BẰNG GAME VERSION
        // Dropdown trả về 0, 1, 2 tương ứng với dòng 1, 2, 3
        int serverIndex = serverDropdown.value + 1; 
        PhotonNetwork.GameVersion = "SV" + serverIndex.ToString(); // Kết quả: "SV1", "SV2", "SV3"

        // 4. TIẾN HÀNH KẾT NỐI
        PhotonNetwork.ConnectUsingSettings();
    }

    // --- CÁC HÀM TỰ ĐỘNG CHẠY KHI KẾT NỐI MẠNG ---

    public override void OnConnectedToMaster()
    {
        statusText.text = "Đã vào " + PhotonNetwork.GameVersion + "! Đang tìm phòng...";
        
        // Cùng 1 Server (GameVersion) sẽ chui chung vào phòng "MainWorld"
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 20 };
        PhotonNetwork.JoinOrCreateRoom("MainWorld", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "<color=green>Thành công! Đang tải map...</color>";
        
        // Chỉ có Master Client (Người tạo phòng) mới LoadLevel để kéo mọi người theo
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(gameSceneName);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        playButton.interactable = true;
        statusText.text = "<color=red>Mất kết nối: " + cause.ToString() + "</color>";
    }
}