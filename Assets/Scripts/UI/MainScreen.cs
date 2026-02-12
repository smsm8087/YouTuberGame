using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YouTuberGame.Network;
using System.Threading.Tasks;

public class MainScreen : MonoBehaviour
{
    [Header("플레이어 정보")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI gemText;
    public TextMeshProUGUI subscribersText;
    
    [Header("버튼")]
    public Button makeContentButton;
    public Button gachaButton;
    public Button charactersButton;
    public Button equipmentButton;
    
    [Header("콘텐츠 제작 UI")]
    public GameObject productionPanel;
    public TextMeshProUGUI timerText;
    public Button completeButton;
    public Button uploadButton;
    
    private string currentContentId;
    private int remainingSeconds;
    private bool isProducing = false;

    async void Start()
    {
        // 플레이어 데이터 로드
        await LoadPlayerData();
        
        // 버튼 이벤트
        makeContentButton.onClick.AddListener(() => OnMakeContent());
        gachaButton.onClick.AddListener(() => Debug.Log("가챠 (TODO)"));
        charactersButton.onClick.AddListener(() => Debug.Log("팀원 (TODO)"));
        equipmentButton.onClick.AddListener(() => Debug.Log("장비 (TODO)"));
        
        completeButton.onClick.AddListener(() => OnComplete());
        uploadButton.onClick.AddListener(() => OnUpload());
        
        // 제작 중인 콘텐츠 확인
        await CheckProducing();
        
        productionPanel.SetActive(false);
    }

    async Task LoadPlayerData()
    {
        var data = await APIClient.Instance.GetPlayerData();
        if (data != null)
        {
            playerNameText.text = data.PlayerName;
            goldText.text = FormatNum(data.Gold);
            gemText.text = data.Gems.ToString();
            subscribersText.text = FormatNum(data.Subscribers);
        }
    }

    async Task CheckProducing()
    {
        var contents = await APIClient.Instance.GetProducingContent();
        if (contents != null && contents.Length > 0)
        {
            var content = contents[0];
            currentContentId = content.ContentId;
            remainingSeconds = content.RemainingSeconds;
            
            if (content.Status == 0) // Producing
            {
                isProducing = true;
                productionPanel.SetActive(true);
                StartTimer();
            }
            else if (content.Status == 1) // Completed
            {
                ShowComplete();
            }
        }
    }

    async void OnMakeContent()
    {
        if (isProducing)
        {
            Debug.Log("이미 제작 중");
            return;
        }
        
        // 간단한 테스트: 빈 캐릭터로 콘텐츠 시작
        var response = await APIClient.Instance.StartContent("테스트 콘텐츠", 0, new string[0]);
        if (response != null && response.Success)
        {
            currentContentId = response.Content.ContentId;
            remainingSeconds = response.Content.RemainingSeconds;
            isProducing = true;
            productionPanel.SetActive(true);
            StartTimer();
        }
    }

    void StartTimer()
    {
        InvokeRepeating(nameof(UpdateTimer), 0f, 1f);
    }

    void UpdateTimer()
    {
        if (remainingSeconds > 0)
        {
            remainingSeconds--;
            int h = remainingSeconds / 3600;
            int m = (remainingSeconds % 3600) / 60;
            int s = remainingSeconds % 60;
            timerText.text = $"{h:D2}:{m:D2}:{s:D2}";
        }
        else
        {
            CancelInvoke(nameof(UpdateTimer));
            ShowComplete();
        }
    }

    void ShowComplete()
    {
        isProducing = false;
        timerText.text = "제작 완료!";
        completeButton.gameObject.SetActive(true);
        uploadButton.gameObject.SetActive(false);
    }

    async void OnComplete()
    {
        var response = await APIClient.Instance.CompleteContent(currentContentId);
        if (response != null && response.Success)
        {
            completeButton.gameObject.SetActive(false);
            uploadButton.gameObject.SetActive(true);
        }
    }

    async void OnUpload()
    {
        var response = await APIClient.Instance.UploadContent(currentContentId);
        if (response != null && response.Success)
        {
            Debug.Log($"조회수: {FormatNum(response.Views)}, 수익: {FormatNum(response.Revenue)}");
            productionPanel.SetActive(false);
            await LoadPlayerData();
        }
    }

    string FormatNum(long num)
    {
        if (num >= 1000000) return (num / 1000000f).ToString("F1") + "M";
        if (num >= 1000) return (num / 1000f).ToString("F1") + "K";
        return num.ToString();
    }
}
