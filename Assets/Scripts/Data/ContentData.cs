using UnityEngine;
using System;

namespace YouTuberGame.Data
{
    /// <summary>
    /// 콘텐츠 장르
    /// </summary>
    public enum ContentGenre
    {
        Vlog,       // 브이로그 (시작)
        Gaming,     // 게임 (구독자 1,000)
        Mukbang,    // 먹방 (구독자 5,000)
        Education,  // 교육 (구독자 20,000)
        Shorts,     // 쇼츠 (구독자 50,000)
        Documentary // 다큐 (구독자 200,000)
    }

    /// <summary>
    /// 콘텐츠 품질 등급
    /// </summary>
    public enum ContentQuality
    {
        D = 0,
        C = 1,
        B = 2,
        A = 3,
        S = 4
    }

    /// <summary>
    /// 콘텐츠 장르별 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "New Content Genre", menuName = "YouTuber Game/Content Genre Data")]
    public class ContentGenreData : ScriptableObject
    {
        [Header("기본 정보")]
        public ContentGenre genre;
        public string genreName;
        public int unlockSubscribers;  // 해금 구독자 수

        [Header("제작 시간")]
        public float productionTimeMinutes;  // 제작 시간 (분)

        [Header("스탯 의존도")]
        [Range(0f, 1f)] public float filmingWeight = 0.25f;
        [Range(0f, 1f)] public float editingWeight = 0.25f;
        [Range(0f, 1f)] public float planningWeight = 0.25f;
        [Range(0f, 1f)] public float designWeight = 0.25f;

        [Header("수익")]
        public int baseViewsMultiplier = 100;
        public float baseRevenuePerView = 0.5f;

        [Header("비주얼")]
        public Sprite genreIcon;

        [TextArea(2, 4)]
        public string description;
    }

    /// <summary>
    /// 제작된 콘텐츠 인스턴스
    /// </summary>
    [Serializable]
    public class ContentInstance
    {
        public string contentId;
        public ContentGenre genre;
        public ContentQuality quality;
        public DateTime createdTime;
        public DateTime uploadTime;

        [Header("스탯")]
        public int totalStats;  // 제작 당시 팀 스탯 합산
        public long views;
        public long revenue;

        [Header("제작 팀원")]
        public string[] teamMemberIds;

        public ContentInstance(ContentGenre genre, int stats, string[] teamIds)
        {
            contentId = Guid.NewGuid().ToString();
            this.genre = genre;
            totalStats = stats;
            teamMemberIds = teamIds;
            createdTime = DateTime.Now;

            // 품질 판정
            quality = CalculateQuality(stats);
        }

        private ContentQuality CalculateQuality(int stats)
        {
            // TODO: 스탯 기반 품질 계산 로직
            if (stats >= 400) return ContentQuality.S;
            if (stats >= 300) return ContentQuality.A;
            if (stats >= 200) return ContentQuality.B;
            if (stats >= 100) return ContentQuality.C;
            return ContentQuality.D;
        }

        public void Upload()
        {
            uploadTime = DateTime.Now;
            // TODO: 조회수/수익 시뮬레이션
            SimulateViews();
        }

        private void SimulateViews()
        {
            // TODO: 품질과 스탯 기반 조회수 계산
            int baseViews = totalStats * 10;
            int qualityBonus = (int)quality * 50;
            views = baseViews + qualityBonus;
            revenue = (long)(views * 0.5f);
        }
    }
}
