using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 콘텐츠 제작 팝업 UI
/// 장르 선택 → 팀원 배치 → 제작 시작
/// </summary>
public class ContentProductionUI : MonoBehaviour
{
    [Header("Genre Selection")]
    [SerializeField] private Transform _genreButtonContainer;
    [SerializeField] private Button _genreButtonPrefab;

    [Header("Team Selection")]
    [SerializeField] private Transform _teamSlotContainer;
    [SerializeField] private Transform _availableCharacterContainer;

    [Header("Production Info")]
    [SerializeField] private TextMeshProUGUI _selectedGenreText;
    [SerializeField] private TextMeshProUGUI _teamPowerText;
    [SerializeField] private TextMeshProUGUI _estimatedQualityText;
    [SerializeField] private TextMeshProUGUI _productionTimeText;

    [Header("Buttons")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _closeButton;

    private string _selectedGenre;
    private string[] _selectedTeamIds = new string[0];
    private Action _onProductionStarted;

    public void Show(Action onStarted)
    {
        _onProductionStarted = onStarted;
        _selectedGenre = null;
        _selectedTeamIds = new string[0];

        _startButton.onClick.RemoveAllListeners();
        _startButton.onClick.AddListener(OnStartClick);
        _closeButton.onClick.RemoveAllListeners();
        _closeButton.onClick.AddListener(() => gameObject.SetActive(false));

        _startButton.interactable = false;
        UpdateInfo();

        // TODO: 해금된 장르 목록으로 버튼 생성
        // TODO: 보유 캐릭터 목록으로 팀 선택 UI 생성
    }

    public void SelectGenre(string genre)
    {
        _selectedGenre = genre;
        _selectedGenreText.text = genre;
        UpdateStartButton();
        UpdateInfo();
    }

    public void SetTeam(string[] teamMemberIds)
    {
        _selectedTeamIds = teamMemberIds;
        UpdateStartButton();
        UpdateInfo();
    }

    void UpdateStartButton()
    {
        _startButton.interactable = !string.IsNullOrEmpty(_selectedGenre)
                                    && _selectedTeamIds != null
                                    && _selectedTeamIds.Length > 0;
    }

    void UpdateInfo()
    {
        if (string.IsNullOrEmpty(_selectedGenre))
        {
            _selectedGenreText.text = "장르를 선택하세요";
            _teamPowerText.text = "-";
            _estimatedQualityText.text = "-";
            _productionTimeText.text = "-";
            return;
        }

        _teamPowerText.text = _selectedTeamIds.Length > 0
            ? $"팀원 {_selectedTeamIds.Length}명 배치"
            : "팀원을 배치하세요";
    }

    void OnStartClick()
    {
        if (string.IsNullOrEmpty(_selectedGenre) || _selectedTeamIds.Length == 0) return;

        _startButton.interactable = false;

        ContentApi.StartProduction(_selectedGenre, _selectedTeamIds, (ok, res) =>
        {
            if (ok)
            {
                Debug.Log($"[ContentProduction] Started! ID: {res.content_id}");
                gameObject.SetActive(false);
                _onProductionStarted?.Invoke();
            }
            else
            {
                _startButton.interactable = true;
                Debug.LogError("[ContentProduction] Failed to start production");
            }
        });
    }
}
