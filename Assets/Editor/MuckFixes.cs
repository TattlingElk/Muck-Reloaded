// Assets/Editor/MuckMissingScanner.cs
// Robust scan for "Missing (Mono Script)" by detecting null component slots.
// Also provides a scene-only scan and a heuristic TMP text check.

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class MuckMissingScanner
{
    [MenuItem("Tools/Muck SAFE/Scan Project for Missing (Mono Script) - Accurate")]
    public static void ScanProjectAccurate()
    {
        var guids = new List<string>();
        guids.AddRange(AssetDatabase.FindAssets("t:Scene"));
        guids.AddRange(AssetDatabase.FindAssets("t:Prefab"));

        int total = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            total += ScanAssetForMissingScripts(path);
        }

        if (total == 0)
            Debug.Log("[Accurate Missing Scan] ✅ No missing scripts found in scenes/prefabs.");
        else
            Debug.LogWarning($"[Accurate Missing Scan] Total missing script components: {total}");
    }

    [MenuItem("Tools/Muck SAFE/Scan OPEN Scene for Missing (Mono Script)")]
    public static void ScanOpenSceneAccurate()
    {
        var scn = SceneManager.GetActiveScene();
        if (!scn.IsValid() || !scn.isLoaded)
        {
            Debug.LogWarning("[Accurate Missing Scan] No open scene to scan.");
            return;
        }
        int total = 0;
        foreach (var root in scn.GetRootGameObjects())
            total += LogMissingInHierarchy(root, scn.path);

        if (total == 0)
            Debug.Log($"[Accurate Missing Scan] ✅ No missing scripts in open scene: {scn.path}");
        else
            Debug.LogWarning($"[Accurate Missing Scan] Missing scripts in open scene: {total}");
    }

    // Optional: find objects that look like text but have NO TMP component (heuristic)
    [MenuItem("Tools/Muck SAFE/Report Likely Text Nodes WITHOUT TMP (Project)")]
    public static void ReportLikelyTextWithoutTMP()
    {
        string[] texty = { "Text", "Label", "Name", "Status", "Hit", "Damage", "Ping", "Objective", "Title", "Amount", "Value", "Count" };
        int hits = 0;

        foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                var go = t.gameObject;
                if (go.GetComponent<TMP_Text>() != null) continue;
                foreach (var key in texty)
                {
                    if (go.name.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Debug.Log($"[LikelyTextNoTMP] {path} → {GetHierarchyPath(go)}");
                        hits++;
                        break;
                    }
                }
            }
            PrefabUtility.UnloadPrefabContents(root);
        }

        if (hits == 0) Debug.Log("[LikelyTextNoTMP] ✅ None found.");
        else Debug.LogWarning($"[LikelyTextNoTMP] Found {hits} likely text nodes without TMP.");
    }

    // ---- internals ----

    static int ScanAssetForMissingScripts(string path)
    {
        int count = 0;

        if (path.EndsWith(".unity"))
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            foreach (var go in scene.GetRootGameObjects())
                count += LogMissingInHierarchy(go, path);
            // (Not saving; this is read-only scan)
        }
        else if (path.EndsWith(".prefab"))
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            count += LogMissingInHierarchy(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
        return count;
    }

    static int LogMissingInHierarchy(GameObject root, string assetPath)
    {
        int missing = 0;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            var go = t.gameObject;
            // Classic reliable pattern: iterate the Component array and check for null entries
            var comps = go.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null)
                {
                    missing++;
                    Debug.LogWarning($"[MissingScript] {assetPath} → {GetHierarchyPath(go)} (component index {i})");
                }
            }
        }
        return missing;
    }

    static string GetHierarchyPath(GameObject go)
    {
        var path = go.name;
        var tr = go.transform;
        while (tr.parent != null)
        {
            tr = tr.parent;
            path = tr.name + "/" + path;
        }
        return path;
    }
}
