using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YouTuberGame.Network;
using System.Threading.Tasks;

namespace YouTuberGame.UI
{
    /// <summary>
    /// 로그인/회원가입 UI 관리
    /// </summary>
    public class LoginManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject registerPanel;

        [Header("Login Panel")]
        [SerializeField] private TMP_InputField loginEmail;
        [SerializeField] private TMP_InputField loginPassword;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button toRegisterButton;
        [SerializeField] private TextMeshProUGUI loginStatusText;

        [Header("Register Panel")]
        [SerializeField] private TMP_InputField registerEmail;
        [SerializeField] private TMP_InputField registerPassword;
        [SerializeField] private TMP_InputField registerPlayerName;
        [SerializeField] private TMP_InputField registerChannelName;
        [SerializeField] private Button registerButton;
        [SerializeField] private Button toLoginButton;
        [SerializeField] private TextMeshProUGUI registerStatusText;

        private void Start()
        {
            // 버튼 리스너 등록
            loginButton.onClick.AddListener(() => OnLoginButtonClicked());
            toRegisterButton.onClick.AddListener(() => ShowRegisterPanel());
            registerButton.onClick.AddListener(() => OnRegisterButtonClicked());
            toLoginButton.onClick.AddListener(() => ShowLoginPanel());

            // 초기 화면 설정
            ShowLoginPanel();

            // 저장된 토큰 확인
            APIClient.Instance.LoadAuthToken();
            if (APIClient.Instance.IsLoggedIn)
            {
                Debug.Log("[LoginManager] Already logged in, loading game...");
                LoadMainGame();
            }
        }

        private void ShowLoginPanel()
        {
            loginPanel.SetActive(true);
            registerPanel.SetActive(false);
            loginStatusText.text = "";
        }

        private void ShowRegisterPanel()
        {
            loginPanel.SetActive(false);
            registerPanel.SetActive(true);
            registerStatusText.text = "";
        }

        private async void OnLoginButtonClicked()
        {
            string email = loginEmail.text.Trim();
            string password = loginPassword.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                loginStatusText.text = "이메일과 비밀번호를 입력해주세요.";
                loginStatusText.color = Color.red;
                return;
            }

            loginButton.interactable = false;
            loginStatusText.text = "로그인 중...";
            loginStatusText.color = Color.white;

            var response = await APIClient.Instance.Login(email, password);

            if (response != null && response.Success)
            {
                loginStatusText.text = $"환영합니다, {response.PlayerName}님!";
                loginStatusText.color = Color.green;
                await Task.Delay(1000);
                LoadMainGame();
            }
            else
            {
                loginStatusText.text = response?.Message ?? "로그인 실패";
                loginStatusText.color = Color.red;
                loginButton.interactable = true;
            }
        }

        private async void OnRegisterButtonClicked()
        {
            string email = registerEmail.text.Trim();
            string password = registerPassword.text;
            string playerName = registerPlayerName.text.Trim();
            string channelName = registerChannelName.text.Trim();

            // 유효성 검사
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(channelName))
            {
                registerStatusText.text = "모든 항목을 입력해주세요.";
                registerStatusText.color = Color.red;
                return;
            }

            if (password.Length < 6)
            {
                registerStatusText.text = "비밀번호는 6자 이상이어야 합니다.";
                registerStatusText.color = Color.red;
                return;
            }

            registerButton.interactable = false;
            registerStatusText.text = "회원가입 중...";
            registerStatusText.color = Color.white;

            var response = await APIClient.Instance.Register(email, password, playerName, channelName);

            if (response != null && response.Success)
            {
                registerStatusText.text = "회원가입 성공! 로그인 중...";
                registerStatusText.color = Color.green;
                await Task.Delay(1000);
                LoadMainGame();
            }
            else
            {
                registerStatusText.text = response?.Message ?? "회원가입 실패";
                registerStatusText.color = Color.red;
                registerButton.interactable = true;
            }
        }

        private void LoadMainGame()
        {
            // TODO: 메인 게임 씬으로 이동
            Debug.Log("[LoginManager] Loading main game...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
        }
    }
}
