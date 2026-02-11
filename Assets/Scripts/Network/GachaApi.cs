using System;

/// <summary>
/// 가챠(캐릭터 뽑기) API
/// </summary>
public static class GachaApi
{
    public static void Draw(int drawCount, bool useTicket, Action<bool, GachaDrawResponse> callback)
    {
        var request = new GachaDrawRequest
        {
            draw_count = drawCount,
            use_ticket = useTicket
        };
        ApiClient.Instance.Post<GachaDrawRequest, GachaDrawResponse>("/gacha/draw", request, callback);
    }
}
