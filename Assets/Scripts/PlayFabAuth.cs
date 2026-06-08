using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro; 
using UnityEngine.SceneManagement; 
using System.Collections.Generic; 

public class PlayFabAuth : MonoBehaviour
{
    [Header("Quản lý Cửa sổ (Panels)")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject resetPasswordPanel; // THÊM PANEL QUÊN MẬT KHẨU
    public TextMeshProUGUI messageText;  

    [Header("Giao diện ĐĂNG NHẬP")]
    public TMP_InputField loginUsername; 
    public TMP_InputField loginPassword; 

    [Header("Giao diện ĐĂNG KÝ")]
    public TMP_InputField regUsername; 
    public TMP_InputField regEmail;    
    public TMP_InputField regPhone;    
    public TMP_InputField regPassword; 

    [Header("Giao diện QUÊN MẬT KHẨU")]
    public TMP_InputField resetEmailInput; // THÊM Ô NHẬP EMAIL ĐỂ KHÔI PHỤC

    void Start()
    {
        OpenLoginPanel();
    }

    // ==========================================
    // HÀM BẬT / TẮT PANEL
    // ==========================================
    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        resetPasswordPanel.SetActive(false); // Tắt luôn panel reset
        ShowMessage("", Color.white);
    }

    public void OpenRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        resetPasswordPanel.SetActive(false); 
        ShowMessage("", Color.white);
    }

    public void OpenResetPasswordPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        resetPasswordPanel.SetActive(true); // Bật panel reset lên
        ShowMessage("", Color.white);
    }

    // ==========================================
    // HÀM XỬ LÝ QUÊN MẬT KHẨU (Gửi Email)
    // ==========================================
    public void OnResetPasswordButtonClicked()
    {
        if (!resetEmailInput.text.Contains("@"))
        {
            ShowMessage("Vui lòng nhập Email hợp lệ!", Color.red);
            return;
        }

        ShowMessage("Đang gửi email khôi phục...", Color.yellow);

        var request = new SendAccountRecoveryEmailRequest
        {
            Email = resetEmailInput.text,
            TitleId = PlayFabSettings.TitleId // Tự động lấy mã 1552A7 của bro
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordResetSuccess, OnError);
    }

    void OnPasswordResetSuccess(SendAccountRecoveryEmailResult result)
    {
        ShowMessage("Đã gửi link khôi phục! Hãy kiểm tra Hộp thư Email của bạn.", Color.green);
        OpenLoginPanel(); // Gửi xong thì tự đẩy về màn hình đăng nhập
    }

    // ==========================================
    // HÀM XỬ LÝ ĐĂNG KÝ
    // ==========================================
    public void OnRegisterButtonClicked()
    {
        if (regUsername.text.Length < 3 || !regEmail.text.Contains("@") || regPassword.text.Length < 6)
        {
            ShowMessage("Vui lòng nhập đúng định dạng (Tên > 2, Pass > 5, có @ ở Email)!", Color.red);
            return;
        }

        ShowMessage("Đang tạo tài khoản...", Color.yellow);

        var request = new RegisterPlayFabUserRequest
        {
            Username = regUsername.text,
            Email = regEmail.text,
            Password = regPassword.text,
            RequireBothUsernameAndEmail = true 
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        var nameRequest = new UpdateUserTitleDisplayNameRequest { DisplayName = regUsername.text };
        PlayFabClientAPI.UpdateUserTitleDisplayName(nameRequest, 
            (res) => {
                if (!string.IsNullOrEmpty(regPhone.text))
                {
                    var dataRequest = new UpdateUserDataRequest {
                        Data = new Dictionary<string, string> { { "PhoneNumber", regPhone.text } }
                    };
                    PlayFabClientAPI.UpdateUserData(dataRequest, (dataRes) => { CompleteRegistration(); }, OnError);
                }
                else
                {
                    CompleteRegistration();
                }
            }, 
            OnError);
    }

    void CompleteRegistration()
    {
        ShowMessage("Đăng ký thành công!", Color.green);
        OpenLoginPanel(); 
    }

    // ==========================================
    // HÀM XỬ LÝ ĐĂNG NHẬP
    // ==========================================
    public void OnLoginButtonClicked()
    {
        if (loginUsername.text.Length == 0 || loginPassword.text.Length == 0)
        {
            ShowMessage("Vui lòng điền đủ Tên và Mật khẩu!", Color.red);
            return;
        }

        ShowMessage("Đang kiểm tra thông tin...", Color.yellow);

        var request = new LoginWithPlayFabRequest
        {
            Username = loginUsername.text,
            Password = loginPassword.text
        };

        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        ShowMessage("Đăng nhập thành công! Đang vào game...", Color.green);
        SceneManager.LoadScene(1); 
    }

    void OnError(PlayFabError error)
    {
        ShowMessage("Lỗi: " + error.ErrorMessage, Color.red);
    }

    void ShowMessage(string msg, Color color)
    {
        messageText.color = color;
        messageText.text = msg;
    }
}