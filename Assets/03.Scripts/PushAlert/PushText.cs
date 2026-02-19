using UnityEngine.Localization.Settings;

public static class PushText
{
    const string TABLE = "PushNotifications";

    static string Get(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(TABLE, key);
    }

    public static (string title, string body) GetA(int ch)
        => (Get($"push_title_A_ch{ch}"), Get($"push_A_ch{ch}"));

    public static (string title, string body) GetA6(int ch)
        => (Get($"push_title_A6_ch{ch}"), Get($"push_A6_ch{ch}"));

    public static (string title, string body) GetB(int ch)
        => (Get($"push_title_B_ch{ch}"), Get($"push_B_ch{ch}"));

    public static (string title, string body) GetB6(int ch)
        => (Get($"push_title_B6_ch{ch}"), Get($"push_B6_ch{ch}"));

    public static (string title, string body) GetWatching(int ch)
        => (Get($"push_title_watching_{ch}"), Get($"push_watching_{ch}"));

    public static (string title, string body) GetNight()
    {
        int idx = UnityEngine.Random.Range(1, 8); // 1~7
        return (Get($"push_title_night_{idx}"), Get($"push_night_{idx}"));
    }
}