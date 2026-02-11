using System;

/// <summary>
/// 트렌드 & 랭킹 API
/// </summary>
public static class TrendApi
{
    public static void GetTodayTrend(Action<bool, TrendResponse> callback)
    {
        ApiClient.Instance.Get("/trend/today", callback);
    }

    public static void GetWeeklyRanking(int limit, Action<bool, RankingResponse> callback)
    {
        ApiClient.Instance.Get($"/rankings/weekly?limit={limit}", callback);
    }

    public static void GetChannelPowerRanking(int limit, Action<bool, RankingResponse> callback)
    {
        ApiClient.Instance.Get($"/rankings/channel-power?limit={limit}", callback);
    }
}
