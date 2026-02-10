using System.Collections.Generic;
using UnityEngine;
using YouTuberGame.Data;

namespace YouTuberGame.Managers
{
    /// <summary>
    /// 스튜디오 구역 타입
    /// </summary>
    public enum StudioZone
    {
        FilmingBooth,   // 촬영 부스
        EditingDesk,    // 편집 데스크
        PlanningRoom,   // 기획 회의실
        DesignStation   // 디자인 석
    }

    /// <summary>
    /// 스튜디오 관리 (메인 화면)
    /// </summary>
    public class StudioManager : MonoBehaviour
    {
        private static StudioManager instance;
        public static StudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("StudioManager");
                    instance = go.AddComponent<StudioManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Studio Zones")]
        [SerializeField] private Dictionary<StudioZone, List<CharacterInstance>> zoneAssignments;

        [Header("Studio Level")]
        private int currentStudioLevel = 1;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                zoneAssignments = new Dictionary<StudioZone, List<CharacterInstance>>();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void Initialize()
        {
            Debug.Log("[StudioManager] Initializing...");
            InitializeZones();
            LoadStudioLevel();
        }

        private void InitializeZones()
        {
            foreach (StudioZone zone in System.Enum.GetValues(typeof(StudioZone)))
            {
                zoneAssignments[zone] = new List<CharacterInstance>();
            }
        }

        private void LoadStudioLevel()
        {
            currentStudioLevel = DataManager.Instance.CurrentPlayer.studioLevel;
            Debug.Log($"[StudioManager] Studio Level: {currentStudioLevel}");
        }

        /// <summary>
        /// 캐릭터를 스튜디오 구역에 배치
        /// </summary>
        public bool AssignCharacterToZone(CharacterInstance character, StudioZone zone)
        {
            if (character == null) return false;

            // 최대 배치 인원 체크
            int maxCapacity = GetZoneCapacity(zone);
            if (zoneAssignments[zone].Count >= maxCapacity)
            {
                Debug.LogWarning($"[StudioManager] Zone {zone} is full!");
                return false;
            }

            // 다른 구역에서 제거
            RemoveCharacterFromAllZones(character);

            // 배치
            zoneAssignments[zone].Add(character);
            Debug.Log($"[StudioManager] Assigned {character.characterData.characterName} to {zone}");

            return true;
        }

        /// <summary>
        /// 캐릭터를 모든 구역에서 제거
        /// </summary>
        public void RemoveCharacterFromAllZones(CharacterInstance character)
        {
            foreach (var zone in zoneAssignments.Keys)
            {
                zoneAssignments[zone].Remove(character);
            }
        }

        /// <summary>
        /// 구역의 최대 수용 인원
        /// </summary>
        private int GetZoneCapacity(StudioZone zone)
        {
            // 스튜디오 레벨에 따라 수용 인원 증가
            return zone switch
            {
                StudioZone.FilmingBooth => 1 + (currentStudioLevel / 3),  // 1~2명
                StudioZone.EditingDesk => 1 + (currentStudioLevel / 3),   // 1~2명
                StudioZone.PlanningRoom => 1,                              // 1명
                StudioZone.DesignStation => 1,                             // 1명
                _ => 1
            };
        }

        /// <summary>
        /// 현재 작업 중인 팀 가져오기
        /// </summary>
        public List<CharacterInstance> GetCurrentTeam()
        {
            List<CharacterInstance> team = new List<CharacterInstance>();

            foreach (var zone in zoneAssignments.Values)
            {
                team.AddRange(zone);
            }

            return team;
        }

        /// <summary>
        /// 스튜디오 레벨업
        /// </summary>
        public bool UpgradeStudio(long cost)
        {
            if (!DataManager.Instance.CurrentPlayer.SpendGold(cost))
            {
                Debug.LogWarning("[StudioManager] Not enough gold to upgrade studio!");
                return false;
            }

            currentStudioLevel++;
            DataManager.Instance.CurrentPlayer.studioLevel = currentStudioLevel;

            Debug.Log($"[StudioManager] Studio upgraded to level {currentStudioLevel}!");
            // TODO: 배경 변경 (원룸 → 오피스 → 대형 스튜디오)

            return true;
        }

        public int GetStudioLevel() => currentStudioLevel;

        public long GetUpgradeCost()
        {
            // 레벨에 따른 업그레이드 비용
            return 1000 * currentStudioLevel * currentStudioLevel;
        }

        /// <summary>
        /// 특정 구역에 배치된 캐릭터 목록
        /// </summary>
        public List<CharacterInstance> GetCharactersInZone(StudioZone zone)
        {
            return zoneAssignments.ContainsKey(zone) ? zoneAssignments[zone] : new List<CharacterInstance>();
        }
    }
}
