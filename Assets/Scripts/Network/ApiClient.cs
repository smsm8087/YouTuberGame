using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// HTTP 통신 핵심 클래스
/// UnityWebRequest 기반, JWT 토큰 자동 첨부
/// 모든 API 호출은 이 클래스를 통해 수행
/// </summary>
public class ApiClient : MonoBehaviour
{
    private static ApiClient _instance;
    public static ApiClient Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[ApiClient]");
                _instance = go.AddComponent<ApiClient>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private string _authToken;
    private string _userId;

    public string UserId => _userId;
    public bool IsLoggedIn => !string.IsNullOrEmpty(_authToken);

    public void SetAuth(string token, string userId)
    {
        _authToken = token;
        _userId = userId;
        PlayerPrefs.SetString("auth_token", token);
        PlayerPrefs.SetString("user_id", userId);
        PlayerPrefs.Save();
    }

    public void LoadSavedAuth()
    {
        _authToken = PlayerPrefs.GetString("auth_token", "");
        _userId = PlayerPrefs.GetString("user_id", "");
    }

    public void ClearAuth()
    {
        _authToken = null;
        _userId = null;
        PlayerPrefs.DeleteKey("auth_token");
        PlayerPrefs.DeleteKey("user_id");
        PlayerPrefs.Save();
    }

    // ===== HTTP Methods =====

    public void Get<T>(string endpoint, Action<bool, T> callback)
    {
        StartCoroutine(SendRequest<T>("GET", endpoint, null, callback));
    }

    public void Post<TReq, TRes>(string endpoint, TReq body, Action<bool, TRes> callback)
    {
        string json = JsonUtility.ToJson(body);
        StartCoroutine(SendRequest<TRes>("POST", endpoint, json, callback));
    }

    public void Post<T>(string endpoint, Action<bool, T> callback)
    {
        StartCoroutine(SendRequest<T>("POST", endpoint, null, callback));
    }

    public void Put<TReq, TRes>(string endpoint, TReq body, Action<bool, TRes> callback)
    {
        string json = JsonUtility.ToJson(body);
        StartCoroutine(SendRequest<TRes>("PUT", endpoint, json, callback));
    }

    private IEnumerator SendRequest<T>(string method, string endpoint, string jsonBody, Action<bool, T> callback)
    {
        string url = $"{ApiConfig.Instance.BaseUrl}{endpoint}";

        using var request = new UnityWebRequest(url, method);

        if (!string.IsNullOrEmpty(jsonBody))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        }

        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(_authToken))
            request.SetRequestHeader("Authorization", $"Bearer {_authToken}");

        request.timeout = (int)ApiConfig.Instance.TimeoutSeconds;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                T response = JsonUtility.FromJson<T>(request.downloadHandler.text);
                callback?.Invoke(true, response);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ApiClient] JSON parse error: {e.Message}\nResponse: {request.downloadHandler.text}");
                callback?.Invoke(false, default);
            }
        }
        else
        {
            Debug.LogError($"[ApiClient] {method} {endpoint} failed: {request.error}");

            // 서버가 에러 JSON을 보냈으면 파싱 시도
            if (!string.IsNullOrEmpty(request.downloadHandler?.text))
            {
                try
                {
                    T response = JsonUtility.FromJson<T>(request.downloadHandler.text);
                    callback?.Invoke(false, response);
                    yield break;
                }
                catch { }
            }

            callback?.Invoke(false, default);
        }
    }
}
