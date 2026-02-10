using UnityEngine;
using System;

namespace YouTuberGame.Data
{
    /// <summary>
    /// 캐릭터(팀원) 등급
    /// </summary>
    public enum CharacterRarity
    {
        C = 0,  // 일반 (50%)
        B = 1,  // 우수 (30%)
        A = 2,  // 희귀 (15%)
        S = 3   // 전설 (5%)
    }

    /// <summary>
    /// 캐릭터 특화 스탯
    /// </summary>
    public enum CharacterSpecialty
    {
        Filming,   // 촬영 특화
        Editing,   // 편집 특화
        Planning,  // 기획 특화
        Design     // 디자인 특화
    }

    /// <summary>
    /// 캐릭터 4대 스탯
    /// </summary>
    [Serializable]
    public class CharacterStats
    {
        public int filming;    // 촬영력
        public int editing;    // 편집력
        public int planning;   // 기획력
        public int design;     // 디자인력

        public int TotalPower => filming + editing + planning + design;

        public CharacterStats(int f, int e, int p, int d)
        {
            filming = f;
            editing = e;
            planning = p;
            design = d;
        }
    }

    /// <summary>
    /// 캐릭터 데이터 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "New Character", menuName = "YouTuber Game/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("기본 정보")]
        public string characterId;
        public string characterName;
        public CharacterRarity rarity;
        public CharacterSpecialty specialty;

        [Header("스탯")]
        public CharacterStats baseStats;

        [Header("비주얼")]
        public Sprite characterSprite;
        public string spineAnimationName;

        [Header("스킬/패시브")]
        [TextArea(3, 5)]
        public string passiveSkillDescription;
        public float passiveSkillValue;

        [Header("대사")]
        [TextArea(2, 4)]
        public string[] dialogues;

        public int GetMaxLevel()
        {
            return rarity switch
            {
                CharacterRarity.C => 30,
                CharacterRarity.B => 40,
                CharacterRarity.A => 50,
                CharacterRarity.S => 60,
                _ => 30
            };
        }
    }

    /// <summary>
    /// 플레이어가 보유한 캐릭터 인스턴스
    /// </summary>
    [Serializable]
    public class CharacterInstance
    {
        public string instanceId;
        public CharacterData characterData;
        public int level;
        public int experience;
        public int breakthrough;  // 돌파 횟수

        public CharacterStats CurrentStats
        {
            get
            {
                // TODO: 레벨과 돌파에 따른 스탯 계산
                return characterData.baseStats;
            }
        }

        public CharacterInstance(CharacterData data)
        {
            instanceId = Guid.NewGuid().ToString();
            characterData = data;
            level = 1;
            experience = 0;
            breakthrough = 0;
        }
    }
}
