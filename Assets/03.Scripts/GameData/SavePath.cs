using System;
using System.IO;
using UnityEngine;

public static class SavePaths
{
    public const string PlayerDataFile = "PlayerData.json";
    public const string RecentFile     = "recent.json";

#if UNITY_EDITOR
    private static string EditorSaveRoot => Path.Combine(Application.dataPath, "_LocalSaves");
#endif

    public static string PlayerDataPath => BuildPath(PlayerDataFile);
    public static string RecentPath     => BuildPath(RecentFile);

    private static string BuildPath(string filename)
    {
        try
        {
#if UNITY_EDITOR
            Directory.CreateDirectory(EditorSaveRoot);
            return Path.Combine(EditorSaveRoot, filename);
#else
            Directory.CreateDirectory(Application.persistentDataPath);
            return Path.Combine(Application.persistentDataPath, filename);
#endif
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SavePaths] BuildPath failed: {e.Message}");
            // 최후 fallback (그래도 앱은 살리기)
            return Path.Combine(Application.persistentDataPath, filename);
        }
    }

#if UNITY_IOS && !UNITY_EDITOR
    private static string LegacyIOSDocumentsPath(string filename)
    {
        string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
        path = path.Substring(0, path.LastIndexOf('/'));
        return Path.Combine(path, "Documents", filename);
    }

    public static void MigrateLegacyPlayerDataIfNeeded()
    {
        string newPath = Path.Combine(Application.persistentDataPath, PlayerDataFile);
        if (File.Exists(newPath)) return;

        try
        {
            string oldPath = LegacyIOSDocumentsPath(PlayerDataFile);
            if (File.Exists(oldPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                File.Copy(oldPath, newPath);
                Debug.Log($"[SavePaths] Migrated PlayerData: {oldPath} -> {newPath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SavePaths] Migration failed: {e.Message}");
        }
    }
#else
    public static void MigrateLegacyPlayerDataIfNeeded() { }
#endif

    public static void WriteAllTextAtomic(string path, string contents)
    {
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            string tmp = path + ".tmp";
            File.WriteAllText(tmp, contents);

            // 가능하면 Replace가 더 안전
            if (File.Exists(path))
            {
                try
                {
                    File.Replace(tmp, path, null);
                }
                catch
                {
                    File.Delete(path);
                    File.Move(tmp, path);
                }
            }
            else
            {
                File.Move(tmp, path);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SavePaths] WriteAllTextAtomic failed: {e.Message}");
        }
    }

    public static bool TryReadAllText(string path, out string text)
    {
        try
        {
            if (File.Exists(path))
            {
                text = File.ReadAllText(path);
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SavePaths] TryReadAllText failed: {e.Message}");
        }

        text = null;
        return false;
    }

    public static bool Exists(string path)
    {
        try { return File.Exists(path); }
        catch { return false; }
    }
}
