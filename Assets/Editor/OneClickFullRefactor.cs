using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System;

public class OneClickFullRefactor : EditorWindow
{
    private MonoScript targetScript;

    [MenuItem("Tools/🔥 One Click Script Refactor - Random")]
    public static void ShowWindow()
    {
        GetWindow<OneClickFullRefactor>("One-Click Refactor");
    }

    private void OnGUI()
    {
        GUILayout.Label("🧠 Đổi tên script random + cập nhật toàn bộ", EditorStyles.boldLabel);
        targetScript = (MonoScript)EditorGUILayout.ObjectField("Script Gốc", targetScript, typeof(MonoScript), false);

        GUI.enabled = targetScript != null;
        if (GUILayout.Button("🚀 One Click Refactor"))
        {
            StartRefactor(targetScript);
        }
        GUI.enabled = true;
    }

    private void StartRefactor(MonoScript script)
    {
        string oldClass = script.name;
        string oldPath = AssetDatabase.GetAssetPath(script);
        string content = File.ReadAllText(oldPath);

        if (!content.Contains("class " + oldClass))
        {
            Debug.LogError("❌ Không tìm thấy class trùng tên script.");
            return;
        }

        string newClass = GenerateRandomName("Script_", 6);
        string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newClass + ".cs");

        content = content.Replace("class " + oldClass, "class " + newClass);
        File.WriteAllText(newPath, content);
        AssetDatabase.DeleteAsset(oldPath);
        AssetDatabase.Refresh();

        Debug.Log($"✅ Đổi tên script: {oldClass} → {newClass}");

        EditorApplication.delayCall += () =>
        {
            MonoScript newScript = AssetDatabase.LoadAssetAtPath<MonoScript>(newPath);
            System.Type newType = newScript?.GetClass();

            if (newType == null)
            {
                Debug.LogError("❌ Không thể load type mới. Có thể đang compile.");
                return;
            }

            ReplaceInScene(oldClass, newClass, newType);
            ReplaceInPrefabs(oldClass, newClass, newType);

            Debug.Log("🎉 Refactor hoàn tất.");
        };
    }

    private string GenerateRandomName(string prefix, int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        string randomStr = new string(Enumerable.Repeat(chars, length)
            .Select(s => s[UnityEngine.Random.Range(0, s.Length)]).ToArray());
        return prefix + randomStr;
    }

    private void ReplaceInScene(string oldClass, string newClass, Type newType)
    {
        int changed = 0;
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.hideFlags != HideFlags.None || string.IsNullOrEmpty(go.scene.name))
                continue;

            var comps = go.GetComponents<MonoBehaviour>();
            foreach (var c in comps)
            {
                if (c == null) continue;
                if (c.GetType().Name == oldClass)
                {
                    DestroyImmediate(c, true);
                    go.AddComponent(newType);
                    changed++;

                    if (go.name == oldClass)
                        go.name = newClass;
                }
            }
        }

        if (changed > 0)
        {
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log($"🎯 Đã cập nhật {changed} GameObject trong scene.");
        }
    }

    private void ReplaceInPrefabs(string oldClass, string newClass, Type newType)
    {
        int changed = 0;
        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            bool updated = false;

            var comps = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var c in comps)
            {
                if (c == null) continue;
                if (c.GetType().Name == oldClass)
                {
                    DestroyImmediate(c, true);
                    prefab.AddComponent(newType);
                    updated = true;

                    if (prefab.name == oldClass)
                        prefab.name = newClass;
                }
            }

            if (updated)
            {
                PrefabUtility.SavePrefabAsset(prefab);
                changed++;
            }
        }

        Debug.Log($"📦 Đã cập nhật {changed} prefab.");
    }
}
