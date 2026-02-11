using UnityEngine;
using Zenject;

public class DebugProjectContextCount : MonoBehaviour
{
    static string GetPath(Transform t)
    {
        var path = t.name;
        while (t.parent != null) { t = t.parent; path = $"{t.name}/{path}"; }
        return path;
    }
}
