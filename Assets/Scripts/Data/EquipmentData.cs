using UnityEngine;
using System;

namespace YouTuberGame.Data
{
    /// <summary>
    /// 장비 타입
    /// </summary>
    public enum EquipmentType
    {
        Camera,      // 카메라 (촬영력)
        PC,          // PC (편집력)
        Microphone,  // 마이크 (촬영력 + 편집력)
        Lighting,    // 조명 (디자인력)
        Tablet       // 태블릿 (디자인력 + 기획력)
    }

    /// <summary>
    /// 장비 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "New Equipment", menuName = "YouTuber Game/Equipment Data")]
    public class EquipmentData : ScriptableObject
    {
        [Header("기본 정보")]
        public EquipmentType equipmentType;
        public string equipmentName;
        public int level;  // 1~5 단계

        [Header("스탯 보너스")]
        public int filmingBonus;
        public int editingBonus;
        public int planningBonus;
        public int designBonus;

        [Header("비용")]
        public long purchaseCost;

        [Header("비주얼")]
        public Sprite equipmentSprite;

        [TextArea(2, 4)]
        public string description;

        public int GetTotalStatBonus()
        {
            return filmingBonus + editingBonus + planningBonus + designBonus;
        }
    }

    /// <summary>
    /// 장비 업그레이드 정보
    /// </summary>
    [Serializable]
    public class EquipmentUpgradeInfo
    {
        public EquipmentType type;
        public int currentLevel;
        public string[] levelNames;  // 예: "스마트폰", "미러리스", "시네마 카메라"

        public EquipmentUpgradeInfo(EquipmentType type)
        {
            this.type = type;
            currentLevel = 1;
            levelNames = GetDefaultLevelNames(type);
        }

        private string[] GetDefaultLevelNames(EquipmentType type)
        {
            return type switch
            {
                EquipmentType.Camera => new[] { "스마트폰", "미러리스", "DSLR", "시네마 카메라", "전문 시네마 카메라" },
                EquipmentType.PC => new[] { "저사양 노트북", "노트북", "데스크탑", "워크스테이션", "전문 워크스테이션" },
                EquipmentType.Microphone => new[] { "이어폰", "USB 마이크", "콘덴서 마이크", "방송용 마이크", "스튜디오 마이크" },
                EquipmentType.Lighting => new[] { "스탠드", "링라이트", "소프트박스", "전문 조명", "스튜디오 조명" },
                EquipmentType.Tablet => new[] { "보급형", "중급", "준프로", "프로", "전문가용" },
                _ => new[] { "Lv1", "Lv2", "Lv3", "Lv4", "Lv5" }
            };
        }

        public string GetCurrentLevelName()
        {
            if (currentLevel > 0 && currentLevel <= levelNames.Length)
            {
                return levelNames[currentLevel - 1];
            }
            return "Unknown";
        }
    }
}
