using System.IO;
using UnityEngine;

[System.Serializable]
public class RecentData
{
    public int isContinue = 0; // 0: ó������, 1: �̾
    public string objectName;
    public int value;
    public int index;
    public int tutonum = 0;
}

public static class RecentManager
{
    private static string fileName = "recent.json";
    private static string FilePath => Path.Combine(Application.persistentDataPath, fileName);

    public static void Save(GameObject obj, int value, int index)
    {
        RecentData data = Load() ?? new RecentData(); // ���� ������ ����

        data.isContinue = 1;
        data.objectName = obj.name;
        data.value = value;
        data.index = index;
        // ������ data.tutonum ���� ������

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(FilePath, json);
    }

    public static RecentData Load()
    {
        if (!File.Exists(FilePath))
        {
            // ������ ������ �⺻������ ����
            RecentData defaultData = new RecentData
            {
                isContinue = 0,
                objectName = "",
                value = 0,
                index = 0,
                tutonum = 0 // tutonum �ʵ� �߰��Ǿ��ٰ� ����
            };

            string json = JsonUtility.ToJson(defaultData);
            File.WriteAllText(FilePath, json);
            return defaultData;
        }

        string existingJson = File.ReadAllText(FilePath);
        return JsonUtility.FromJson<RecentData>(existingJson);
    }

    public static void ResetFlagOnly()
    {
        if (!File.Exists(FilePath))
            return;

        string json = File.ReadAllText(FilePath);
        RecentData data = JsonUtility.FromJson<RecentData>(json);
        data.isContinue = 0;
        data.tutonum = 0;
        File.WriteAllText(FilePath, JsonUtility.ToJson(data));
    }

    public static bool Exists()
    {
        return File.Exists(FilePath);
    }

    public static void RecentSave()
    {
        if (!File.Exists(FilePath))
            return;

        string json = File.ReadAllText(FilePath);
        RecentData data = JsonUtility.FromJson<RecentData>(json);
        File.WriteAllText(FilePath, JsonUtility.ToJson(data));
    }
    public static void TutoNumChange()
    {
        if (!File.Exists(FilePath))
            return;

        string json = File.ReadAllText(FilePath);
        RecentData data = JsonUtility.FromJson<RecentData>(json);
        data.tutonum = 1;
        File.WriteAllText(FilePath, JsonUtility.ToJson(data));
    }
}