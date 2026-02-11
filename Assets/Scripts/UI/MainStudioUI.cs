using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 메인 스튜디오 뷰 UI
/// 게임의 핵심 화면: 스튜디오 상태, 팀원, 콘텐츠 제작, 하단 메뉴
/// </summary>
public class MainStudioUI : MonoBehaviour
{
    [Header("Top Bar - Channel Info")]
    [SerializeField] private TextMeshProUGUI _channelNameText;
    [SerializeField] private TextMeshProUGUI _subscriberText;
    [SerializeField] private TextMeshProUGUI _channelPowerText;

    [Header("Currency Display")]
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _gemsText;

    [Header("Studio View")]
    [SerializeField] private TextMeshProUGUI _studioLevelText;
    [SerializeField] private Image _studioBackground;

    [Header("Content Production")]
    [SerializeField] private GameObject _productionPanel;
    [SerializeField] private TextMeshProUGUI _productionStatusText;
    [SerializeField] private Slider _productionProgressBar;
    [SerializeField] private Button _startProductionButton;
    [SerializeField] private Button _uploadButton;

    [Header("Daily Trend")]
    [SerializeField] private TextMeshProUGUI _trendGenreText;
    [SerializeField] private TextMeshProUGUI _trendBonusText;

    [Header("Bottom Navigation")]
    [SerializeField] private Button _studioButton;
    [SerializeField] private Button _teamButton;
    [SerializeField] private Button _equipmentButton;
    [SerializeField] private Button _gachaButton;
    [SerializeField] private Button _rankingButton;

    [Header("Sub Panels")]
    [SerializeField] private GameObject _teamPanel;
    [SerializeField] private GameObject _equipmentPanel;
    [SerializeField] private GameObject _gachaPanel;
    [SerializeField] private GameObject _rankingPanel;
    [SerializeField] private ContentProductionUI _contentProductionUI;

    private PlayerResponse _playerData;
    private ProducingContent _currentProduction;

    private void Start()
    {
        _studioButton.onClick.AddListener(() => ShowPanel(null));
        _teamButton.onClick.AddListener(() => ShowPanel(_teamPanel));
        _equipmentButton.onClick.AddListener(() => ShowPanel(_equipmentPanel));
        _gachaButton.onClick.AddListener(() => ShowPanel(_gachaPanel));
        _rankingButton.onClick.AddListener(() => ShowPanel(_rankingPanel));

        _startProductionButton.onClick.AddListener(OnStartProduction);
        _uploadButton.onClick.AddListener(OnUpload);

        RefreshAll();
    }

    public void RefreshAll()
    {
        LoadPlayerData();
        LoadProducingContent();
        LoadDailyTrend();
    }

    void LoadPlayerData()
    {
        PlayerApi.GetMyData((ok, res) =>
        {
            if (!ok) return;
            _playerData = res;
            UpdatePlayerUI();
        });
    }

    void UpdatePlayerUI()
    {
        if (_playerData == null) return;

        _channelNameText.text = _playerData.channel_name;
        _subscriberText.text = FormatNumber(_playerData.subscribers);
        _channelPowerText.text = $"CP {FormatNumber(_playerData.channel_power)}";
        _goldText.text = FormatNumber(_playerData.gold);
        _gemsText.text = _playerData.gems.ToString();
        _studioLevelText.text = $"Lv.{_playerData.studio_level}";
    }

    void LoadProducingContent()
    {
        ContentApi.GetProducing((ok, res) =>
        {
            if (!ok || res.producing_contents == null || res.producing_contents.Length == 0)
            {
                _currentProduction = null;
                UpdateProductionUI();
                return;
            }

            _currentProduction = res.producing_contents[0];
            UpdateProductionUI();
        });
    }

    void UpdateProductionUI()
    {
        if (_currentProduction == null)
        {
            _productionStatusText.text = "대기 중";
            _productionProgressBar.value = 0f;
            _startProductionButton.gameObject.SetActive(true);
            _uploadButton.gameObject.SetActive(false);
            return;
        }

        if (_currentProduction.is_complete)
        {
            _productionStatusText.text = $"{_currentProduction.genre} - 완성!";
            _productionProgressBar.value = 1f;
            _startProductionButton.gameObject.SetActive(false);
            _uploadButton.gameObject.SetActive(true);
        }
        else
        {
            _productionStatusText.text = $"{_currentProduction.genre} 제작 중...";
            _startProductionButton.gameObject.SetActive(false);
            _uploadButton.gameObject.SetActive(false);
            // TODO: 남은 시간 기반 프로그레스 계산
        }
    }

    void LoadDailyTrend()
    {
        TrendApi.GetTodayTrend((ok, res) =>
        {
            if (!ok) return;
            _trendGenreText.text = $"트렌드: {res.trend_genre}";
            _trendBonusText.text = $"+{res.bonus_percentage}%";
        });
    }

    void OnStartProduction()
    {
        if (_contentProductionUI != null)
        {
            _contentProductionUI.gameObject.SetActive(true);
            _contentProductionUI.Show(OnProductionStarted);
        }
    }

    void OnProductionStarted()
    {
        LoadProducingContent();
    }

    void OnUpload()
    {
        if (_currentProduction == null) return;

        _uploadButton.interactable = false;
        ContentApi.Upload(_currentProduction.content_id, (ok, res) =>
        {
            _uploadButton.interactable = true;
            if (ok)
            {
                Debug.Log($"[MainStudio] Upload success! Views: {res.views}, Revenue: {res.revenue}, +{res.subscribers_gained} subs");
                _currentProduction = null;
                RefreshAll();
                // TODO: 업로드 결과 팝업 표시
            }
        });
    }

    void ShowPanel(GameObject panel)
    {
        _teamPanel?.SetActive(panel == _teamPanel);
        _equipmentPanel?.SetActive(panel == _equipmentPanel);
        _gachaPanel?.SetActive(panel == _gachaPanel);
        _rankingPanel?.SetActive(panel == _rankingPanel);
    }

    string FormatNumber(long number)
    {
        if (number >= 1_000_000) return $"{number / 1_000_000f:F1}M";
        if (number >= 1_000) return $"{number / 1_000f:F1}K";
        return number.ToString();
    }
}
