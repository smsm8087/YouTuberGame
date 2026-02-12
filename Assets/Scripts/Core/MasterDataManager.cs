using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 마스터 데이터 매니저
/// 서버에서 밸런스 테이블을 받아 로컬 캐싱
/// 앱 시작 시 버전 체크 → 변경 시만 다운로드
///
/// 사용법:
///   int cost = MasterDataManager.Instance.Data.Equipment.CostPerLevel * level;
///   int maxLv = MasterDataManager.Instance.GetMaxLevel("A"); // 50
/// </summary>
public class MasterDataManager : MonoBehaviour
{
    public static MasterDataManager Instance { get; private set; }

    public MasterData Data { get; private set; }
    public bool IsLoaded { get; private set; }

    private const string CACHE_KEY = "master_data_cache";
    private const string VERSION_KEY = "master_data_version";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCache();
    }

    /// <summary>
    /// 서버에서 마스터 데이터 로드 (버전 체크 → 필요시 다운로드)
    /// </summary>
    public IEnumerator LoadFromServer(Action<bool> callback = null)
    {
        // 1. 버전 체크
        yield return APIClient.Instance.GetMasterDataVersion((ok, res) =>
        {
            if (!ok)
            {
                Debug.LogWarning("[MasterData] 버전 체크 실패, 캐시 사용");
                callback?.Invoke(Data != null);
                return;
            }

            var versionRes = JsonUtility.FromJson<MasterDataVersionResponse>(res);
            int cachedVersion = PlayerPrefs.GetInt(VERSION_KEY, 0);

            if (cachedVersion >= versionRes.Version && Data != null)
            {
                Debug.Log($"[MasterData] 캐시 최신 (v{cachedVersion})");
                IsLoaded = true;
                callback?.Invoke(true);
            }
            else
            {
                StartCoroutine(DownloadMasterData(versionRes.Version, callback));
            }
        });
    }

    private IEnumerator DownloadMasterData(int version, Action<bool> callback)
    {
        yield return APIClient.Instance.GetMasterData((ok, res) =>
        {
            if (!ok)
            {
                Debug.LogError("[MasterData] 다운로드 실패");
                callback?.Invoke(Data != null);
                return;
            }

            Data = JsonUtility.FromJson<MasterData>(res);
            IsLoaded = true;

            // 캐싱
            PlayerPrefs.SetString(CACHE_KEY, res);
            PlayerPrefs.SetInt(VERSION_KEY, version);
            PlayerPrefs.Save();

            Debug.Log($"[MasterData] v{version} 다운로드 완료");
            callback?.Invoke(true);
        });
    }

    private void LoadCache()
    {
        string cached = PlayerPrefs.GetString(CACHE_KEY, "");
        if (!string.IsNullOrEmpty(cached))
        {
            Data = JsonUtility.FromJson<MasterData>(cached);
            IsLoaded = true;
            Debug.Log("[MasterData] 캐시 로드 완료");
        }
    }

    // ───── 편의 메서드 ─────

    public int GetMaxLevel(string rarity)
    {
        if (Data?.Character?.MaxLevelByRarity == null) return 30;
        foreach (var entry in Data.Character.MaxLevelByRarity)
        {
            if (entry.Key == rarity) return entry.Value;
        }
        return 30;
    }

    public int GetProductionTime(string genre)
    {
        if (Data?.Content?.ProductionTimeByGenre == null) return 300;
        foreach (var entry in Data.Content.ProductionTimeByGenre)
        {
            if (entry.Key == genre) return entry.Value;
        }
        return 300;
    }

    public int GetGachaRarityProb(string rarity)
    {
        if (Data?.Gacha?.RarityProbability == null) return 0;
        foreach (var entry in Data.Gacha.RarityProbability)
        {
            if (entry.Key == rarity) return entry.Value;
        }
        return 0;
    }

    public int GetEquipmentUpgradeCost(int currentLevel)
    {
        if (Data?.Equipment == null) return currentLevel * 500;
        return currentLevel * Data.Equipment.CostPerLevel;
    }

    public bool IsEquipmentMaxLevel(int level)
    {
        if (Data?.Equipment == null) return level >= 10;
        return level >= Data.Equipment.MaxLevel;
    }
}

// ───── 클라이언트용 데이터 클래스 (JsonUtility 호환) ─────

[Serializable]
public class MasterDataVersionResponse
{
    public int Version;
}

[Serializable]
public class MasterData
{
    public int Version;
    public GachaBalance Gacha;
    public CharacterBalance Character;
    public ContentBalance Content;
    public EquipmentBalance Equipment;
    public PlayerStart PlayerStart;
    public MilestoneEntry[] Milestones;
}

[Serializable]
public class GachaBalance
{
    public int GemCostPerDraw;
    public int MaxDrawCount;
    public DictEntry[] RarityProbability;
}

[Serializable]
public class CharacterBalance
{
    public DictEntry[] MaxLevelByRarity;
    public int BreakthroughLevelBonus;
    public int ExpPerChip;
    public float LevelStatMultiplier;
}

[Serializable]
public class ContentBalance
{
    public DictEntry[] ProductionTimeByGenre;
    public int ViewsPerQualityMin;
    public int ViewsPerQualityMax;
    public int LikesPercentMin;
    public int LikesPercentMax;
    public int ViewsPerGold;
    public int SubscriberPerViewsMin;
    public int SubscriberPerViewsMax;
}

[Serializable]
public class EquipmentBalance
{
    public int MaxLevel;
    public int CostPerLevel;
    public int BonusPerLevel;
}

[Serializable]
public class PlayerStart
{
    public long StartGold;
    public int StartGems;
    public int StartTickets;
    public int StartExpChips;
}

[Serializable]
public class MilestoneEntry
{
    public long RequiredSubscribers;
    public string UnlockType;
    public string UnlockValue;
    public string Description;
}

/// <summary>
/// JsonUtility는 Dictionary를 지원하지 않으므로
/// Key-Value 배열로 대체
/// </summary>
[Serializable]
public class DictEntry
{
    public string Key;
    public int Value;
}
