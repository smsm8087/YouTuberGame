using System.Collections.Generic;
using UnityEngine;
using YouTuberGame.Data;

namespace YouTuberGame.Managers
{
    /// <summary>
    /// 캐릭터(팀원) 관리
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        private static CharacterManager instance;
        public static CharacterManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("CharacterManager");
                    instance = go.AddComponent<CharacterManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Character Database")]
        [SerializeField] private List<CharacterData> allCharacters;

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
            Debug.Log("[CharacterManager] Initializing...");
            LoadCharacterDatabase();
        }

        private void LoadCharacterDatabase()
        {
            // TODO: Resources 폴더에서 모든 CharacterData 로드
            CharacterData[] characters = Resources.LoadAll<CharacterData>("Characters");
            allCharacters = new List<CharacterData>(characters);
            Debug.Log($"[CharacterManager] Loaded {allCharacters.Count} characters");
        }

        /// <summary>
        /// 가챠로 캐릭터 뽑기
        /// </summary>
        public CharacterInstance DrawCharacter()
        {
            if (allCharacters == null || allCharacters.Count == 0)
            {
                Debug.LogError("[CharacterManager] No characters in database!");
                return null;
            }

            // TODO: 확률 기반 가챠 로직
            CharacterRarity drawnRarity = GetRandomRarity();
            CharacterData drawnCharacter = GetRandomCharacterByRarity(drawnRarity);

            if (drawnCharacter != null)
            {
                CharacterInstance newInstance = new CharacterInstance(drawnCharacter);
                DataManager.Instance.CurrentPlayer.ownedCharacters.Add(newInstance);
                Debug.Log($"[CharacterManager] Drew {drawnCharacter.characterName} ({drawnRarity})");
                return newInstance;
            }

            return null;
        }

        private CharacterRarity GetRandomRarity()
        {
            float roll = Random.Range(0f, 100f);

            // C: 50%, B: 30%, A: 15%, S: 5%
            if (roll < 5f) return CharacterRarity.S;
            if (roll < 20f) return CharacterRarity.A;
            if (roll < 50f) return CharacterRarity.B;
            return CharacterRarity.C;
        }

        private CharacterData GetRandomCharacterByRarity(CharacterRarity rarity)
        {
            List<CharacterData> candidates = allCharacters.FindAll(c => c.rarity == rarity);
            if (candidates.Count > 0)
            {
                return candidates[Random.Range(0, candidates.Count)];
            }
            return null;
        }

        /// <summary>
        /// 캐릭터 레벨업
        /// </summary>
        public bool LevelUpCharacter(CharacterInstance character, int expAmount)
        {
            if (character == null) return false;

            int maxLevel = character.characterData.GetMaxLevel();
            if (character.level >= maxLevel) return false;

            character.experience += expAmount;

            // TODO: 레벨업 로직
            int requiredExp = GetRequiredExp(character.level);
            if (character.experience >= requiredExp)
            {
                character.level++;
                character.experience -= requiredExp;
                Debug.Log($"[CharacterManager] {character.characterData.characterName} leveled up to {character.level}!");
                return true;
            }

            return false;
        }

        private int GetRequiredExp(int currentLevel)
        {
            // TODO: 레벨별 필요 경험치 공식
            return 100 + (currentLevel * 50);
        }

        /// <summary>
        /// 캐릭터 돌파
        /// </summary>
        public bool BreakthroughCharacter(CharacterInstance character)
        {
            if (character == null) return false;
            if (character.breakthrough >= 5) return false;  // 최대 5돌파

            // TODO: 돌파 재료 체크
            character.breakthrough++;
            Debug.Log($"[CharacterManager] {character.characterData.characterName} breakthrough to +{character.breakthrough}!");
            return true;
        }

        /// <summary>
        /// 팀 총 스탯 계산
        /// </summary>
        public CharacterStats CalculateTeamStats(List<CharacterInstance> team)
        {
            CharacterStats totalStats = new CharacterStats(0, 0, 0, 0);

            foreach (var member in team)
            {
                if (member != null)
                {
                    totalStats.filming += member.CurrentStats.filming;
                    totalStats.editing += member.CurrentStats.editing;
                    totalStats.planning += member.CurrentStats.planning;
                    totalStats.design += member.CurrentStats.design;
                }
            }

            return totalStats;
        }
    }
}
