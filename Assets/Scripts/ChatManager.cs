using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections.Generic;
using TMPro;

public class ChatManager : MonoBehaviourPun
{
    [Header("Giao diện Thu gọn")]
    public TextMeshProUGUI miniChatText;     // Khung chữ nhỏ xíu bên ngoài (chỉ hiện 2 dòng)

    [Header("Giao diện Mở rộng")]
    public GameObject fullChatWindow; // Cái Panel nền đen chứa toàn bộ chat
    public TextMeshProUGUI fullChatText;         // Khung chữ to tổ chảng bên trong
    public TMP_InputField chatInput;      // Ô gõ chữ

    // Biến CỰC KỲ QUAN TRỌNG: Khóa nhân vật khi đang mở chat
    public static bool isChatOpen = false; 

    private List<string> chatHistory = new List<string>();

    void Start()
    {
        // Vừa vào game thì giấu khung chat to đi, xóa trắng khung chat nhỏ
        fullChatWindow.SetActive(false);
        miniChatText.text = "";
        fullChatText.text = "";
        isChatOpen = false;
    }

    void Update()
    {
        // BẤM ENTER ĐỂ MỞ HOẶC GỬI CHAT
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!isChatOpen)
            {
                OpenChatWindow();
            }
            else
            {
                // Nếu đã gõ chữ thì gửi
                if (chatInput.text.Trim() != "") 
                {
                    SendChat();
                }
                CloseChatWindow(); // Gửi xong hoặc không gõ gì thì đóng khung chat lại
            }
        }
    }

    void OpenChatWindow()
    {
        isChatOpen = true;
        fullChatWindow.SetActive(true); // Hiện bảng chat to lên
        chatInput.ActivateInputField(); // Tự động nhấp nháy con trỏ chuột vào ô gõ chữ
    }

    void CloseChatWindow()
    {
        isChatOpen = false;
        fullChatWindow.SetActive(false); // Giấu bảng chat to đi
        chatInput.text = ""; // Xóa trắng ô gõ chữ
    }

    public void SendChat()
    {
        string msg = chatInput.text;
        string senderName = PhotonNetwork.NickName;
        if (string.IsNullOrEmpty(senderName)) senderName = "Player " + PhotonNetwork.LocalPlayer.ActorNumber;

        string fullMessage = "<color=yellow>" + senderName + ":</color> " + msg;

        // Bắn tin nhắn lên mạng cho mọi người
        photonView.RPC("ReceiveChat", RpcTarget.All, fullMessage);
    }

    [PunRPC]
    void ReceiveChat(string msg)
    {
        // Lưu tin nhắn vào lịch sử
        chatHistory.Add(msg);

        // 1. CẬP NHẬT BẢNG CHAT TO (Hiện tất cả)
        fullChatText.text = string.Join("\n", chatHistory);

        // 2. CẬP NHẬT BẢNG CHAT NHỎ (Chỉ lấy 2 dòng cuối cùng)
        UpdateMiniChat();
    }

    void UpdateMiniChat()
    {
        int count = chatHistory.Count;
        miniChatText.text = "";
        
        // Nếu có từ 2 tin nhắn trở lên
        if (count >= 2)
        {
            miniChatText.text = chatHistory[count - 2] + "\n" + chatHistory[count - 1];
        }
        // Nếu mới chỉ có 1 tin nhắn
        else if (count == 1)
        {
            miniChatText.text = chatHistory[0];
        }
    }
}