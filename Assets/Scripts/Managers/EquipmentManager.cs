using System.Collections.Generic;
using UnityEngine;
using YouTuberGame.Data;

namespace YouTuberGame.Managers
{
    /// <summary>
    /// 장비 시스템 관리
    /// </summary>
    public class EquipmentManager : MonoBehaviour
    {
        private static EquipmentManager instance;
        public static EquipmentManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("EquipmentManager");
                    instance = go.AddComponent<EquipmentManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Equipment Database")]
        [SerializeField] private List<EquipmentData> allEquipment;

        private Dictionary<EquipmentType, EquipmentUpgradeInfo> equipmentStatus;

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
            Debug.Log("[EquipmentManager] Initializing...");
            LoadEquipmentDatabase();
            InitializeEquipmentStatus();
        }

        private void LoadEquipmentDatabase()
        {
            EquipmentData[] equipment = Resources.LoadAll<EquipmentData>("Equipment");
            allEquipment = new List<EquipmentData>(equipment);
            Debug.Log($"[EquipmentManager] Loaded {allEquipment.Count} equipment items");
        }

        private void InitializeEquipmentStatus()
        {
            equipmentStatus = new Dictionary<EquipmentType, EquipmentUpgradeInfo>();

            foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType)))
            {
                equipmentStatus[type] = new EquipmentUpgradeInfo(type);
            }
        }

        /// <summary>
        /// 장비 업그레이드
        /// </summary>
        public bool UpgradeEquipment(EquipmentType type)
        {
            if (!equipmentStatus.ContainsKey(type))
            {
                Debug.LogError($"[EquipmentManager] Equipment type {type} not found!");
                return false;
            }

            EquipmentUpgradeInfo info = equipmentStatus[type];

            if (info.currentLevel >= 5)
            {
                Debug.LogWarning($"[EquipmentManager] {type} is already at max level!");
                return false;
            }

            long cost = GetUpgradeCost(type, info.currentLevel);

            if (!DataManager.Instance.CurrentPlayer.SpendGold(cost))
            {
                Debug.LogWarning($"[EquipmentManager] Not enough gold to upgrade {type}!");
                return false;
            }

            info.currentLevel++;
            Debug.Log($"[EquipmentManager] Upgraded {type} to level {info.currentLevel}: {info.GetCurrentLevelName()}");

            // TODO: 스튜디오 뷰에 장비 변경 반영

            return true;
        }

        /// <summary>
        /// 업그레이드 비용 계산
        /// </summary>
        public long GetUpgradeCost(EquipmentType type, int currentLevel)
        {
            long baseCost = 500;
            return baseCost * currentLevel * currentLevel;
        }

        /// <summary>
        /// 전체 장비 스탯 보너스 계산
        /// </summary>
        public CharacterStats GetTotalEquipmentBonus()
        {
            CharacterStats bonus = new CharacterStats(0, 0, 0, 0);

            foreach (var kvp in equipmentStatus)
            {
                EquipmentType type = kvp.Key;
                int level = kvp.Value.currentLevel;

                // 레벨에 따른 스탯 보너스
                int statBonus = level * 10;

                switch (type)
                {
                    case EquipmentType.Camera:
                        bonus.filming += statBonus;
                        break;
                    case EquipmentType.PC:
                        bonus.editing += statBonus;
                        break;
                    case EquipmentType.Microphone:
                        bonus.filming += statBonus / 2;
                        bonus.editing += statBonus / 2;
                        break;
                    case EquipmentType.Lighting:
                        bonus.design += statBonus;
                        break;
                    case EquipmentType.Tablet:
                        bonus.design += statBonus / 2;
                        bonus.planning += statBonus / 2;
                        break;
                }
            }

            return bonus;
        }

        public EquipmentUpgradeInfo GetEquipmentInfo(EquipmentType type)
        {
            return equipmentStatus.ContainsKey(type) ? equipmentStatus[type] : null;
        }

        public Dictionary<EquipmentType, EquipmentUpgradeInfo> GetAllEquipmentStatus()
        {
            return equipmentStatus;
        }
    }
}
