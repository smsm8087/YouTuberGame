using UnityEngine;

/// <summary>
/// API 서버 설정 (ScriptableObject)
/// Resources/ApiConfig 에 배치하여 사용
/// 서버 이사 시 이 파일만 수정하면 됨
/// </summary>
[CreateAssetMenu(fileName = "ApiConfig", menuName = "YouTuberGame/ApiConfig")]
public class ApiConfig : ScriptableObject
{
    [Header("Server")]
    [SerializeField] private string _baseUrl = "https://api.youtubertycoon.com/v1";
    [SerializeField] private float _timeoutSeconds = 10f;
    [SerializeField] private int _maxRetryCount = 2;

    public string BaseUrl => _baseUrl;
    public float TimeoutSeconds => _timeoutSeconds;
    public int MaxRetryCount => _maxRetryCount;

    private static ApiConfig _instance;
    public static ApiConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<ApiConfig>("ApiConfig");
            return _instance;
        }
    }
}
