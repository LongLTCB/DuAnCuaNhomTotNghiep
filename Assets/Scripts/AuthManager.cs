using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI statusText;

    public void OnLoginButton()
    {
        statusText.text = "Đang đăng nhập...";
        var request = new LoginWithPlayFabRequest { Username = usernameInput.text, Password = passwordInput.text };
        PlayFabClientAPI.LoginWithPlayFab(request, 
            result => {
                statusText.color = Color.green;
                statusText.text = "Thành công! Đang vào Sảnh...";
                SceneManager.LoadScene(1); // Mở cổng sang Scene số 1 (Lobby)
            }, 
            error => { statusText.color = Color.red; statusText.text = "Lỗi: " + error.ErrorMessage; }
        );
    }

    public void OnRegisterButton()
    {
        statusText.text = "Đang tạo tài khoản...";
        var request = new RegisterPlayFabUserRequest { Username = usernameInput.text, Password = passwordInput.text, RequireBothUsernameAndEmail = false };
        PlayFabClientAPI.RegisterPlayFabUser(request, 
            result => { statusText.color = Color.green; statusText.text = "Đăng ký thành công! Hãy bấm Đăng Nhập."; }, 
            error => { statusText.color = Color.red; statusText.text = "Lỗi: " + error.ErrorMessage; }
        );
    }
}