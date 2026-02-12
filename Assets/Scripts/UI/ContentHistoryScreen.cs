using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 콘텐츠 히스토리 화면
/// 업로드한 콘텐츠 목록 (제목, 장르, 조회수, 수익)
/// </summary>
public class ContentHistoryScreen : MonoBehaviour
{
    [Header("콘텐츠 리스트")]
    public Transform contentContainer;
    public GameObject contentEntryPrefab;

    [Header("정렬 옵션")]
    public Button sortByDateButton;
    public Button sortByViewsButton;
    public Button sortByRevenueButton;

    [Header("통계")]
    public TextMeshProUGUI totalContentsText;
    public TextMeshProUGUI totalViewsText;
    public TextMeshProUGUI totalRevenueText;

    [Header("공통")]
    public TextMeshProUGUI statusText;
    public Button refreshButton;
    public Button backButton;

    private ContentResponse[] allContents;
    private enum SortType { Date, Views, Revenue }
    private SortType currentSort = SortType.Date;

    void Start()
    {
        sortByDateButton.onClick.AddListener(() => SortBy(SortType.Date));
        sortByViewsButton.onClick.AddListener(() => SortBy(SortType.Views));
        sortByRevenueButton.onClick.AddListener(() => SortBy(SortType.Revenue));
        refreshButton.onClick.AddListener(LoadHistory);
        backButton.onClick.AddListener(OnBack);

        LoadHistory();
    }

    void LoadHistory()
    {
        SetStatus("히스토리 로딩 중...");

        StartCoroutine(APIClient.Instance.GetContentHistory((success, response) =>
        {
            if (success)
            {
                var data = JsonUtility.FromJson<ContentListResponse>(response);
                allContents = data.Contents;
                SortBy(currentSort);
                UpdateStatistics();
                SetStatus($"{allContents.Length}개 콘텐츠");
            }
            else
            {
                SetStatus("히스토리를 불러올 수 없습니다.");
            }
        }));
    }

    void SortBy(SortType sortType)
    {
        currentSort = sortType;

        // 정렬 버튼 상태
        sortByDateButton.interactable = (sortType != SortType.Date);
        sortByViewsButton.interactable = (sortType != SortType.Views);
        sortByRevenueButton.interactable = (sortType != SortType.Revenue);

        if (allContents == null || allContents.Length == 0) return;

        // 정렬
        System.Array.Sort(allContents, (a, b) =>
        {
            switch (sortType)
            {
                case SortType.Views:
                    return b.Views.CompareTo(a.Views);
                case SortType.Revenue:
                    return b.Revenue.CompareTo(a.Revenue);
                case SortType.Date:
                default:
                    return 0; // 서버에서 이미 날짜순
            }
        });

        DisplayContents();
    }

    void DisplayContents()
    {
        // 기존 항목 삭제
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        if (allContents == null || allContents.Length == 0)
        {
            SetStatus("업로드한 콘텐츠가 없습니다.");
            return;
        }

        // 콘텐츠 항목 생성
        foreach (var content in allContents)
        {
            GameObject entry = Instantiate(contentEntryPrefab, contentContainer);

            var texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 4)
            {
                texts[0].text = content.Title;
                texts[1].text = GetGenreKorean(content.Genre);
                texts[2].text = $"{FormatNumber(content.Views)} 조회";
                texts[3].text = $"{FormatNumber(content.Revenue)} 골드";
            }

            // 품질에 따른 색상
            var image = entry.GetComponent<Image>();
            if (image != null)
            {
                image.color = GetQualityColor(content.TotalQuality);
            }
        }
    }

    void UpdateStatistics()
    {
        if (allContents == null || allContents.Length == 0)
        {
            totalContentsText.text = "총 콘텐츠: 0";
            totalViewsText.text = "총 조회수: 0";
            totalRevenueText.text = "총 수익: 0";
            return;
        }

        long totalViews = 0;
        long totalRevenue = 0;

        foreach (var content in allContents)
        {
            totalViews += content.Views;
            totalRevenue += content.Revenue;
        }

        totalContentsText.text = $"총 콘텐츠: {allContents.Length}";
        totalViewsText.text = $"총 조회수: {FormatNumber(totalViews)}";
        totalRevenueText.text = $"총 수익: {FormatNumber(totalRevenue)} 골드";
    }

    string GetGenreKorean(string genre)
    {
        switch (genre.ToLower())
        {
            case "vlog": return "브이로그";
            case "gaming": return "게임";
            case "mukbang": return "먹방";
            case "education": return "교육";
            case "shorts": return "쇼츠";
            case "documentary": return "다큐";
            case "review": return "리뷰";
            case "entertainment": return "엔터테인먼트";
            default: return genre;
        }
    }

    string FormatNumber(long value)
    {
        if (value >= 1000000)
            return $"{value / 1000000.0:F1}M";
        else if (value >= 1000)
            return $"{value / 1000.0:F1}K";
        else
            return value.ToString();
    }

    Color GetQualityColor(int quality)
    {
        // 품질에 따른 그라데이션 (회색 → 초록 → 파랑 → 보라 → 금색)
        if (quality >= 500)
            return new Color(1f, 0.84f, 0f, 0.3f); // 금색
        else if (quality >= 400)
            return new Color(0.7f, 0.3f, 1f, 0.3f); // 보라
        else if (quality >= 300)
            return new Color(0.3f, 0.7f, 1f, 0.3f); // 파랑
        else if (quality >= 200)
            return new Color(0.3f, 1f, 0.3f, 0.3f); // 초록
        else
            return new Color(0.7f, 0.7f, 0.7f, 0.3f); // 회색
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
