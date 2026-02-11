using System;

/// <summary>
/// 콘텐츠 제작 API
/// 제작 시작, 진행 조회, 업로드
/// </summary>
public static class ContentApi
{
    public static void StartProduction(string genre, string[] teamMemberIds, Action<bool, ContentStartResponse> callback)
    {
        var request = new ContentStartRequest
        {
            genre = genre,
            team_member_ids = teamMemberIds
        };
        ApiClient.Instance.Post<ContentStartRequest, ContentStartResponse>("/content/start", request, callback);
    }

    public static void GetProducing(Action<bool, ProducingContentResponse> callback)
    {
        ApiClient.Instance.Get("/content/producing", callback);
    }

    public static void Upload(string contentId, Action<bool, ContentUploadResponse> callback)
    {
        ApiClient.Instance.Post($"/content/{contentId}/upload", callback);
    }
}
