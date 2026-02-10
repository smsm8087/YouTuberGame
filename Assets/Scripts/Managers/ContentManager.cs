using System.Collections.Generic;
using UnityEngine;
using YouTuberGame.Data;
using System;
using System.Linq;

namespace YouTuberGame.Managers
{
    /// <summary>
    /// 콘텐츠 제작 및 관리
    /// </summary>
    public class ContentManager : MonoBehaviour
    {
        private static ContentManager instance;
        public static ContentManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("ContentManager");
                    instance = go.AddComponent<ContentManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Content Genre Database")]
        [SerializeField] private List<ContentGenreData> allGenres;

        [Header("Trend System")]
        private ContentGenre currentTrendGenre;
        private DateTime trendChangeTime;

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
            Debug.Log("[ContentManager] Initializing...");
            LoadGenreDatabase();
            UpdateDailyTrend();
        }

        private void LoadGenreDatabase()
        {
            // TODO: Resources 폴더에서 모든 ContentGenreData 로드
            ContentGenreData[] genres = Resources.LoadAll<ContentGenreData>("ContentGenres");
            allGenres = new List<ContentGenreData>(genres);
            Debug.Log($"[ContentManager] Loaded {allGenres.Count} content genres");
        }

        /// <summary>
        /// 콘텐츠 제작 시작
        /// </summary>
        public ContentInstance StartProduction(ContentGenre genre, List<CharacterInstance> team)
        {
            if (team == null || team.Count == 0)
            {
                Debug.LogError("[ContentManager] No team members assigned!");
                return null;
            }

            // 팀 스탯 계산
            CharacterStats teamStats = CharacterManager.Instance.CalculateTeamStats(team);
            string[] teamIds = team.Select(c => c.instanceId).ToArray();

            // 콘텐츠 인스턴스 생성
            ContentInstance content = new ContentInstance(genre, teamStats.TotalPower, teamIds);

            // 제작 중 목록에 추가
            DataManager.Instance.CurrentPlayer.producingContents.Add(content);

            Debug.Log($"[ContentManager] Started producing {genre} content with {team.Count} members (Total Power: {teamStats.TotalPower})");

            return content;
        }

        /// <summary>
        /// 콘텐츠 제작 완료 체크
        /// </summary>
        public bool IsProductionComplete(ContentInstance content)
        {
            if (content == null) return false;

            ContentGenreData genreData = GetGenreData(content.genre);
            if (genreData == null) return false;

            TimeSpan elapsed = DateTime.Now - content.createdTime;
            return elapsed.TotalMinutes >= genreData.productionTimeMinutes;
        }

        /// <summary>
        /// 콘텐츠 업로드
        /// </summary>
        public void UploadContent(ContentInstance content)
        {
            if (content == null) return;

            // 트렌드 보너스 적용
            if (content.genre == currentTrendGenre)
            {
                content.views = (long)(content.views * 1.5f);
                Debug.Log($"[ContentManager] Trend bonus applied! +50% views");
            }

            content.Upload();

            // 제작 중 목록에서 제거하고 업로드된 목록에 추가
            PlayerData player = DataManager.Instance.CurrentPlayer;
            player.producingContents.Remove(content);
            player.uploadedContents.Add(content);

            // 구독자 및 수익 증가
            player.AddSubscribers(content.views / 100);  // 조회수 100당 구독자 1명
            player.AddGold(content.revenue);

            Debug.Log($"[ContentManager] Uploaded {content.genre} content! Views: {content.views}, Revenue: {content.revenue}");
        }

        /// <summary>
        /// 일일 트렌드 업데이트
        /// </summary>
        public void UpdateDailyTrend()
        {
            // 매일 자정에 트렌드 변경
            DateTime now = DateTime.Now;
            if (trendChangeTime.Date != now.Date)
            {
                // 잠금 해제된 장르 중 랜덤 선택
                List<ContentGenre> unlockedGenres = DataManager.Instance.CurrentPlayer.unlockedGenres;
                if (unlockedGenres.Count > 0)
                {
                    currentTrendGenre = unlockedGenres[UnityEngine.Random.Range(0, unlockedGenres.Count)];
                    trendChangeTime = now;
                    Debug.Log($"[ContentManager] Today's trend: {currentTrendGenre}");
                }
            }
        }

        public ContentGenre GetCurrentTrend() => currentTrendGenre;

        public ContentGenreData GetGenreData(ContentGenre genre)
        {
            return allGenres?.Find(g => g.genre == genre);
        }

        /// <summary>
        /// 장르 해금 체크
        /// </summary>
        public void CheckGenreUnlocks()
        {
            PlayerData player = DataManager.Instance.CurrentPlayer;
            long subs = player.subscribers;

            List<ContentGenre> toUnlock = new List<ContentGenre>();

            if (subs >= 1000 && !player.unlockedGenres.Contains(ContentGenre.Gaming))
                toUnlock.Add(ContentGenre.Gaming);

            if (subs >= 5000 && !player.unlockedGenres.Contains(ContentGenre.Mukbang))
                toUnlock.Add(ContentGenre.Mukbang);

            if (subs >= 20000 && !player.unlockedGenres.Contains(ContentGenre.Education))
                toUnlock.Add(ContentGenre.Education);

            if (subs >= 50000 && !player.unlockedGenres.Contains(ContentGenre.Shorts))
                toUnlock.Add(ContentGenre.Shorts);

            if (subs >= 200000 && !player.unlockedGenres.Contains(ContentGenre.Documentary))
                toUnlock.Add(ContentGenre.Documentary);

            foreach (var genre in toUnlock)
            {
                player.unlockedGenres.Add(genre);
                Debug.Log($"[ContentManager] New genre unlocked: {genre}!");
                // TODO: UI 알림
            }
        }
    }
}
