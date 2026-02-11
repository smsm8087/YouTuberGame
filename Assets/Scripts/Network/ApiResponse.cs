using System;

/// <summary>
/// 서버 공통 응답 래퍼
/// 모든 API 응답은 이 형태로 반환됨
/// </summary>
[Serializable]
public class ApiResponse<T>
{
    public bool success;
    public T data;
    public int error_code;
    public string message;
}

[Serializable]
public class ApiResponse
{
    public bool success;
    public int error_code;
    public string message;
}

// ===== Auth =====

[Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}

[Serializable]
public class RegisterRequest
{
    public string email;
    public string password;
    public string player_name;
    public string channel_name;
}

[Serializable]
public class AuthResponse
{
    public bool success;
    public string user_id;
    public string token;
}

// ===== Player =====

[Serializable]
public class PlayerResponse
{
    public string user_id;
    public string player_name;
    public string channel_name;
    public long subscribers;
    public long total_views;
    public long channel_power;
    public long gold;
    public int gems;
    public int gacha_tickets;
    public int exp_chips;
    public int studio_level;
    public string[] unlocked_genres;
}

[Serializable]
public class PlayerSaveRequest
{
    public long gold;
    public int gems;
    public long subscribers;
    public int studio_level;
}

// ===== Gacha =====

[Serializable]
public class GachaDrawRequest
{
    public int draw_count;
    public bool use_ticket;
}

[Serializable]
public class GachaDrawResponse
{
    public bool success;
    public DrawnCharacter[] drawn_characters;
    public int remaining_tickets;
}

[Serializable]
public class DrawnCharacter
{
    public string instance_id;
    public string character_id;
    public string character_name;
    public string rarity;
}

// ===== Content =====

[Serializable]
public class ContentStartRequest
{
    public string genre;
    public string[] team_member_ids;
}

[Serializable]
public class ContentStartResponse
{
    public bool success;
    public string content_id;
    public string estimated_completion;
}

[Serializable]
public class ProducingContentResponse
{
    public ProducingContent[] producing_contents;
}

[Serializable]
public class ProducingContent
{
    public string content_id;
    public string genre;
    public string quality;
    public int total_stats;
    public string created_at;
    public string estimated_completion;
    public bool is_complete;
}

[Serializable]
public class ContentUploadResponse
{
    public bool success;
    public long views;
    public long revenue;
    public int subscribers_gained;
}

// ===== Character =====

[Serializable]
public class OwnedCharactersResponse
{
    public OwnedCharacter[] owned_characters;
}

[Serializable]
public class OwnedCharacter
{
    public string instance_id;
    public string character_id;
    public int level;
    public int experience;
    public int breakthrough;
    public string acquired_at;
}

[Serializable]
public class LevelUpRequest
{
    public int exp_chips;
}

[Serializable]
public class LevelUpResponse
{
    public bool success;
    public int new_level;
    public int remaining_exp;
}

// ===== Equipment =====

[Serializable]
public class EquipmentUpgradeResponse
{
    public bool success;
    public int new_level;
    public string new_name;
    public long remaining_gold;
}

// ===== Trend =====

[Serializable]
public class TrendResponse
{
    public string trend_genre;
    public int bonus_percentage;
    public string next_change;
}

// ===== Ranking =====

[Serializable]
public class RankingResponse
{
    public RankingEntry[] rankings;
    public int my_rank;
    public long my_weekly_views;
    public long my_channel_power;
}

[Serializable]
public class RankingEntry
{
    public int rank;
    public string player_name;
    public string channel_name;
    public long weekly_views;
    public long channel_power;
}
