using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace YouTuberGame.Network
{
    /// <summary>
    /// 서버 REST API 통신 클라이언트
    /// </summary>
    public class APIClient : MonoBehaviour
    {
        private static APIClient instance;
        public static APIClient Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("APIClient");
                    instance = go.AddComponent<APIClient>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Server Settings")]
        [SerializeField] private string baseUrl = "https://localhost:7047/api";  // HTTPS 포트
        [SerializeField] private bool useLocalhost = true;

        private string authToken;
        public bool IsLoggedIn => !string.IsNullOrEmpty(authToken);

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void SetAuthToken(string token)
        {
            authToken = token;
            PlayerPrefs.SetString("auth_token", token);
            PlayerPrefs.Save();
            Debug.Log("[APIClient] Auth token saved");
        }

        public void LoadAuthToken()
        {
            authToken = PlayerPrefs.GetString("auth_token", "");
            if (!string.IsNullOrEmpty(authToken))
            {
                Debug.Log("[APIClient] Auth token loaded from PlayerPrefs");
            }
        }

        public void Logout()
        {
            authToken = "";
            PlayerPrefs.DeleteKey("auth_token");
            PlayerPrefs.Save();
            Debug.Log("[APIClient] Logged out");
        }

        // ===== Auth API =====
        public async Task<AuthResponse> Register(string email, string password, string playerName, string channelName)
        {
            var request = new RegisterRequest
            {
                Email = email,
                Password = password,
                PlayerName = playerName,
                ChannelName = channelName
            };

            string json = JsonUtility.ToJson(request);
            var response = await PostRequest<AuthResponse>("/auth/register", json, false);

            if (response != null && response.Success)
            {
                SetAuthToken(response.Token);
            }

            return response;
        }

        public async Task<AuthResponse> Login(string email, string password)
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            string json = JsonUtility.ToJson(request);
            var response = await PostRequest<AuthResponse>("/auth/login", json, false);

            if (response != null && response.Success)
            {
                SetAuthToken(response.Token);
            }

            return response;
        }

        // ===== Player API =====
        public async Task<PlayerDataResponse> GetPlayerData()
        {
            return await GetRequest<PlayerDataResponse>("/player/me");
        }

        public async Task<bool> SavePlayerData(UpdatePlayerDataRequest request)
        {
            string json = JsonUtility.ToJson(request);
            var response = await PutRequest<GenericResponse>("/player/save", json);
            return response != null && response.Success;
        }

        // ===== Gacha API =====
        public async Task<GachaResponse> DrawGacha(int drawCount, bool useTicket)
        {
            var request = new GachaRequest
            {
                DrawCount = drawCount,
                UseTicket = useTicket
            };

            string json = JsonUtility.ToJson(request);
            return await PostRequest<GachaResponse>("/gacha/draw", json);
        }

        // ===== Character API =====
        public async Task<CharacterResponse[]> GetAllCharacters()
        {
            return await GetRequest<CharacterResponse[]>("/characters");
        }

        public async Task<LevelUpResponse> LevelUpCharacter(string instanceId, int expChipsToUse)
        {
            var request = new LevelUpRequest { ExpChipsToUse = expChipsToUse };
            string json = JsonUtility.ToJson(request);
            return await PostRequest<LevelUpResponse>($"/player/characters/{instanceId}/levelup", json);
        }

        public async Task<BreakthroughResponse> BreakthroughCharacter(string instanceId, string sacrificeInstanceId)
        {
            var request = new BreakthroughRequest { SacrificeInstanceId = sacrificeInstanceId };
            string json = JsonUtility.ToJson(request);
            return await PostRequest<BreakthroughResponse>($"/player/characters/{instanceId}/breakthrough", json);
        }

        // ===== Content API =====
        public async Task<StartContentResponse> StartContent(string title, int genre, string[] assignedCharacterIds)
        {
            var request = new StartContentRequest
            {
                Title = title,
                Genre = genre,
                AssignedCharacterInstanceIds = assignedCharacterIds
            };

            string json = JsonUtility.ToJson(request);
            return await PostRequest<StartContentResponse>("/content/start", json);
        }

        public async Task<ContentResponse[]> GetProducingContent()
        {
            return await GetRequest<ContentResponse[]>("/content/producing");
        }

        public async Task<CompleteContentResponse> CompleteContent(string contentId)
        {
            return await PostRequest<CompleteContentResponse>($"/content/{contentId}/complete", "{}");
        }

        public async Task<UploadContentResponse> UploadContent(string contentId)
        {
            return await PostRequest<UploadContentResponse>($"/content/{contentId}/upload", "{}");
        }

        public async Task<ContentResponse[]> GetContentHistory(int page = 1, int pageSize = 20)
        {
            return await GetRequest<ContentResponse[]>($"/content/history?page={page}&pageSize={pageSize}");
        }

        // ===== HTTP Request Helpers =====
        private async Task<T> GetRequest<T>(string endpoint)
        {
            string url = baseUrl + endpoint;

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                if (IsLoggedIn)
                {
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"[APIClient] GET {endpoint} success: {jsonResponse}");
                    return JsonUtility.FromJson<T>(jsonResponse);
                }
                else
                {
                    Debug.LogError($"[APIClient] GET {endpoint} failed: {request.error}");
                    return default(T);
                }
            }
        }

        private async Task<T> PostRequest<T>(string endpoint, string jsonData, bool requireAuth = true)
        {
            string url = baseUrl + endpoint;

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                if (requireAuth && IsLoggedIn)
                {
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"[APIClient] POST {endpoint} success: {jsonResponse}");
                    return JsonUtility.FromJson<T>(jsonResponse);
                }
                else
                {
                    Debug.LogError($"[APIClient] POST {endpoint} failed: {request.error}\nResponse: {request.downloadHandler.text}");
                    return default(T);
                }
            }
        }

        private async Task<T> PutRequest<T>(string endpoint, string jsonData)
        {
            string url = baseUrl + endpoint;

            using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                if (IsLoggedIn)
                {
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }

                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"[APIClient] PUT {endpoint} success: {jsonResponse}");
                    return JsonUtility.FromJson<T>(jsonResponse);
                }
                else
                {
                    Debug.LogError($"[APIClient] PUT {endpoint} failed: {request.error}");
                    return default(T);
                }
            }
        }
    }

    // ===== DTO Classes =====
    [Serializable]
    public class RegisterRequest
    {
        public string Email;
        public string Password;
        public string PlayerName;
        public string ChannelName;
    }

    [Serializable]
    public class LoginRequest
    {
        public string Email;
        public string Password;
    }

    [Serializable]
    public class AuthResponse
    {
        public bool Success;
        public string Token;
        public string UserId;
        public string PlayerName;
        public string Message;
    }

    [Serializable]
    public class PlayerDataResponse
    {
        public string UserId;
        public string PlayerName;
        public string ChannelName;
        public long Subscribers;
        public long TotalViews;
        public long ChannelPower;
        public long Gold;
        public int Gems;
        public int GachaTickets;
        public int ExpChips;
        public int StudioLevel;
    }

    [Serializable]
    public class UpdatePlayerDataRequest
    {
        public long? Gold;
        public int? Gems;
        public long? Subscribers;
        public int? StudioLevel;
    }

    [Serializable]
    public class GachaRequest
    {
        public int DrawCount;
        public bool UseTicket;
    }

    [Serializable]
    public class GachaResult
    {
        public string InstanceId;
        public string CharacterId;
        public string CharacterName;
        public int Rarity;
        public int Specialty;
        public bool IsNew;
    }

    [Serializable]
    public class GachaResponse
    {
        public bool Success;
        public GachaResult[] Results;
        public int RemainingTickets;
        public int RemainingGems;
        public string Message;
    }

    [Serializable]
    public class CharacterResponse
    {
        public string CharacterId;
        public string CharacterName;
        public int Rarity;
        public int Specialty;
        public int BaseFilming;
        public int BaseEditing;
        public int BasePlanning;
        public int BaseDesign;
        public string PassiveSkillDesc;
        public float PassiveSkillValue;
    }

    [Serializable]
    public class LevelUpRequest
    {
        public int ExpChipsToUse;
    }

    [Serializable]
    public class LevelUpResponse
    {
        public bool Success;
        public string Message;
        public int NewLevel;
        public int CurrentExp;
        public int RequiredExp;
        public int MaxLevel;
        public int RemainingExpChips;
    }

    [Serializable]
    public class BreakthroughRequest
    {
        public string SacrificeInstanceId;
    }

    [Serializable]
    public class BreakthroughResponse
    {
        public bool Success;
        public string Message;
        public int NewBreakthrough;
        public int NewMaxLevel;
    }

    [Serializable]
    public class StartContentRequest
    {
        public string Title;
        public int Genre;
        public string[] AssignedCharacterInstanceIds;
    }

    [Serializable]
    public class ContentResponse
    {
        public string ContentId;
        public string Title;
        public int Genre;
        public int Status;
        public int FilmingScore;
        public int EditingScore;
        public int PlanningScore;
        public int DesignScore;
        public int TotalQuality;
        public int ProductionSeconds;
        public int RemainingSeconds;
        public long Views;
        public long Likes;
        public long Revenue;
    }

    [Serializable]
    public class StartContentResponse
    {
        public bool Success;
        public string Message;
        public ContentResponse Content;
    }

    [Serializable]
    public class CompleteContentResponse
    {
        public bool Success;
        public string Message;
        public ContentResponse Content;
    }

    [Serializable]
    public class UploadContentResponse
    {
        public bool Success;
        public string Message;
        public long Views;
        public long Likes;
        public long Revenue;
        public long NewSubscribers;
        public long TotalSubscribers;
    }

    [Serializable]
    public class GenericResponse
    {
        public bool Success;
        public string Message;
    }
}
