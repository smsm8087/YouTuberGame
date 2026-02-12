using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 캐릭터 목록 팝업 - 보유 캐릭터 조회, 레벨업, 돌파
/// </summary>
public class CharacterPopup : UI_Popup
{
    enum Buttons { CloseBtn, FilterAllBtn, FilterCBtn, FilterBBtn, FilterABtn, FilterSBtn }
    enum Texts { TitleText }
    enum GameObjects { CharacterListContent, DetailPanel }

    private string _currentFilter = "All";
    private CharacterResponse[] _characters;

    protected override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        GetButton(Buttons.CloseBtn).AddButtonEvent(() => ClosePop());
        GetButton(Buttons.FilterAllBtn).AddButtonEvent(() => SetFilter("All"));
        GetButton(Buttons.FilterCBtn).AddButtonEvent(() => SetFilter("C"));
        GetButton(Buttons.FilterBBtn).AddButtonEvent(() => SetFilter("B"));
        GetButton(Buttons.FilterABtn).AddButtonEvent(() => SetFilter("A"));
        GetButton(Buttons.FilterSBtn).AddButtonEvent(() => SetFilter("S"));

        GetObject(GameObjects.DetailPanel).SetActive(false);
    }

    public override void Init()
    {
        base.Init();
        LoadCharacters();
        OpenPop();

        GameObserver.On(ObserverEvent.CharacterUpdated, LoadCharacters);
    }

    public override void OnClose()
    {
        base.OnClose();
        GameObserver.Off(ObserverEvent.CharacterUpdated, LoadCharacters);
    }

    void LoadCharacters()
    {
        StartCoroutine(APIClient.Instance.GetAllCharacters((ok, res) =>
        {
            if (!ok) return;
            var data = JsonUtility.FromJson<CharactersResponse>(res);
            _characters = data.Characters;
            RefreshList();
        }));
    }

    void SetFilter(string filter)
    {
        _currentFilter = filter;
        RefreshList();
    }

    void RefreshList()
    {
        if (_characters == null) return;

        var content = GetObject(GameObjects.CharacterListContent);
        foreach (Transform child in content.transform)
            Destroy(child.gameObject);

        foreach (var ch in _characters)
        {
            // Rarity: 0=C, 1=B, 2=A, 3=S
            string rarity = ch.Rarity switch { 0 => "C", 1 => "B", 2 => "A", 3 => "S", _ => "C" };
            if (_currentFilter != "All" && rarity != _currentFilter)
                continue;

            // TODO: CharacterCard 프리팹 생성
        }
    }
}
