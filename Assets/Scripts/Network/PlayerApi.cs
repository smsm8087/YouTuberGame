using System;

/// <summary>
/// 플레이어 데이터 API
/// 내 정보 조회, 저장, 캐릭터 목록 등
/// </summary>
public static class PlayerApi
{
    public static void GetMyData(Action<bool, PlayerResponse> callback)
    {
        ApiClient.Instance.Get("/player/me", callback);
    }

    public static void SaveData(PlayerSaveRequest data, Action<bool, ApiResponse> callback)
    {
        ApiClient.Instance.Put<PlayerSaveRequest, ApiResponse>("/player/save", data, callback);
    }

    public static void GetOwnedCharacters(Action<bool, OwnedCharactersResponse> callback)
    {
        ApiClient.Instance.Get("/player/characters", callback);
    }

    public static void LevelUpCharacter(string instanceId, int expChips, Action<bool, LevelUpResponse> callback)
    {
        var request = new LevelUpRequest { exp_chips = expChips };
        ApiClient.Instance.Post<LevelUpRequest, LevelUpResponse>($"/player/characters/{instanceId}/levelup", request, callback);
    }

    public static void GetEquipment(Action<bool, ApiResponse<string>> callback)
    {
        ApiClient.Instance.Get("/player/equipment", callback);
    }

    public static void UpgradeEquipment(string equipmentType, Action<bool, EquipmentUpgradeResponse> callback)
    {
        ApiClient.Instance.Post($"/player/equipment/{equipmentType}/upgrade", callback);
    }

    public static void UpgradeStudio(Action<bool, ApiResponse> callback)
    {
        ApiClient.Instance.Post("/player/studio/upgrade", callback);
    }
}
