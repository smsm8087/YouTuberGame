using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 앱 진입점 - 초기화 및 씬 전환 관리
/// 1. 저장된 토큰 확인 → 자동 로그인 시도
/// 2. 성공 시 Main 씬, 실패 시 Login 씬
/// </summary>
public class AppEntry : MonoBehaviour
{
    [SerializeField] private string _loginSceneName = "Login";
    [SerializeField] private string _mainSceneName = "Main";

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    void Initialize()
    {
        ApiClient.Instance.LoadSavedAuth();

        if (ApiClient.Instance.IsLoggedIn)
        {
            PlayerApi.GetMyData((ok, res) =>
            {
                if (ok)
                {
                    Debug.Log($"[AppEntry] Auto-login success: {res.player_name}");
                    GoToMain();
                }
                else
                {
                    Debug.Log("[AppEntry] Token expired, go to login");
                    ApiClient.Instance.ClearAuth();
                    GoToLogin();
                }
            });
        }
        else
        {
            GoToLogin();
        }
    }

    public void GoToLogin()
    {
        SceneManager.LoadScene(_loginSceneName);
    }

    public void GoToMain()
    {
        SceneManager.LoadScene(_mainSceneName);
    }
}
