using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 랭킹 팝업 - 주간/채널파워 탭
/// </summary>
public class RankingPopup : UI_Popup
{
    enum Buttons { CloseBtn, WeeklyTabBtn, PowerTabBtn }
    enum Texts { MyRankText, MyScoreText }
    enum GameObjects { RankingListContent }

    private bool _isWeeklyTab = true;

    protected override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        GetButton(Buttons.CloseBtn).AddButtonEvent(() => ClosePop());
        GetButton(Buttons.WeeklyTabBtn).AddButtonEvent(() => SwitchTab(true));
        GetButton(Buttons.PowerTabBtn).AddButtonEvent(() => SwitchTab(false));
    }

    public override void Init()
    {
        base.Init();
        LoadRanking();
        OpenPop();
    }

    void SwitchTab(bool weekly)
    {
        _isWeeklyTab = weekly;
        LoadRanking();
    }

    void LoadRanking()
    {
        if (_isWeeklyTab)
        {
            StartCoroutine(APIClient.Instance.GetWeeklyRanking((ok, res) =>
            {
                if (ok) DisplayRanking(res);
            }));
        }
        else
        {
            StartCoroutine(APIClient.Instance.GetChannelPowerRanking((ok, res) =>
            {
                if (ok) DisplayRanking(res);
            }));
        }
    }

    void DisplayRanking(string res)
    {
        var data = JsonUtility.FromJson<RankingResponse>(res);

        if (data.MyRanking != null)
        {
            GetText(Texts.MyRankText).text = $"내 순위: {data.MyRanking.Rank}위";
            GetText(Texts.MyScoreText).text = Util.FormatNumber(data.MyRanking.Value);
        }

        var content = GetObject(GameObjects.RankingListContent);
        foreach (Transform child in content.transform)
            Destroy(child.gameObject);

        // TODO: RankingEntryItem 프리팹 생성
    }
}
