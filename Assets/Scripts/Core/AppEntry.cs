using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 앱 진입점 - 매니저 초기화 순서 보장
/// Login 씬에 배치. APIClient, MasterDataManager도 같은 씬에 배치.
///
/// 초기화 순서:
///   1. APIClient (Awake에서 자동 초기화)
///   2. MasterDataManager (Awake에서 캐시 로드)
///   3. AppEntry.Start() → 서버에서 마스터데이터 갱신 → LoginPopup 표시
/// </summary>
public class AppEntry : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(InitializeApp());
    }

    IEnumerator InitializeApp()
    {
        // 1. 마스터 데이터 서버에서 갱신 (캐시 있으면 버전 체크만)
        bool masterDataLoaded = false;
        yield return MasterDataManager.Instance.LoadFromServer(ok =>
        {
            masterDataLoaded = ok;
        });

        if (!masterDataLoaded)
        {
            Debug.LogWarning("[AppEntry] 마스터 데이터 로드 실패, 캐시로 진행");
        }

        // 2. 토큰 만료 시 로그인 씬으로 복귀
        APIClient.OnTokenExpired += OnTokenExpired;

        // 3. 로그인 팝업 표시
        UIManager.Instance.ShowPopup<LoginPopup>();
    }

    void OnTokenExpired()
    {
        UIManager.Instance.CloseAllPopups();
        SceneManager.LoadScene("LoginScene");
    }

    void OnDestroy()
    {
        APIClient.OnTokenExpired -= OnTokenExpired;
    }
}
