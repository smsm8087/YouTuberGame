using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 메인 씬 HUD - 상단 재화 바 + 하단 메뉴 버튼 + 콘텐츠 제작 패널
/// 씬에 직접 배치되는 UI (팝업이 아님)
/// </summary>
public class MainHUD : UI_Base
{
    enum Buttons
    {
        MakeContentBtn,
        GachaBtn,
        CharacterBtn,
        EquipmentBtn,
        RankingBtn,
        HistoryBtn,
        CompleteBtn,
        UploadBtn
    }

    enum Texts
    {
        PlayerNameText,
        GoldText,
        GemText,
        SubscribersText,
        TimerText
    }

    enum GameObjects { ProductionPanel }

    private string _currentContentId;
    private int _remainingSeconds;
    private bool _isProducing;

    void Awake()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        SetupButtons();
        SetupObservers();
        RefreshPlayerData();
        CheckProducing();
    }

    protected override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));
    }

    void SetupButtons()
    {
        GetButton(Buttons.MakeContentBtn).AddButtonEvent(OnMakeContent);
        GetButton(Buttons.GachaBtn).AddButtonEvent(() =>
            UIManager.Instance.ShowPopup<GachaPopup>());
        GetButton(Buttons.CharacterBtn).AddButtonEvent(() =>
            UIManager.Instance.ShowPopup<CharacterPopup>());
        GetButton(Buttons.EquipmentBtn).AddButtonEvent(() =>
            UIManager.Instance.ShowPopup<EquipmentPopup>());
        GetButton(Buttons.RankingBtn).AddButtonEvent(() =>
            UIManager.Instance.ShowPopup<RankingPopup>());
        GetButton(Buttons.HistoryBtn).AddButtonEvent(() =>
            UIManager.Instance.ShowPopup<ContentHistoryPopup>());
        GetButton(Buttons.CompleteBtn).AddButtonEvent(OnComplete);
        GetButton(Buttons.UploadBtn).AddButtonEvent(OnUpload);

        GetObject(GameObjects.ProductionPanel).SetActive(false);
        GetButton(Buttons.CompleteBtn).gameObject.SetActive(false);
        GetButton(Buttons.UploadBtn).gameObject.SetActive(false);
    }

    void SetupObservers()
    {
        this.Listen(ObserverEvent.CurrencyChanged, RefreshPlayerData);
        this.Listen(ObserverEvent.ContentCompleted, RefreshPlayerData);
        this.Listen(ObserverEvent.ContentUploaded, () =>
        {
            GetObject(GameObjects.ProductionPanel).SetActive(false);
            RefreshPlayerData();
        });
    }

    void OnDestroy()
    {
        this.Unlisten(ObserverEvent.CurrencyChanged, RefreshPlayerData);
        this.Unlisten(ObserverEvent.ContentCompleted, RefreshPlayerData);
    }

    void RefreshPlayerData()
    {
        StartCoroutine(APIClient.Instance.GetPlayerData((ok, res) =>
        {
            if (!ok) return;
            var data = JsonUtility.FromJson<PlayerResponse>(res);
            GetText(Texts.PlayerNameText).text = data.PlayerName;
            GetText(Texts.GoldText).text = Util.FormatNumber(data.Gold);
            GetText(Texts.GemText).text = data.Gem.ToString();
            GetText(Texts.SubscribersText).text = Util.FormatNumber(data.Subscribers);
        }));
    }

    void CheckProducing()
    {
        StartCoroutine(APIClient.Instance.GetProducingContent((ok, res) =>
        {
            if (!ok) return;
            var data = JsonUtility.FromJson<ContentListResponse>(res);
            if (data.Contents == null || data.Contents.Length == 0) return;

            var content = data.Contents[0];
            _currentContentId = content.ContentId;
            _remainingSeconds = content.RemainingSeconds;

            if (content.Status == "Producing")
            {
                _isProducing = true;
                GetObject(GameObjects.ProductionPanel).SetActive(true);
                InvokeRepeating(nameof(UpdateTimer), 0f, 1f);
            }
            else if (content.Status == "Completed")
            {
                ShowCompleteState();
            }
        }));
    }

    void OnMakeContent()
    {
        if (_isProducing) return;

        StartCoroutine(APIClient.Instance.StartContent("콘텐츠", "0", (ok, res) =>
        {
            if (!ok) return;
            var data = JsonUtility.FromJson<ContentResponse>(res);
            _currentContentId = data.ContentId;
            _remainingSeconds = data.RemainingSeconds;
            _isProducing = true;
            GetObject(GameObjects.ProductionPanel).SetActive(true);
            InvokeRepeating(nameof(UpdateTimer), 0f, 1f);
            GameObserver.Emit(ObserverEvent.ContentStarted);
        }));
    }

    void UpdateTimer()
    {
        if (_remainingSeconds > 0)
        {
            _remainingSeconds--;
            int h = _remainingSeconds / 3600;
            int m = (_remainingSeconds % 3600) / 60;
            int s = _remainingSeconds % 60;
            GetText(Texts.TimerText).text = $"{h:D2}:{m:D2}:{s:D2}";
        }
        else
        {
            CancelInvoke(nameof(UpdateTimer));
            ShowCompleteState();
        }
    }

    void ShowCompleteState()
    {
        _isProducing = false;
        GetObject(GameObjects.ProductionPanel).SetActive(true);
        GetText(Texts.TimerText).text = "제작 완료!";
        GetButton(Buttons.CompleteBtn).gameObject.SetActive(true);
        GetButton(Buttons.UploadBtn).gameObject.SetActive(false);
    }

    void OnComplete()
    {
        StartCoroutine(APIClient.Instance.CompleteContent(_currentContentId, (ok, res) =>
        {
            if (!ok) return;
            GetButton(Buttons.CompleteBtn).gameObject.SetActive(false);
            GetButton(Buttons.UploadBtn).gameObject.SetActive(true);
            GameObserver.Emit(ObserverEvent.ContentCompleted);
        }));
    }

    void OnUpload()
    {
        StartCoroutine(APIClient.Instance.UploadContent(_currentContentId, (ok, res) =>
        {
            if (!ok) return;
            GetButton(Buttons.UploadBtn).gameObject.SetActive(false);
            GetObject(GameObjects.ProductionPanel).SetActive(false);
            GameObserver.Emit(ObserverEvent.ContentUploaded);
            RefreshPlayerData();
        }));
    }
}
