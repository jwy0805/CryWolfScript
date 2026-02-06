using UnityEngine;
using Zenject;

public class DebugProjectContextCount : MonoBehaviour
{
    void Start()
    {
        var pcs = FindObjectsOfType<ProjectContext>(true);
        Debug.Log($"[Debug] ProjectContext count: {pcs.Length}");
        foreach (var pc in pcs)
        {
            Debug.Log($"[Debug] PC: name={pc.name}, scene={pc.gameObject.scene.name}, path={GetPath(pc.transform)}");
        }
    }

    static string GetPath(Transform t)
    {
        var path = t.name;
        while (t.parent != null) { t = t.parent; path = $"{t.name}/{path}"; }
        return path;
    }
}
