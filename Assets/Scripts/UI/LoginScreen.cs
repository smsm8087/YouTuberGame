using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginScreen : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField playerNameInput;
    public TMP_InputField channelNameInput;
    public Button loginButton;
    public Button registerButton;
    public Button switchModeButton;
    public TextMeshProUGUI statusText;
    public GameObject registerPanel;

    private bool isRegisterMode = false;

    void Start()
    {
        // 자동 로그인 체크
        if (APIClient.Instance.IsLoggedIn)
        {
            statusText.text = "자동 로그인 중...";
            StartCoroutine(APIClient.Instance.GetPlayerData((success, response) =>
            {
                if (success)
                {
                    LoadMainScene();
                }
                else
                {
                    statusText.text = "토큰 만료. 다시 로그인하세요.";
                    APIClient.Instance.ClearToken();
                }
            }));
            return;
        }

        // 버튼 이벤트
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
        switchModeButton.onClick.AddListener(OnSwitchMode);

        // 초기 상태: 로그인 모드
        SetRegisterMode(false);
    }

    void OnLoginClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "이메일과 비밀번호를 입력하세요.";
            return;
        }

        statusText.text = "로그인 중...";
        loginButton.interactable = false;

        StartCoroutine(APIClient.Instance.Login(email, password, (success, response) =>
        {
            loginButton.interactable = true;

            if (success)
            {
                var auth = JsonUtility.FromJson<AuthResponse>(response);
                statusText.text = $"환영합니다! {auth.Message}";
                Invoke(nameof(LoadMainScene), 1f);
            }
            else
            {
                statusText.text = $"로그인 실패: {response}";
            }
        }));
    }

    void OnRegisterClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string playerName = playerNameInput.text.Trim();
        string channelName = channelNameInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(channelName))
        {
            statusText.text = "모든 항목을 입력하세요.";
            return;
        }

        if (password.Length < 6)
        {
            statusText.text = "비밀번호는 6자 이상이어야 합니다.";
            return;
        }

        statusText.text = "회원가입 중...";
        registerButton.interactable = false;

        StartCoroutine(APIClient.Instance.Register(email, password, playerName, channelName, (success, response) =>
        {
            registerButton.interactable = true;

            if (success)
            {
                var auth = JsonUtility.FromJson<AuthResponse>(response);
                statusText.text = $"가입 완료! {auth.Message}";
                Invoke(nameof(LoadMainScene), 1f);
            }
            else
            {
                statusText.text = $"회원가입 실패: {response}";
            }
        }));
    }

    void OnSwitchMode()
    {
        SetRegisterMode(!isRegisterMode);
    }

    void SetRegisterMode(bool register)
    {
        isRegisterMode = register;
        registerPanel.SetActive(register);
        loginButton.gameObject.SetActive(!register);
        registerButton.gameObject.SetActive(register);
        switchModeButton.GetComponentInChildren<TextMeshProUGUI>().text = register ? "로그인으로 돌아가기" : "회원가입";
        statusText.text = register ? "회원가입 정보를 입력하세요" : "로그인하세요";
    }

    void LoadMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}
