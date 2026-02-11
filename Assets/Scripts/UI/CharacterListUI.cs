using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 캐릭터 목록 UI - 보유 캐릭터 표시, 필터링, 레벨업/돌파
/// </summary>
public class CharacterListUI : MonoBehaviour
{
    [Header("Character List")]
    [SerializeField] private Transform _characterContainer;
    [SerializeField] private GameObject _characterCardPrefab;
    [SerializeField] private Button _refreshButton;

    [Header("Filter Buttons")]
    [SerializeField] private Button _filterAllButton;
    [SerializeField] private Button _filterCButton;
    [SerializeField] private Button _filterBButton;
    [SerializeField] private Button _filterAButton;
    [SerializeField] private Button _filterSButton;

    [Header("Detail Panel")]
    [SerializeField] private GameObject _detailPanel;
    [SerializeField] private TextMeshProUGUI _detailNameText;
    [SerializeField] private TextMeshProUGUI _detailRarityText;
    [SerializeField] private TextMeshProUGUI _detailLevelText;
    [SerializeField] private TextMeshProUGUI _detailExpText;
    [SerializeField] private TextMeshProUGUI _detailStatsText;
    [SerializeField] private TextMeshProUGUI _detailSkillText;
    [SerializeField] private Button _levelUpButton;
    [SerializeField] private Button _breakthroughButton;
    [SerializeField] private Button _closeDetailButton;

    [Header("Currency Display")]
    [SerializeField] private TextMeshProUGUI _expChipsText;

    [Header("Common")]
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private GameObject _loadingIndicator;

    private List<CharacterResponse> _allCharacters = new List<CharacterResponse>();
    private CharacterResponse _selectedCharacter;
    private int _currentFilter = -1;
    private long _currentExpChips = 0;

    void Start()
    {
        _refreshButton.onClick.AddListener(LoadCharacters);
        _filterAllButton.onClick.AddListener(() => SetFilter(-1));
        _filterCButton.onClick.AddListener(() => SetFilter(0));
        _filterBButton.onClick.AddListener(() => SetFilter(1));
        _filterAButton.onClick.AddListener(() => SetFilter(2));
        _filterSButton.onClick.AddListener(() => SetFilter(3));
        _levelUpButton.onClick.AddListener(OnLevelUpClick);
        _breakthroughButton.onClick.AddListener(OnBreakthroughClick);
        _closeDetailButton.onClick.AddListener(CloseDetailPanel);

        _detailPanel.SetActive(false);
        SetLoading(false);

        LoadCharacters();
        LoadPlayerCurrency();
    }

    void LoadCharacters()
    {
        SetLoading(true);
        SetStatus("캐릭터 목록 불러오는 중...");

        CharacterApi.GetAll((ok, characters) =>
        {
            SetLoading(false);
            if (ok && characters != null)
            {
                _allCharacters = characters.ToList();
                DisplayCharacters();
                SetStatus($"{_allCharacters.Count}개 캐릭터 보유");
            }
            else
            {
                SetStatus("캐릭터 목록을 불러올 수 없습니다.");
            }
        });
    }

    void LoadPlayerCurrency()
    {
        PlayerApi.GetMyData((ok, res) =>
        {
            if (ok)
            {
                _currentExpChips = res.exp_chips;
                UpdateCurrencyDisplay();
            }
        });
    }

    void UpdateCurrencyDisplay()
    {
        if (_expChipsText != null)
            _expChipsText.text = $"경험치 칩: {_currentExpChips:#,0}";
    }

    void SetFilter(int rarity)
    {
        _currentFilter = rarity;
        DisplayCharacters();
    }

    void DisplayCharacters()
    {
        foreach (Transform child in _characterContainer)
        {
            Destroy(child.gameObject);
        }

        if (_allCharacters == null || _allCharacters.Count == 0)
        {
            SetStatus("보유 캐릭터가 없습니다.");
            return;
        }

        var filtered = _currentFilter == -1
            ? _allCharacters
            : _allCharacters.Where(c => c.rarity == _currentFilter).ToList();

        if (filtered.Count == 0)
        {
            SetStatus($"{GetRarityString(_currentFilter)} 등급 캐릭터가 없습니다.");
            return;
        }

        foreach (var character in filtered)
        {
            GameObject card = Instantiate(_characterCardPrefab, _characterContainer);

            var texts = card.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 3)
            {
                texts[0].text = character.character_name;
                texts[1].text = $"[{GetRarityString(character.rarity)}] Lv.{character.level}";
                texts[2].text = $"촬영:{character.filming} 편집:{character.editing} 기획:{character.planning} 디자인:{character.design}";
            }

            var image = card.GetComponent<Image>();
            if (image != null)
            {
                image.color = GetRarityColor(character.rarity);
            }

            var button = card.GetComponent<Button>();
            if (button != null)
            {
                var charData = character;
                button.onClick.AddListener(() => ShowDetail(charData));
            }
        }

        SetStatus($"{filtered.Count}개 캐릭터 표시 중");
    }

    void ShowDetail(CharacterResponse character)
    {
        _selectedCharacter = character;

        _detailNameText.text = character.character_name;
        _detailRarityText.text = $"등급: {GetRarityString(character.rarity)}";
        _detailLevelText.text = $"Lv. {character.level} / {GetMaxLevel(character.rarity, character.breakthrough)}";

        int currentExp = character.experience;
        int nextLevelExp = GetRequiredExp(character.level);
        _detailExpText.text = $"경험치: {currentExp} / {nextLevelExp}";

        _detailStatsText.text = $"촬영력: {character.filming}\n편집력: {character.editing}\n기획력: {character.planning}\n디자인력: {character.design}";

        _detailSkillText.text = string.IsNullOrEmpty(character.passive_skill_desc)
            ? "패시브 스킬 없음"
            : $"[패시브] {character.passive_skill_desc}";

        _detailPanel.SetActive(true);
    }

    void CloseDetailPanel()
    {
        _detailPanel.SetActive(false);
        _selectedCharacter = null;
    }

    void OnLevelUpClick()
    {
        if (_selectedCharacter == null) return;

        int maxLevel = GetMaxLevel(_selectedCharacter.rarity, _selectedCharacter.breakthrough);
        if (_selectedCharacter.level >= maxLevel)
        {
            SetStatus("최대 레벨입니다.");
            return;
        }

        int requiredExp = GetRequiredExp(_selectedCharacter.level);
        int currentExp = _selectedCharacter.experience;
        int neededExp = requiredExp - currentExp;
        int neededChips = Mathf.CeilToInt(neededExp / 100f);

        if (_currentExpChips < neededChips)
        {
            SetStatus($"경험치 칩이 부족합니다. (필요: {neededChips}개)");
            return;
        }

        SetLoading(true);
        SetStatus("레벨업 중...");

        CharacterApi.LevelUp(_selectedCharacter.player_character_id, neededChips, (ok, updatedChar) =>
        {
            SetLoading(false);
            if (ok && updatedChar != null)
            {
                SetStatus("레벨업 성공!");
                _currentExpChips -= neededChips;
                UpdateCurrencyDisplay();

                var index = _allCharacters.FindIndex(c => c.player_character_id == updatedChar.player_character_id);
                if (index >= 0)
                {
                    _allCharacters[index] = updatedChar;
                }

                ShowDetail(updatedChar);
                DisplayCharacters();
            }
            else
            {
                SetStatus("레벨업 실패.");
            }
        });
    }

    void OnBreakthroughClick()
    {
        if (_selectedCharacter == null) return;

        SetStatus("돌파 기능은 아직 구현되지 않았습니다.");
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
            case 0: return new Color(0.8f, 0.8f, 0.8f, 0.5f);
            case 1: return new Color(0.3f, 0.7f, 1f, 0.5f);
            case 2: return new Color(0.7f, 0.3f, 1f, 0.5f);
            case 3: return new Color(1f, 0.9f, 0.3f, 0.5f);
            default: return Color.gray;
        }
    }

    int GetMaxLevel(int rarity, int breakthrough)
    {
        int baseMax = rarity switch
        {
            0 => 30,
            1 => 40,
            2 => 50,
            3 => 60,
            _ => 30
        };
        return baseMax + (breakthrough * 10);
    }

    int GetRequiredExp(int level)
    {
        return level * 100;
    }

    void SetStatus(string message)
    {
        if (_statusText != null)
            _statusText.text = message;
    }

    void SetLoading(bool isLoading)
    {
        if (_loadingIndicator != null)
            _loadingIndicator.SetActive(isLoading);

        _refreshButton.interactable = !isLoading;
        _levelUpButton.interactable = !isLoading;
        _breakthroughButton.interactable = !isLoading;
    }
}
