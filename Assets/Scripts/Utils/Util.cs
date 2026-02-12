using UnityEngine;

/// <summary>
/// 유틸리티 - 자식 찾기, 컴포넌트 추가 등
/// </summary>
public static class Util
{
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : Object
    {
        if (go == null) return null;

        if (recursive)
        {
            foreach (T component in go.GetComponentsInChildren<T>(true))
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }
        else
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform child = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || child.name == name)
                {
                    T component = child.GetComponent<T>();
                    if (component != null) return component;
                }
            }
        }

        return null;
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null) return null;
        return transform.gameObject;
    }

    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static string FormatNumber(long number)
    {
        if (number >= 1_000_000) return $"{number / 1_000_000f:F1}M";
        if (number >= 1_000) return $"{number / 1_000f:F1}K";
        return number.ToString();
    }
}

/// <summary>
/// Transform 확장 메서드
/// </summary>
public static class TransformExtensions
{
    public static GameObject FindRecursive(this GameObject go, string name)
    {
        return Util.FindChild(go, name, true);
    }
}
