using UnityEngine;
using YouTuberGame.Data;
using System.IO;

namespace YouTuberGame.Managers
{
    /// <summary>
    /// 데이터 저장/로드 관리
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        private static DataManager instance;
        public static DataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("DataManager");
                    instance = go.AddComponent<DataManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private PlayerData currentPlayerData;
        public PlayerData CurrentPlayer => currentPlayerData;

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "playerdata.json");

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

        public void Initialize()
        {
            Debug.Log("[DataManager] Initializing...");
            LoadPlayerData();
        }

        public void CreateNewPlayer(string playerName)
        {
            currentPlayerData = new PlayerData
            {
                playerName = playerName,
                channelName = $"{playerName}의 채널"
            };

            // TODO: 기본 팀원 지급
            SavePlayerData();
        }

        public void LoadPlayerData()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    currentPlayerData = JsonUtility.FromJson<PlayerData>(json);
                    currentPlayerData.lastLoginDate = System.DateTime.Now;
                    Debug.Log($"[DataManager] Player data loaded: {currentPlayerData.playerName}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[DataManager] Failed to load player data: {e.Message}");
                    CreateNewPlayer("플레이어");
                }
            }
            else
            {
                Debug.Log("[DataManager] No save file found. Creating new player...");
                CreateNewPlayer("플레이어");
            }
        }

        public void SavePlayerData()
        {
            try
            {
                string json = JsonUtility.ToJson(currentPlayerData, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"[DataManager] Player data saved to {SaveFilePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DataManager] Failed to save player data: {e.Message}");
            }
        }

        public void DeleteSaveData()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
                Debug.Log("[DataManager] Save data deleted.");
            }
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
    }
}
