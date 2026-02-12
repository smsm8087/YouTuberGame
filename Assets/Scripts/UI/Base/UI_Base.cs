using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

/// <summary>
/// 모든 UI의 기반 클래스
/// Enum 기반 바인딩으로 SerializeField 없이 UI 요소 참조
///
/// 사용법:
///   enum Buttons { PlayBtn, QuitBtn }
///   enum Texts { TitleText }
///
///   FirstSetting() {
///       Bind<Button>(typeof(Buttons));
///       Bind<TextMeshProUGUI>(typeof(Texts));
///   }
///
///   // 접근: GetButton(Buttons.PlayBtn)
/// </summary>
public abstract class UI_Base : MonoBehaviour
{
    protected Dictionary<Type, Object[]> _objects = new();
    protected bool _isSetting = false;

    public virtual void Init()
    {
        if (!_isSetting)
            FirstSetting();
    }

    protected virtual void FirstSetting()
    {
        _isSetting = true;
    }

    protected void Bind<T>(Type enumType) where T : Object
    {
        string[] names = Enum.GetNames(enumType);
        Object[] objects = new Object[names.Length];
        _objects[typeof(T)] = objects;

        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
            {
                objects[i] = Util.FindChild(gameObject, names[i], true);
            }
            else
            {
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);
            }

            if (objects[i] == null)
                Debug.LogWarning($"[UI_Base] Bind failed: {names[i]} ({typeof(T).Name}) not found in {gameObject.name}");
        }
    }

    protected T Get<T>(int idx) where T : Object
    {
        if (!_objects.TryGetValue(typeof(T), out Object[] objects))
            return null;
        if (idx < 0 || idx >= objects.Length)
            return null;
        return objects[idx] as T;
    }

    // 편의 메서드
    protected GameObject GetObject(Enum idx) => Get<GameObject>(Convert.ToInt32(idx));
    protected Button GetButton(Enum idx) => Get<Button>(Convert.ToInt32(idx));
    protected Image GetImage(Enum idx) => Get<Image>(Convert.ToInt32(idx));
    protected TextMeshProUGUI GetText(Enum idx) => Get<TextMeshProUGUI>(Convert.ToInt32(idx));
    protected Slider GetSlider(Enum idx) => Get<Slider>(Convert.ToInt32(idx));
    protected TMP_InputField GetInputField(Enum idx) => Get<TMP_InputField>(Convert.ToInt32(idx));
    protected Toggle GetToggle(Enum idx) => Get<Toggle>(Convert.ToInt32(idx));
}

/// <summary>
/// 버튼 이벤트 헬퍼
/// </summary>
public static class UI_ButtonExtension
{
    public static void AddButtonEvent(this Button button, Action action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => action?.Invoke());
    }
}
