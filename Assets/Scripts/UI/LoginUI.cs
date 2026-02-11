using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 로그인/회원가입 UI
/// 앱 시작 시 표시, 자동 로그인 지원
/// </summary>
public class LoginUI : MonoBehaviour
{
    [Header("Login Panel")]
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private TMP_InputField _loginEmailInput;
    [SerializeField] private TMP_InputField _loginPasswordInput;
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _goRegisterButton;

    [Header("Register Panel")]
    [SerializeField] private GameObject _registerPanel;
    [SerializeField] private TMP_InputField _registerEmailInput;
    [SerializeField] private TMP_InputField _registerPasswordInput;
    [SerializeField] private TMP_InputField _registerNameInput;
    [SerializeField] private TMP_InputField _registerChannelInput;
    [SerializeField] private Button _registerButton;
    [SerializeField] private Button _goLoginButton;

    [Header("Common")]
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private GameObject _loadingIndicator;

    private void Start()
    {
        _loginButton.onClick.AddListener(OnLoginClick);
        _goRegisterButton.onClick.AddListener(ShowRegisterPanel);
        _registerButton.onClick.AddListener(OnRegisterClick);
        _goLoginButton.onClick.AddListener(ShowLoginPanel);

        ShowLoginPanel();
        SetLoading(false);

        TryAutoLogin();
    }

    void TryAutoLogin()
    {
        ApiClient.Instance.LoadSavedAuth();
        if (ApiClient.Instance.IsLoggedIn)
        {
            SetLoading(true);
            SetStatus("자동 로그인 중...");
            PlayerApi.GetMyData((ok, res) =>
            {
                SetLoading(false);
                if (ok)
                {
                    OnLoginSuccess(res);
                }
                else
                {
                    ApiClient.Instance.ClearAuth();
                    SetStatus("세션 만료. 다시 로그인해주세요.");
                }
            });
        }
    }

    void ShowLoginPanel()
    {
        _loginPanel.SetActive(true);
        _registerPanel.SetActive(false);
        SetStatus("");
    }

    void ShowRegisterPanel()
    {
        _loginPanel.SetActive(false);
        _registerPanel.SetActive(true);
        SetStatus("");
    }

    void OnLoginClick()
    {
        string email = _loginEmailInput.text.Trim();
        string password = _loginPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetStatus("이메일과 비밀번호를 입력해주세요.");
            return;
        }

        SetLoading(true);
        SetStatus("로그인 중...");

        AuthApi.Login(email, password, (ok, res) =>
        {
            SetLoading(false);
            if (ok)
            {
                SetStatus("로그인 성공!");
                LoadPlayerAndEnter();
            }
            else
            {
                SetStatus("로그인 실패. 이메일/비밀번호를 확인해주세요.");
            }
        });
    }

    void OnRegisterClick()
    {
        string email = _registerEmailInput.text.Trim();
        string password = _registerPasswordInput.text;
        string playerName = _registerNameInput.text.Trim();
        string channelName = _registerChannelInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)
            || string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(channelName))
        {
            SetStatus("모든 항목을 입력해주세요.");
            return;
        }

        if (password.Length < 6)
        {
            SetStatus("비밀번호는 6자 이상이어야 합니다.");
            return;
        }

        SetLoading(true);
        SetStatus("회원가입 중...");

        AuthApi.Register(email, password, playerName, channelName, (ok, res) =>
        {
            SetLoading(false);
            if (ok)
            {
                SetStatus("회원가입 성공!");
                LoadPlayerAndEnter();
            }
            else
            {
                SetStatus("회원가입 실패. 이미 사용 중인 이메일일 수 있습니다.");
            }
        });
    }

    void LoadPlayerAndEnter()
    {
        SetLoading(true);
        PlayerApi.GetMyData((ok, res) =>
        {
            SetLoading(false);
            if (ok)
            {
                OnLoginSuccess(res);
            }
            else
            {
                SetStatus("플레이어 데이터를 불러올 수 없습니다.");
            }
        });
    }

    void OnLoginSuccess(PlayerResponse playerData)
    {
        Debug.Log($"[LoginUI] Welcome {playerData.player_name}! Channel: {playerData.channel_name}");
        // TODO: SceneManager.LoadScene("Main") 또는 UI 전환
        gameObject.SetActive(false);
    }

    void SetStatus(string message)
    {
        if (_statusText != null)
            _statusText.text = message;
    }

    void SetLoading(bool isLoading)
    {
        if (_loadingIndicator != null)
            _loadingIndicator.SetActive(isLoading);

        _loginButton.interactable = !isLoading;
        _registerButton.interactable = !isLoading;
    }
}
