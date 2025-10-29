using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public static class FindBrokenInventoryCells
{
    [MenuItem("Tools/Muck SAFE/Report Broken InventoryCells (Open Scene)")]
    public static void ReportOpenScene()
    {
        var scn = SceneManager.GetActiveScene();
        int bad = 0;
        foreach (var root in scn.GetRootGameObjects())
        {
            foreach (var cell in root.GetComponentsInChildren<InventoryCell>(true))
            {
                var icon = GetPrivateField<Image>(cell, "icon") ?? cell.GetComponentInChildren<Image>(true);
                var amt = GetPrivateField<TMP_Text>(cell, "amountText") ?? cell.GetComponentInChildren<TMP_Text>(true);
                var btn = GetPrivateField<Button>(cell, "button") ?? cell.GetComponentInChildren<Button>(true);

                if (!icon || !amt || !btn)
                {
                    bad++;
                    Debug.LogWarning($"[Broken InventoryCell] {HierarchyPath(cell.gameObject)}  " +
                                     $"icon={(icon ? "ok" : "MISSING")}, amountText={(amt ? "ok" : "MISSING")}, button={(btn ? "ok" : "MISSING")}");
                }
            }
        }
        Debug.Log($"[Broken InventoryCell] Total with missing refs: {bad}");
    }

    static T GetPrivateField<T>(object obj, string name) where T : Object
    {
        var f = obj.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        return f != null ? (T)f.GetValue(obj) : null;
    }

    static string HierarchyPath(GameObject go)
    {
        var path = go.name;
        var t = go.transform;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }
}
