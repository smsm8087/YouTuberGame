using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 랭킹 화면
/// 주간 랭킹(구독자 증가), 채널 파워 랭킹 표시
/// </summary>
public class RankingScreen : MonoBehaviour
{
    [Header("탭 버튼")]
    public Button weeklyTabButton;
    public Button channelPowerTabButton;

    [Header("랭킹 리스트")]
    public Transform rankingContainer;
    public GameObject rankingEntryPrefab;

    [Header("내 순위")]
    public GameObject myRankingPanel;
    public TextMeshProUGUI myRankText;
    public TextMeshProUGUI myNameText;
    public TextMeshProUGUI myValueText;

    [Header("공통")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI statusText;
    public Button refreshButton;
    public Button backButton;

    private enum RankingType { Weekly, ChannelPower }
    private RankingType currentType = RankingType.Weekly;

    void Start()
    {
        weeklyTabButton.onClick.AddListener(() => SwitchTab(RankingType.Weekly));
        channelPowerTabButton.onClick.AddListener(() => SwitchTab(RankingType.ChannelPower));
        refreshButton.onClick.AddListener(LoadRanking);
        backButton.onClick.AddListener(OnBack);

        SwitchTab(RankingType.Weekly);
    }

    void SwitchTab(RankingType type)
    {
        currentType = type;

        // 탭 버튼 상태 업데이트
        weeklyTabButton.interactable = (type != RankingType.Weekly);
        channelPowerTabButton.interactable = (type != RankingType.ChannelPower);

        LoadRanking();
    }

    void LoadRanking()
    {
        SetStatus("랭킹 로딩 중...");

        if (currentType == RankingType.Weekly)
        {
            titleText.text = "주간 랭킹 (구독자 증가)";
            StartCoroutine(APIClient.Instance.GetWeeklyRanking(OnRankingLoaded));
        }
        else
        {
            titleText.text = "채널 파워 랭킹";
            StartCoroutine(APIClient.Instance.GetChannelPowerRanking(OnRankingLoaded));
        }
    }

    void OnRankingLoaded(bool success, string response)
    {
        if (!success)
        {
            SetStatus("랭킹을 불러올 수 없습니다.");
            return;
        }

        var data = JsonUtility.FromJson<RankingResponse>(response);
        DisplayRanking(data);
        SetStatus($"Top {data.Rankings.Length} 랭킹");
    }

    void DisplayRanking(RankingResponse data)
    {
        // 기존 항목 삭제
        foreach (Transform child in rankingContainer)
        {
            Destroy(child.gameObject);
        }

        // 랭킹 항목 생성
        foreach (var entry in data.Rankings)
        {
            GameObject item = Instantiate(rankingEntryPrefab, rankingContainer);

            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 3)
            {
                texts[0].text = $"#{entry.Rank}";
                texts[1].text = $"{entry.PlayerName}\n{entry.ChannelName}";
                texts[2].text = FormatValue(entry.Value);
            }

            // 내 랭킹이면 강조
            if (entry.IsMe)
            {
                var image = item.GetComponent<Image>();
                if (image != null)
                {
                    image.color = new Color(1f, 1f, 0.5f, 0.3f); // 노란색 강조
                }
            }
        }

        // 내 순위 표시
        if (data.MyRanking != null)
        {
            myRankingPanel.SetActive(true);
            myRankText.text = $"내 순위: #{data.MyRanking.Rank}";
            myNameText.text = $"{data.MyRanking.PlayerName} ({data.MyRanking.ChannelName})";
            myValueText.text = FormatValue(data.MyRanking.Value);
        }
        else
        {
            myRankingPanel.SetActive(false);
        }
    }

    string FormatValue(long value)
    {
        if (value >= 1000000)
            return $"{value / 1000000.0:F1}M";
        else if (value >= 1000)
            return $"{value / 1000.0:F1}K";
        else
            return value.ToString();
    }

    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    void OnBack()
    {
        gameObject.SetActive(false);
    }
}
