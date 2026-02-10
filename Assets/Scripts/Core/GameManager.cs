using UnityEngine;

namespace YouTuberGame.Core
{
    /// <summary>
    /// 게임 전체를 관리하는 메인 매니저
    /// 싱글톤 패턴으로 구현
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<GameManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("Game Settings")]
        [SerializeField] private bool isDebugMode = true;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeGame()
        {
            Debug.Log("[GameManager] Initializing YouTuber Game...");

            // TODO: 매니저들 초기화
            // DataManager 초기화
            // StudioManager 초기화
            // CharacterManager 초기화
            // etc...

            Debug.Log("[GameManager] Game initialized successfully!");
        }

        private void Start()
        {
            // TODO: 게임 시작 로직
            LoadPlayerData();
        }

        private void LoadPlayerData()
        {
            Debug.Log("[GameManager] Loading player data...");
            // TODO: 플레이어 데이터 로드
        }

        private void OnApplicationQuit()
        {
            SavePlayerData();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SavePlayerData();
            }
        }

        private void SavePlayerData()
        {
            Debug.Log("[GameManager] Saving player data...");
            // TODO: 플레이어 데이터 저장
        }

        public void DebugLog(string message)
        {
            if (isDebugMode)
            {
                Debug.Log($"[YouTuberGame] {message}");
            }
        }
    }
}
