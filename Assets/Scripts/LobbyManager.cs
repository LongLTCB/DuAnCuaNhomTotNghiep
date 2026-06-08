using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Giao Diện Sảnh")]
    public TMP_InputField nameInput;
    public TMP_Dropdown serverDropdown;
    public Button playButton;
    public TextMeshProUGUI statusText;

    [Header("Giao Diện Chọn Class")]
    public GameObject classSelectionPanel; // Cái bảng bự chứa hình nhân vật
    public TextMeshProUGUI classButtonText; // Chữ trên nút CLASS ngoài màn hình
    public string selectedClassName = "Class_Warrior"; 

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        classSelectionPanel.SetActive(false);
        statusText.text = "Đang tải hồ sơ...";
        playButton.interactable = false;

        // Tự động kéo dữ liệu cũ trên mạng về điền sẵn cho người chơi
        LoadPlayerData();
    }

    void LoadPlayerData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), 
            result => {
                playButton.interactable = true;
                statusText.text = "Sẵn sàng!";
                if (result.Data != null)
                {
                    if (result.Data.ContainsKey("SavedName")) nameInput.text = result.Data["SavedName"].Value;
                    if (result.Data.ContainsKey("SavedClass")) ChooseClass(result.Data["SavedClass"].Value);
                }
            }, 
            error => {
                playButton.interactable = true;
                statusText.text = "Tài khoản mới, hãy tạo nhân vật!";
            }
        );
    }

    // --- CÁC HÀM BẬT TẮT BẢNG CHỌN CLASS ---
    public void OpenClassPanel() { classSelectionPanel.SetActive(true); }
    public void CloseClassPanel() { classSelectionPanel.SetActive(false); }

    public void ChooseClass(string className)
    {
        selectedClassName = className;
        if (classButtonText != null) classButtonText.text = "CLASS: " + className.Replace("Class_", "").ToUpper();
        CloseClassPanel();
    }

    // --- KHI BẤM NÚT VÀO GAME ---
    public void OnClickPlayButton()
    {
        if (string.IsNullOrEmpty(nameInput.text)) { statusText.text = "Nhập tên đi bro!"; return; }

        playButton.interactable = false;
        statusText.text = "Đang lưu cấu hình...";
        string serverVer = "SV" + (serverDropdown.value + 1).ToString();

        // 1. Lưu Tên, Server, Class lên PlayFab
        var request = new UpdateUserDataRequest {
            Data = new Dictionary<string, string> {
                { "SavedName", nameInput.text }, { "SavedServer", serverVer }, { "SavedClass", selectedClassName }
            }
        };

        PlayFabClientAPI.UpdateUserData(request, 
            result => {
                // 2. Lưu thành công thì Kết nối Photon
                PlayerPrefs.SetString("MySelectedClass", selectedClassName); // Lưu tạm ổ cứng để tẹo nữa đẻ nhân vật
                PhotonNetwork.NickName = nameInput.text;
                PhotonNetwork.GameVersion = serverVer;
                statusText.text = "Đang kết nối Photon...";
                PhotonNetwork.ConnectUsingSettings();
            }, 
            error => { statusText.text = "Lỗi lưu mạng!"; playButton.interactable = true; }
        );
    }

    // --- PHOTON CALLBACKS ---
    public override void OnConnectedToMaster()
    {
        statusText.text = "Đang tìm phòng...";
        PhotonNetwork.JoinOrCreateRoom("MainWorld", new RoomOptions() { MaxPlayers = 20 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        // 1. In ra Console xem bro có quyền Chủ Phòng không
        Debug.Log("Vào phòng thành công! Tôi có phải Chủ Phòng (Master Client) không? " + PhotonNetwork.IsMasterClient);
        
        if (PhotonNetwork.IsMasterClient)
        {
            statusText.text = "<color=green>Thành công! Đang kéo mọi người sang Scene 2...</color>";
            Debug.Log("Đang gọi lệnh LoadLevel(2)...");
            
            // Lệnh chuyển cảnh
            PhotonNetwork.LoadLevel(2); 
        }
        else
        {
            statusText.text = "<color=yellow>Đang chờ Chủ Phòng tải Map...</color>";
            Debug.Log("Tôi không phải Chủ Phòng, đứng im đợi lệnh.");
        }
    }
}