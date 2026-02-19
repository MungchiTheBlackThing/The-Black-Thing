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

    private static readonly object _ioLock = new object();

    public static void WriteAllTextAtomic(string path, string contents)
    {
        lock (_ioLock)
        {
            string tmp = path + ".tmp";
            string bak = path + ".bak";

            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                // (옵션) 이전 tmp가 남아있으면 정리
                try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }

                // 1) tmp에 먼저 완전 쓰기
                File.WriteAllText(tmp, contents);

                // 2) 기존 파일이 있으면 bak로 백업(원본 보존)
                if (File.Exists(path))
                {
                    try
                    {
                        if (File.Exists(bak)) File.Delete(bak);
                        File.Move(path, bak);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[SavePaths] Backup move failed: {e.Message}");
                        // 여기서 path가 남아있을 수 있음
                    }
                }

                // 3) tmp를 본 파일로 반영
                // path가 아직 남아있으면(백업 실패 등) 덮어쓰기 가능한 방식으로 처리
                if (File.Exists(path))
                {
                    // 최후 안전: 기존 path를 한번 더 bak로 치우거나 삭제
                    try
                    {
                        if (File.Exists(bak)) File.Delete(bak);
                        File.Move(path, bak);
                    }
                    catch
                    {
                        try { File.Delete(path); } catch { }
                    }
                }

                File.Move(tmp, path);

                // 4) tmp 정리(정상적이면 없어야 하지만, 안전)
                try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SavePaths] WriteAllTextAtomic failed: {e.Message}");
                // 실패 시 tmp/bak로 복구 가능. tmp는 남겨둬도 됨(복구용).
            }
        }
    }

    /// <summary>
    /// path 읽기 실패 시 bak, tmp 순으로 복구 시도
    /// </summary>
    public static bool TryReadAllTextWithBackup(string path, out string text)
    {
        lock (_ioLock)
        {
            // 1) main
            if (TryReadAllText(path, out text) && !string.IsNullOrEmpty(text))
                return true;

            // 2) bak
            string bak = path + ".bak";
            if (TryReadAllText(bak, out text) && !string.IsNullOrEmpty(text))
                return true;

            // 3) tmp (마지막 수단)
            string tmp = path + ".tmp";
            if (TryReadAllText(tmp, out text) && !string.IsNullOrEmpty(text))
                return true;

            text = null;
            return false;
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
