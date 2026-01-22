using UnityEngine;

[System.Serializable]
public class RecentData
{
    public int isContinue = 0; // 0: 처음부터, 1: 이어서
    public string objectName;
    public int value;
    public int index;
    public int tutonum = 0;
    public bool tutoend = false;
    public bool watching = false;
}

public static class RecentManager
{
    private static string FilePath => SavePaths.RecentPath;

    public static void Save(GameObject obj, int value, int index)
    {
        RecentData data = Load() ?? new RecentData(); // 기존 데이터 유지

        data.isContinue = 1;
        data.objectName = obj != null ? obj.name : "";
        data.value = value;
        data.index = index;
        // 기존의 data.tutonum 값을 유지함

        SavePaths.WriteAllTextAtomic(FilePath, JsonUtility.ToJson(data));
    }

    public static RecentData Load()
    {
        if (!SavePaths.TryReadAllText(FilePath, out var json) || string.IsNullOrEmpty(json))
        {
            var defaultData = new RecentData();
            SavePaths.WriteAllTextAtomic(FilePath, JsonUtility.ToJson(defaultData));
            return defaultData;
        }

        try
        {
            return JsonUtility.FromJson<RecentData>(json) ?? new RecentData();
        }
        catch
        {
            // 파일이 깨졌으면 복구
            var defaultData = new RecentData();
            SavePaths.WriteAllTextAtomic(FilePath, JsonUtility.ToJson(defaultData));
            return defaultData;
        }
    }

    public static void ResetFlagOnly()
    {
        var data = Load();
        data.isContinue = 0;
        data.tutonum = 0;
        data.index = 0;
        data.tutoend = false;
        data.watching = false;

        SavePaths.WriteAllTextAtomic(FilePath, JsonUtility.ToJson(data));
    }

    public static bool Exists()
    {
        return SavePaths.Exists(FilePath);
    }

    public static void RecentSave()
    {
        var data = Load();
        SavePaths.WriteAllTextAtomic(FilePath, JsonUtility.ToJson(data));
    }
    public static void TutoNumChange()
    {
        var data = Load();
        data.tutonum = 1;
        SavePaths.WriteAllTextAtomic(FilePath, JsonUtility.ToJson(data));
    }
    public static void tutoSceneEnd()
    {
        var data = Load();
        data.tutoend = true;
        SavePaths.WriteAllTextAtomic(FilePath, JsonUtility.ToJson(data));

    }

    public static void SetIsContinue(int value)
    {
        var data = Load();
        data.isContinue = value;
        SavePaths.WriteAllTextAtomic(FilePath, JsonUtility.ToJson(data));
    }

}