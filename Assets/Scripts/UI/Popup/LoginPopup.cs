using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 로그인/회원가입 팝업
/// </summary>
public class LoginPopup : UI_Popup
{
    enum Buttons { LoginBtn, RegisterBtn, SwitchModeBtn }
    enum Texts { StatusText, SwitchModeText }
    enum GameObjects { RegisterPanel }
    enum InputFields { EmailInput, PasswordInput, PlayerNameInput, ChannelNameInput }

    private bool _isRegisterMode = false;

    protected override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));
        Bind<TMP_InputField>(typeof(InputFields));

        GetButton(Buttons.LoginBtn).AddButtonEvent(OnLogin);
        GetButton(Buttons.RegisterBtn).AddButtonEvent(OnRegister);
        GetButton(Buttons.SwitchModeBtn).AddButtonEvent(OnSwitchMode);
    }

    public override void Init()
    {
        base.Init();
        SetRegisterMode(false);

        // 자동 로그인 체크
        if (APIClient.Instance.IsLoggedIn)
        {
            GetText(Texts.StatusText).text = "자동 로그인 중...";
            StartCoroutine(APIClient.Instance.GetPlayerData((ok, res) =>
            {
                if (ok)
                    LoadMainScene();
                else
                {
                    GetText(Texts.StatusText).text = "토큰 만료. 다시 로그인하세요.";
                    APIClient.Instance.ClearToken();
                }
            }));
            return;
        }

        OpenPop();
    }

    void SetRegisterMode(bool register)
    {
        _isRegisterMode = register;
        GetObject(GameObjects.RegisterPanel).SetActive(register);
        GetButton(Buttons.LoginBtn).gameObject.SetActive(!register);
        GetButton(Buttons.RegisterBtn).gameObject.SetActive(register);
        GetText(Texts.SwitchModeText).text = register ? "로그인으로 돌아가기" : "회원가입";
        GetText(Texts.StatusText).text = register ? "회원가입 정보를 입력하세요" : "로그인하세요";
    }

    void OnSwitchMode()
    {
        SetRegisterMode(!_isRegisterMode);
    }

    void OnLogin()
    {
        string email = GetInputField(InputFields.EmailInput).text.Trim();
        string password = GetInputField(InputFields.PasswordInput).text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            GetText(Texts.StatusText).text = "이메일과 비밀번호를 입력하세요.";
            return;
        }

        SetTouchGuard(true);
        GetText(Texts.StatusText).text = "로그인 중...";

        StartCoroutine(APIClient.Instance.Login(email, password, (ok, res) =>
        {
            SetTouchGuard(false);
            if (ok)
            {
                GetText(Texts.StatusText).text = "환영합니다!";
                Invoke(nameof(LoadMainScene), 0.5f);
            }
            else
            {
                GetText(Texts.StatusText).text = $"로그인 실패: {res}";
            }
        }));
    }

    void OnRegister()
    {
        string email = GetInputField(InputFields.EmailInput).text.Trim();
        string password = GetInputField(InputFields.PasswordInput).text;
        string playerName = GetInputField(InputFields.PlayerNameInput).text.Trim();
        string channelName = GetInputField(InputFields.ChannelNameInput).text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(channelName))
        {
            GetText(Texts.StatusText).text = "모든 항목을 입력하세요.";
            return;
        }

        if (password.Length < 6)
        {
            GetText(Texts.StatusText).text = "비밀번호는 6자 이상이어야 합니다.";
            return;
        }

        SetTouchGuard(true);
        GetText(Texts.StatusText).text = "회원가입 중...";

        StartCoroutine(APIClient.Instance.Register(email, password, playerName, channelName, (ok, res) =>
        {
            SetTouchGuard(false);
            if (ok)
            {
                GetText(Texts.StatusText).text = "가입 완료!";
                Invoke(nameof(LoadMainScene), 0.5f);
            }
            else
            {
                GetText(Texts.StatusText).text = $"회원가입 실패: {res}";
            }
        }));
    }

    void LoadMainScene()
    {
        CloseImmediate();
        SceneManager.LoadScene("MainScene");
    }
}
