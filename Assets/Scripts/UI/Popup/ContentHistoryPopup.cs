using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 콘텐츠 히스토리 팝업 - 업로드된 콘텐츠 목록 및 통계
/// </summary>
public class ContentHistoryPopup : UI_Popup
{
    enum Buttons { CloseBtn }
    enum Texts { TotalContentsText, TotalViewsText, TotalRevenueText }
    enum GameObjects { HistoryListContent }

    protected override void FirstSetting()
    {
        base.FirstSetting();

        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        GetButton(Buttons.CloseBtn).AddButtonEvent(() => ClosePop());
    }

    public override void Init()
    {
        base.Init();
        LoadHistory();
        OpenPop();
    }

    void LoadHistory()
    {
        StartCoroutine(APIClient.Instance.GetContentHistory((ok, res) =>
        {
            if (!ok) return;

            var data = JsonUtility.FromJson<ContentListResponse>(res);
            var content = GetObject(GameObjects.HistoryListContent);
            foreach (Transform child in content.transform)
                Destroy(child.gameObject);

            long totalViews = 0, totalRevenue = 0;
            int totalCount = data.Contents != null ? data.Contents.Length : 0;

            if (data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    totalViews += item.Views;
                    totalRevenue += item.Revenue;
                    // TODO: ContentHistoryCard 프리팹 생성
                }
            }

            GetText(Texts.TotalContentsText).text = $"{totalCount}개";
            GetText(Texts.TotalViewsText).text = Util.FormatNumber(totalViews);
            GetText(Texts.TotalRevenueText).text = Util.FormatNumber(totalRevenue);
        }));
    }
}
