using System.IO;
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
	private static string fileName = "recent.json";

	private static string FilePath
	{
		get
		{
#if UNITY_EDITOR
			// 에디터: Assets/recent.json (playerdata.json처럼 프로젝트에서 직접 수정하면 즉시 반영)
			return Path.Combine(Application.dataPath, fileName);
#else
			// 빌드(Android/iOS/PC): 쓰기 가능한 위치
			return Path.Combine(Application.persistentDataPath, fileName);
#endif
		}
	}

	public static void Save(GameObject obj, int value, int index)
	{
		RecentData data = Load() ?? new RecentData();

		data.isContinue = 1;
		data.objectName = obj != null ? obj.name : "";
		data.value = value;
		data.index = index;

		File.WriteAllText(FilePath, JsonUtility.ToJson(data));
	}

	public static RecentData Load()
	{
		if (!File.Exists(FilePath))
		{
			RecentData defaultData = new RecentData
			{
				isContinue = 0,
				objectName = "",
				value = 0,
				index = 0,
				tutonum = 0,
				tutoend = false,
				watching = false
			};

			File.WriteAllText(FilePath, JsonUtility.ToJson(defaultData));
			return defaultData;
		}

		string json = File.ReadAllText(FilePath);
		if (string.IsNullOrEmpty(json))
			return new RecentData();

		return JsonUtility.FromJson<RecentData>(json);
	}

	public static void ResetFlagOnly()
	{
		if (!File.Exists(FilePath))
			return;

		string json = File.ReadAllText(FilePath);
		RecentData data = JsonUtility.FromJson<RecentData>(json);

		data.isContinue = 0;
		data.tutonum = 0;
		data.index = 0;
		data.tutoend = false;
		data.watching = false;

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

	public static void tutoSceneEnd()
	{
		if (!File.Exists(FilePath))
			return;

		string json = File.ReadAllText(FilePath);
		RecentData data = JsonUtility.FromJson<RecentData>(json);

		data.tutoend = true;
		File.WriteAllText(FilePath, JsonUtility.ToJson(data));
	}
}
