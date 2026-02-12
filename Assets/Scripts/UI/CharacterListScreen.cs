using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YouTuberGame.Network;
using System.Threading.Tasks;
using System.Linq;

public class CharacterListScreen : MonoBehaviour
{
    [Header("캐릭터 목록")]
    public Transform characterContainer;
    public GameObject characterCardPrefab;
    
    [Header("필터")]
    public TMP_Dropdown rarityFilterDropdown;
    public Button filterAllButton;
    public Button filterCButton;
    public Button filterBButton;
    public Button filterAButton;
    public Button filterSButton;
    
    [Header("상세 팝업")]
    public GameObject detailPopup;
    public TextMeshProUGUI detailNameText;
    public TextMeshProUGUI detailRarityText;
    public TextMeshProUGUI detailLevelText;
    public TextMeshProUGUI detailExpText;
    public TextMeshProUGUI detailStatsText;
    public Button levelUpButton;
    public Button breakthroughButton;
    public Button closeDetailButton;
    
    [Header("재화")]
    public TextMeshProUGUI expChipsText;
    
    [Header("뒤로가기")]
    public Button backButton;
    
    private CharacterResponse[] allCharacters;
    private CharacterResponse selectedCharacter;
    private int currentFilter = -1; // -1=All, 0=C, 1=B, 2=A, 3=S

    void Start()
    {
        // 버튼 이벤트
        filterAllButton.onClick.AddListener(() => SetFilter(-1));
        filterCButton.onClick.AddListener(() => SetFilter(0));
        filterBButton.onClick.AddListener(() => SetFilter(1));
        filterAButton.onClick.AddListener(() => SetFilter(2));
        filterSButton.onClick.AddListener(() => SetFilter(3));
        
        levelUpButton.onClick.AddListener(OnLevelUp);
        breakthroughButton.onClick.AddListener(OnBreakthrough);
        closeDetailButton.onClick.AddListener(() => detailPopup.SetActive(false));
        backButton.onClick.AddListener(OnBack);
        
        detailPopup.SetActive(false);
        LoadCharacters();
        LoadPlayerCurrency();
    }

    async void LoadCharacters()
    {
        allCharacters = await APIClient.Instance.GetAllCharacters();
        if (allCharacters != null)
        {
            DisplayCharacters();
        }
    }

    async void LoadPlayerCurrency()
    {
        var data = await APIClient.Instance.GetPlayerData();
        if (data != null)
        {
            expChipsText.text = $"경험치 칩: {data.ExpChips}";
        }
    }

    void SetFilter(int rarity)
    {
        currentFilter = rarity;
        DisplayCharacters();
    }

    void DisplayCharacters()
    {
        // 기존 카드 삭제
        foreach (Transform child in characterContainer)
        {
            Destroy(child.gameObject);
        }
        
        if (allCharacters == null) return;
        
        // 필터링
        var filtered = currentFilter == -1 
            ? allCharacters 
            : allCharacters.Where(c => c.Rarity == currentFilter).ToArray();
        
        // 카드 생성
        foreach (var character in filtered)
        {
            GameObject card = Instantiate(characterCardPrefab, characterContainer);
            
            // 카드 UI 설정
            var texts = card.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 3)
            {
                texts[0].text = character.CharacterName;
                texts[1].text = $"[{GetRarityString(character.Rarity)}] Lv.{character.BaseFilming}"; // 임시로 BaseFilming 사용
                texts[2].text = $"F:{character.BaseFilming} E:{character.BaseEditing} P:{character.BasePlanning} D:{character.BaseDesign}";
            }
            
            // 클릭 이벤트
            var button = card.GetComponent<Button>();
            if (button != null)
            {
                var charData = character; // 클로저 캡처
                button.onClick.AddListener(() => ShowDetail(charData));
            }
            
            // 등급별 색상
            var image = card.GetComponent<Image>();
            if (image != null)
            {
                image.color = GetRarityColor(character.Rarity);
            }
        }
    }

    void ShowDetail(CharacterResponse character)
    {
        selectedCharacter = character;
        
        detailNameText.text = character.CharacterName;
        detailRarityText.text = GetRarityString(character.Rarity);
        detailLevelText.text = $"Lv. {character.BaseFilming}"; // 임시
        detailExpText.text = $"EXP: 0 / 100"; // 임시
        detailStatsText.text = $"촬영: {character.BaseFilming}\n편집: {character.BaseEditing}\n기획: {character.BasePlanning}\n디자인: {character.BaseDesign}";
        
        if (!string.IsNullOrEmpty(character.PassiveSkillDesc))
        {
            detailStatsText.text += $"\n\n[패시브] {character.PassiveSkillDesc}";
        }
        
        detailPopup.SetActive(true);
    }

    async void OnLevelUp()
    {
        if (selectedCharacter == null) return;
        
        // TODO: 실제 instanceId 필요 (현재는 CharacterId만 있음)
        // var response = await APIClient.Instance.LevelUpCharacter(selectedCharacter.CharacterId, 1);
        
        Debug.Log("레벨업 (TODO: PlayerCharacter instanceId 필요)");
    }

    async void OnBreakthrough()
    {
        if (selectedCharacter == null) return;
        
        // TODO: 중복 캐릭터 선택 UI 필요
        Debug.Log("돌파 (TODO: 중복 캐릭터 선택 UI 필요)");
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
            case 0: return new Color(0.8f, 0.8f, 0.8f, 0.3f);  // C - 회색
            case 1: return new Color(0f, 1f, 1f, 0.3f);        // B - 청록
            case 2: return new Color(1f, 0f, 1f, 0.3f);        // A - 마젠타
            case 3: return new Color(1f, 1f, 0f, 0.3f);        // S - 노랑
            default: return Color.gray;
        }
    }

    void OnBack()
    {
        gameObject.SetActive(false);
    }
}
