using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class APIClient : MonoBehaviour
{
    // Inspector에서 설정 가능 (빌드별로 변경)
    [SerializeField] private string apiUrl = "https://localhost:5001/api";

    private string _authToken = null;

    public static APIClient Instance { get; private set; }

    /// <summary>
    /// 토큰 만료 시 발생하는 이벤트
    /// </summary>
    public static event Action OnTokenExpired;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadToken();
    }

    /// <summary>
    /// 런타임에 API URL 변경 (서버 이사 시)
    /// </summary>
    public void SetApiUrl(string url)
    {
        apiUrl = url.TrimEnd('/');
    }

    private void LoadToken()
    {
        _authToken = PlayerPrefs.GetString("auth_token", null);
    }

    private void SaveToken(string token)
    {
        _authToken = token;
        PlayerPrefs.SetString("auth_token", token);
        PlayerPrefs.Save();
    }

    public void ClearToken()
    {
        _authToken = null;
        PlayerPrefs.DeleteKey("auth_token");
    }

    public bool IsLoggedIn => !string.IsNullOrEmpty(_authToken);

    // 회원가입
    public IEnumerator Register(string email, string password, string playerName, string channelName, Action<bool, string> callback)
    {
        var data = new { Email = email, Password = password, PlayerName = playerName, ChannelName = channelName };
        yield return Post("/auth/register", data, false, callback);
    }

    // 로그인
    public IEnumerator Login(string email, string password, Action<bool, string> callback)
    {
        var data = new { Email = email, Password = password };
        yield return Post("/auth/login", data, false, (success, response) =>
        {
            if (success)
            {
                var auth = JsonUtility.FromJson<AuthResponse>(response);
                SaveToken(auth.Token);
            }
            callback?.Invoke(success, response);
        });
    }

    // 플레이어 데이터 조회
    public IEnumerator GetPlayerData(Action<bool, string> callback)
    {
        yield return Get("/player/me", callback);
    }

    // 가챠
    public IEnumerator DrawGacha(int count, Action<bool, string> callback)
    {
        var data = new { Count = count };
        yield return Post("/gacha/draw", data, true, callback);
    }

    // 캐릭터 목록 조회
    public IEnumerator GetAllCharacters(Action<bool, string> callback)
    {
        yield return Get("/characters", callback);
    }

    // 캐릭터 레벨업
    public IEnumerator LevelUpCharacter(string playerCharacterId, int expChipsToUse, Action<bool, string> callback)
    {
        var data = new { ExpChipsToUse = expChipsToUse };
        yield return Post($"/player/characters/{playerCharacterId}/levelup", data, true, callback);
    }

    // 장비 조회
    public IEnumerator GetEquipment(Action<bool, string> callback)
    {
        yield return Get("/player/equipment", callback);
    }

    // 장비 업그레이드
    public IEnumerator UpgradeEquipment(string equipmentType, Action<bool, string> callback)
    {
        yield return Post($"/player/equipment/{equipmentType}/upgrade", null, true, callback);
    }

    // 콘텐츠 제작 시작
    public IEnumerator StartContent(string title, string genre, Action<bool, string> callback)
    {
        var data = new { Title = title, Genre = genre };
        yield return Post("/content/start", data, true, callback);
    }

    // 제작 중 콘텐츠 조회
    public IEnumerator GetProducingContent(Action<bool, string> callback)
    {
        yield return Get("/content/producing", callback);
    }

    // 콘텐츠 제작 완료
    public IEnumerator CompleteContent(string contentId, Action<bool, string> callback)
    {
        yield return Post($"/content/{contentId}/complete", null, true, callback);
    }

    // 콘텐츠 업로드
    public IEnumerator UploadContent(string contentId, Action<bool, string> callback)
    {
        yield return Post($"/content/{contentId}/upload", null, true, callback);
    }

    // 콘텐츠 히스토리
    public IEnumerator GetContentHistory(Action<bool, string> callback)
    {
        yield return Get("/content/history", callback);
    }

    // 주간 랭킹
    public IEnumerator GetWeeklyRanking(Action<bool, string> callback)
    {
        yield return Get("/rankings/weekly", callback);
    }

    // 채널 파워 랭킹
    public IEnumerator GetChannelPowerRanking(Action<bool, string> callback)
    {
        yield return Get("/rankings/channel-power", callback);
    }

    // 마스터 데이터 버전 확인
    public IEnumerator GetMasterDataVersion(Action<bool, string> callback)
    {
        yield return Get("/master-data/version", callback);
    }

    // 마스터 데이터 전체 조회
    public IEnumerator GetMasterData(Action<bool, string> callback)
    {
        yield return Get("/master-data", callback);
    }

    // ───── HTTP 헬퍼 ─────

    private IEnumerator Get(string endpoint, Action<bool, string> callback)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(apiUrl + endpoint))
        {
            req.timeout = 10;

            if (!string.IsNullOrEmpty(_authToken))
                req.SetRequestHeader("Authorization", "Bearer " + _authToken);

            yield return req.SendWebRequest();
            HandleResponse(req, endpoint, callback);
        }
    }

    private IEnumerator Post(string endpoint, object data, bool auth, Action<bool, string> callback)
    {
        string json = data != null ? JsonUtility.ToJson(data) : "{}";
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(apiUrl + endpoint, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 10;

            if (auth && !string.IsNullOrEmpty(_authToken))
                req.SetRequestHeader("Authorization", "Bearer " + _authToken);

            yield return req.SendWebRequest();
            HandleResponse(req, endpoint, callback);
        }
    }

    private void HandleResponse(UnityWebRequest req, string endpoint, Action<bool, string> callback)
    {
        if (req.result == UnityWebRequest.Result.Success)
        {
            callback?.Invoke(true, req.downloadHandler.text);
            return;
        }

        long statusCode = req.responseCode;
        string errorMsg;

        switch (statusCode)
        {
            case 401:
                // 토큰 만료 → 로그인 화면으로
                Debug.LogWarning($"[API] 401 Unauthorized: {endpoint}");
                errorMsg = "로그인이 만료되었습니다.";
                ClearToken();
                OnTokenExpired?.Invoke();
                break;
            case 400:
                // 잘못된 요청 → 서버 에러 메시지 표시
                errorMsg = req.downloadHandler?.text ?? "요청이 잘못되었습니다.";
                Debug.LogWarning($"[API] 400 Bad Request: {endpoint} - {errorMsg}");
                break;
            case 0:
                // 네트워크 연결 실패
                errorMsg = "서버에 연결할 수 없습니다.\n네트워크를 확인해주세요.";
                Debug.LogError($"[API] Network error: {endpoint} - {req.error}");
                break;
            default:
                errorMsg = $"서버 오류가 발생했습니다. ({statusCode})";
                Debug.LogError($"[API] {statusCode} {endpoint}: {req.error}");
                break;
        }

        callback?.Invoke(false, errorMsg);
    }
}

// ───── 응답 DTO ─────

[Serializable]
public class AuthResponse
{
    public bool Success;
    public string Message;
    public string Token;
    public string UserId;
}

[Serializable]
public class PlayerResponse
{
    public string PlayerName;
    public string ChannelName;
    public long Gold;
    public long Gem;
    public long Subscribers;
    public long ExpChips;
    public long GachaTickets;
}

[Serializable]
public class CharacterResponse
{
    public string CharacterId;
    public string PlayerCharacterId;
    public string CharacterName;
    public int Rarity;
    public int Level;
    public int Experience;
    public int Breakthrough;
    public int Filming;
    public int Editing;
    public int Planning;
    public int Design;
    public int BaseFilming;
    public int BaseEditing;
    public int BasePlanning;
    public int BaseDesign;
    public string PassiveSkillDesc;
    public bool IsNew;
}

[Serializable]
public class CharactersResponse
{
    public CharacterResponse[] Characters;
}

[Serializable]
public class GachaResponse
{
    public CharacterResponse[] Characters;
    public long RemainingGold;
    public long RemainingGem;
    public long RemainingTickets;
}

[Serializable]
public class EquipmentResponseData
{
    public string Type;
    public int Level;
    public int Bonus;
}

[Serializable]
public class EquipmentListResponse
{
    public EquipmentResponseData[] Equipment;
    public long Gold;
}

[Serializable]
public class ContentResponse
{
    public string ContentId;
    public string Title;
    public string Genre;
    public string Status;
    public int ProductionSeconds;
    public int RemainingSeconds;
    public int TotalQuality;
    public long Views;
    public long Revenue;
}

[Serializable]
public class ContentListResponse
{
    public ContentResponse[] Contents;
}

[Serializable]
public class RankingEntry
{
    public int Rank;
    public string PlayerName;
    public string ChannelName;
    public long Value;
    public bool IsMe;
}

[Serializable]
public class RankingResponse
{
    public RankingEntry[] Rankings;
    public RankingEntry MyRanking;
}
