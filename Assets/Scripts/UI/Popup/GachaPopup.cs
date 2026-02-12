using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 가챠 팝업 - 캐릭터 뽑기
/// </summary>
public class GachaPopup : UI_Popup
{
    enum Buttons { Draw1Btn, Draw10Btn, CloseBtn }
    enum Texts { TicketCountText, GemCountText }
    enum GameObjects { ResultPanel, ResultContent }

    protected override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        GetButton(Buttons.Draw1Btn).AddButtonEvent(() => OnDraw(1));
        GetButton(Buttons.Draw10Btn).AddButtonEvent(() => OnDraw(10));
        GetButton(Buttons.CloseBtn).AddButtonEvent(() => ClosePop());

        GetObject(GameObjects.ResultPanel).SetActive(false);
    }

    public override void Init()
    {
        base.Init();
        RefreshPlayerInfo();
        OpenPop();
    }

    void RefreshPlayerInfo()
    {
        StartCoroutine(APIClient.Instance.GetPlayerData((ok, res) =>
        {
            if (!ok) return;
            var data = JsonUtility.FromJson<PlayerResponse>(res);
            GetText(Texts.TicketCountText).text = data.GachaTickets.ToString();
            GetText(Texts.GemCountText).text = data.Gem.ToString();
        }));
    }

    void OnDraw(int count)
    {
        if (_isTransition) return;
        SetTouchGuard(true);

        StartCoroutine(APIClient.Instance.DrawGacha(count, (ok, res) =>
        {
            SetTouchGuard(false);
            if (!ok) return;

            var data = JsonUtility.FromJson<GachaResponse>(res);
            ShowResults(data);
            GameObserver.Emit(ObserverEvent.GachaCompleted);
            GameObserver.Emit(ObserverEvent.CurrencyChanged);
        }));
    }

    void ShowResults(GachaResponse data)
    {
        var resultPanel = GetObject(GameObjects.ResultPanel);
        var resultContent = GetObject(GameObjects.ResultContent);
        resultPanel.SetActive(true);

        foreach (Transform child in resultContent.transform)
            Destroy(child.gameObject);

        // 재화 갱신
        GetText(Texts.TicketCountText).text = data.RemainingTickets.ToString();
        GetText(Texts.GemCountText).text = data.RemainingGem.ToString();

        // TODO: GachaResultCard 프리팹으로 결과 카드 생성
    }
}
