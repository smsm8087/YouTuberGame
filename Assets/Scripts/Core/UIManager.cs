using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 매니저 - 팝업 스택 관리, 정렬, 생성/소멸
///
/// 사용법:
///   UIManager.Instance.ShowPopup<GachaPopup>("GachaPopup");
///   UIManager.Instance.CloseTopPopup();
/// </summary>
public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[UIManager]");
                _instance = go.AddComponent<UIManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private int _sortingOrder = 10;
    private readonly List<UI_Popup> _popupStack = new();
    private Transform _root;

    public Transform Root
    {
        get
        {
            if (_root == null)
            {
                var go = GameObject.Find("@UI_Root");
                if (go == null)
                {
                    go = new GameObject("@UI_Root");
                    DontDestroyOnLoad(go);
                }
                _root = go.transform;
            }
            return _root;
        }
    }

    /// <summary>
    /// 팝업 표시 (Resources에서 프리팹 로드)
    /// </summary>
    public T ShowPopup<T>(string prefabName = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(prefabName))
            prefabName = typeof(T).Name;

        GameObject prefab = Resources.Load<GameObject>($"UI/{prefabName}");
        if (prefab == null)
        {
            Debug.LogError($"[UIManager] Prefab not found: UI/{prefabName}");
            return null;
        }

        GameObject go = Instantiate(prefab, Root);
        T popup = go.GetComponent<T>();
        if (popup == null)
        {
            Debug.LogError($"[UIManager] Component {typeof(T).Name} not found on {prefabName}");
            Destroy(go);
            return null;
        }

        // 캔버스 및 정렬 설정
        SetupCanvas(go);

        _popupStack.Add(popup);
        popup.Init();

        GameObserver.Emit(ObserverEvent.PopupOpened);
        return popup;
    }

    /// <summary>
    /// 특정 팝업 닫기
    /// </summary>
    public void ClosePopup(UI_Popup popup)
    {
        if (!_popupStack.Contains(popup)) return;

        popup.OnClose();
        _popupStack.Remove(popup);
        Destroy(popup.gameObject);
        _sortingOrder--;

        GameObserver.Emit(ObserverEvent.PopupClosed);
    }

    /// <summary>
    /// 최상단 팝업 닫기
    /// </summary>
    public void CloseTopPopup()
    {
        if (_popupStack.Count == 0) return;
        var popup = _popupStack[_popupStack.Count - 1];
        ClosePopup(popup);
    }

    /// <summary>
    /// 모든 팝업 닫기
    /// </summary>
    public void CloseAllPopups()
    {
        for (int i = _popupStack.Count - 1; i >= 0; i--)
        {
            var popup = _popupStack[i];
            popup.OnClose();
            Destroy(popup.gameObject);
        }
        _popupStack.Clear();
        _sortingOrder = 10;
    }

    /// <summary>
    /// 팝업 표시 중인지 확인
    /// </summary>
    public bool IsShowPopup() => _popupStack.Count > 0;
    public bool IsShowPopup<T>() where T : UI_Popup => _popupStack.Exists(p => p is T);

    public T GetPopup<T>() where T : UI_Popup
    {
        return _popupStack.Find(p => p is T) as T;
    }

    /// <summary>
    /// 서브 아이템 생성 (리스트 아이템 등)
    /// </summary>
    public T MakeSubItem<T>(Transform parent, string prefabName = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(prefabName))
            prefabName = typeof(T).Name;

        GameObject prefab = Resources.Load<GameObject>($"UI/{prefabName}");
        if (prefab == null)
        {
            Debug.LogError($"[UIManager] SubItem prefab not found: UI/{prefabName}");
            return null;
        }

        GameObject go = Instantiate(prefab, parent);
        return Util.GetOrAddComponent<T>(go);
    }

    void SetupCanvas(GameObject go)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = _sortingOrder++;
        canvas.overrideSorting = true;

        Util.GetOrAddComponent<CanvasScaler>(go);
        Util.GetOrAddComponent<GraphicRaycaster>(go);
    }
}
