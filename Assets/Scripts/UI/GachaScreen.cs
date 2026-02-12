using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YouTuberGame.Network;
using System.Threading.Tasks;

public class GachaScreen : MonoBehaviour
{
    [Header("재화 정보")]
    public TextMeshProUGUI ticketsText;
    public TextMeshProUGUI gemsText;
    
    [Header("가챠 버튼")]
    public Button draw1Button;
    public Button draw10Button;
    public Toggle useTicketToggle;
    public TextMeshProUGUI draw1CostText;
    public TextMeshProUGUI draw10CostText;
    
    [Header("결과 UI")]
    public GameObject resultPanel;
    public Transform resultContainer;
    public GameObject characterCardPrefab;
    public Button confirmButton;
    
    [Header("뒤로가기")]
    public Button backButton;
    
    private int currentTickets;
    private int currentGems;
    private bool useTicket = true;

    void Start()
    {
        // 버튼 이벤트
        draw1Button.onClick.AddListener(() => OnDraw(1));
        draw10Button.onClick.AddListener(() => OnDraw(10));
        useTicketToggle.onValueChanged.AddListener(OnToggleChanged);
        confirmButton.onClick.AddListener(OnConfirmResult);
        backButton.onClick.AddListener(OnBack);
        
        resultPanel.SetActive(false);
        UpdateCostDisplay();
        LoadPlayerCurrency();
    }

    async void LoadPlayerCurrency()
    {
        var data = await APIClient.Instance.GetPlayerData();
        if (data != null)
        {
            currentTickets = data.GachaTickets;
            currentGems = data.Gems;
            ticketsText.text = $"티켓: {currentTickets}";
            gemsText.text = $"젬: {currentGems}";
        }
    }

    void OnToggleChanged(bool value)
    {
        useTicket = value;
        UpdateCostDisplay();
    }

    void UpdateCostDisplay()
    {
        if (useTicket)
        {
            draw1CostText.text = "티켓 1개";
            draw10CostText.text = "티켓 10개";
        }
        else
        {
            draw1CostText.text = "젬 100개";
            draw10CostText.text = "젬 900개";
        }
    }

    async void OnDraw(int count)
    {
        // 재화 체크
        if (useTicket && currentTickets < count)
        {
            Debug.Log("티켓이 부족합니다");
            return;
        }
        
        int gemCost = count == 1 ? 100 : 900;
        if (!useTicket && currentGems < gemCost)
        {
            Debug.Log("젬이 부족합니다");
            return;
        }
        
        // 가챠 실행
        draw1Button.interactable = false;
        draw10Button.interactable = false;
        
        var response = await APIClient.Instance.DrawGacha(count, useTicket);
        
        draw1Button.interactable = true;
        draw10Button.interactable = true;
        
        if (response != null && response.Success)
        {
            ShowResults(response);
            currentTickets = response.RemainingTickets;
            currentGems = response.RemainingGems;
            ticketsText.text = $"티켓: {currentTickets}";
            gemsText.text = $"젬: {currentGems}";
        }
        else
        {
            Debug.LogError($"가챠 실패: {response?.Message}");
        }
    }

    void ShowResults(GachaResponse response)
    {
        // 기존 결과 삭제
        foreach (Transform child in resultContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 결과 카드 생성
        foreach (var result in response.Results)
        {
            GameObject card = Instantiate(characterCardPrefab, resultContainer);
            
            // 카드 UI 설정 (간단 버전)
            var nameText = card.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                string rarityStr = GetRarityString(result.Rarity);
                string newTag = result.IsNew ? " [NEW]" : "";
                nameText.text = $"[{rarityStr}] {result.CharacterName}{newTag}";
                
                // 등급별 색상
                nameText.color = GetRarityColor(result.Rarity);
            }
        }
        
        resultPanel.SetActive(true);
    }

    string GetRarityString(int rarity)
    {
        switch (rarity)
        {
            case 0: return "C";
            case 1: return "B";
            case 2: return "A";
            case 3: return "S";
            default: return "?";
        }
    }

    Color GetRarityColor(int rarity)
    {
        switch (rarity)
        {
            case 0: return Color.white;      // C
            case 1: return Color.cyan;       // B
            case 2: return Color.magenta;    // A
            case 3: return Color.yellow;     // S
            default: return Color.gray;
        }
    }

    void OnConfirmResult()
    {
        resultPanel.SetActive(false);
    }

    void OnBack()
    {
        gameObject.SetActive(false);
    }
}
