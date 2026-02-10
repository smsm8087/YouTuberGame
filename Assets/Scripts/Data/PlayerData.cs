using System;
using System.Collections.Generic;
using UnityEngine;

namespace YouTuberGame.Data
{
    /// <summary>
    /// 플레이어 데이터
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        [Header("기본 정보")]
        public string playerId;
        public string playerName;
        public DateTime createdDate;
        public DateTime lastLoginDate;

        [Header("채널 정보")]
        public string channelName;
        public long subscribers;      // 구독자 수
        public long totalViews;       // 총 조회수
        public long channelPower;     // 채널 파워 (4스탯 합산)

        [Header("재화")]
        public long gold;             // 광고 수익 (골드)
        public int gems;              // 보석
        public int gachaTickets;      // 가챠 티켓
        public int expChips;          // 경험치 칩

        [Header("게임 진행")]
        public int studioLevel;       // 스튜디오 레벨
        public List<ContentGenre> unlockedGenres;  // 해금된 장르

        [Header("보유 캐릭터")]
        public List<CharacterInstance> ownedCharacters;

        [Header("제작 중인 콘텐츠")]
        public List<ContentInstance> producingContents;

        [Header("업로드된 콘텐츠")]
        public List<ContentInstance> uploadedContents;

        [Header("장비")]
        public Dictionary<string, int> equipmentLevels;  // 장비별 레벨

        public PlayerData()
        {
            playerId = Guid.NewGuid().ToString();
            createdDate = DateTime.Now;
            lastLoginDate = DateTime.Now;

            channelName = "새로운 채널";
            subscribers = 0;
            totalViews = 0;
            channelPower = 0;

            gold = 1000;
            gems = 100;
            gachaTickets = 10;
            expChips = 0;

            studioLevel = 1;
            unlockedGenres = new List<ContentGenre> { ContentGenre.Vlog };

            ownedCharacters = new List<CharacterInstance>();
            producingContents = new List<ContentInstance>();
            uploadedContents = new List<ContentInstance>();
            equipmentLevels = new Dictionary<string, int>();
        }

        public void AddGold(long amount)
        {
            gold += amount;
        }

        public bool SpendGold(long amount)
        {
            if (gold >= amount)
            {
                gold -= amount;
                return true;
            }
            return false;
        }

        public void AddGems(int amount)
        {
            gems += amount;
        }

        public bool SpendGems(int amount)
        {
            if (gems >= amount)
            {
                gems -= amount;
                return true;
            }
            return false;
        }

        public void AddSubscribers(long amount)
        {
            subscribers += amount;
            CheckMilestones();
        }

        private void CheckMilestones()
        {
            // TODO: 마일스톤 달성 체크
            // 구독자 수에 따른 장르 해금, 보상 지급 등
        }

        public void UpdateChannelPower()
        {
            // TODO: 보유 캐릭터들의 스탯 합산
            channelPower = 0;
            foreach (var character in ownedCharacters)
            {
                channelPower += character.CurrentStats.TotalPower;
            }
        }
    }
}
