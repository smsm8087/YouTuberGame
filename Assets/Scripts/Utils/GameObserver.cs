using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이벤트 타입 정의
/// UI가 데이터 변경을 감지하고 자동 갱신하기 위한 옵저버
/// </summary>
public enum ObserverEvent
{
    // 플레이어 데이터 변경
    PlayerDataUpdated,
    CurrencyChanged,

    // 콘텐츠
    ContentStarted,
    ContentCompleted,
    ContentUploaded,

    // 캐릭터
    CharacterUpdated,
    GachaCompleted,

    // 장비
    EquipmentUpgraded,

    // UI
    PopupOpened,
    PopupClosed,
}

/// <summary>
/// 간단한 이벤트 시스템
/// 데이터 변경 → UI 자동 갱신 패턴
///
/// 사용법:
///   // 구독 (UI에서)
///   GameObserver.On(ObserverEvent.CurrencyChanged, RefreshGold);
///
///   // 발행 (Manager/API 콜백에서)
///   GameObserver.Emit(ObserverEvent.CurrencyChanged);
///
///   // 해제 (OnDestroy에서)
///   GameObserver.Off(ObserverEvent.CurrencyChanged, RefreshGold);
/// </summary>
public static class GameObserver
{
    private static readonly Dictionary<ObserverEvent, List<Action>> _listeners = new();

    public static void On(ObserverEvent eventType, Action callback)
    {
        if (!_listeners.ContainsKey(eventType))
            _listeners[eventType] = new List<Action>();

        _listeners[eventType].Add(callback);
    }

    public static void Off(ObserverEvent eventType, Action callback)
    {
        if (_listeners.TryGetValue(eventType, out var list))
            list.Remove(callback);
    }

    public static void Emit(ObserverEvent eventType)
    {
        if (!_listeners.TryGetValue(eventType, out var list)) return;

        // 역순 순회 (콜백 중 Off 호출 대비)
        for (int i = list.Count - 1; i >= 0; i--)
        {
            try
            {
                list[i]?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameObserver] Error in {eventType}: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 모든 리스너 해제 (씬 전환 시)
    /// </summary>
    public static void Clear()
    {
        _listeners.Clear();
    }
}

/// <summary>
/// MonoBehaviour 확장 - OnDestroy 시 자동 해제
/// </summary>
public static class GameObserverExtensions
{
    public static void Listen(this MonoBehaviour self, ObserverEvent eventType, Action callback)
    {
        GameObserver.On(eventType, callback);
    }

    public static void Unlisten(this MonoBehaviour self, ObserverEvent eventType, Action callback)
    {
        GameObserver.Off(eventType, callback);
    }
}
