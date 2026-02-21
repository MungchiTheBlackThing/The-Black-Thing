using UnityEngine.Localization.Settings;

public static class PushText
{
    const string TABLE = "PushNotifications";

    static string Get(string key, string nickname = null)
    {
        var text = LocalizationSettings.StringDatabase.GetLocalizedString(TABLE, key);
        if (nickname != null && text.Contains("<nickname>"))
            text = text.Replace("<nickname>", nickname);
        return text;
    }

    public static (string title, string body) GetA(int ch, string nickname = null)
        => (Get($"push_title_A_ch{ch}", nickname), Get($"push_A_ch{ch}", nickname));

    public static (string title, string body) GetA6(int ch, string nickname = null)
        => (Get($"push_title_A6_ch{ch}", nickname), Get($"push_A6_ch{ch}", nickname));

    public static (string title, string body) GetB(int ch, string nickname = null)
        => (Get($"push_title_B_ch{ch}", nickname), Get($"push_B_ch{ch}", nickname));

    public static (string title, string body) GetB6(int ch, string nickname = null)
        => (Get($"push_title_B6_ch{ch}", nickname), Get($"push_B6_ch{ch}", nickname));

    public static (string title, string body) GetWatching(int ch, string nickname = null)
        => (Get($"push_title_watching_{ch}", nickname), Get($"push_watching_{ch}", nickname));

    public static (string title, string body) GetNight(string nickname = null)
    {
        int idx = UnityEngine.Random.Range(1, 8); // 1~7
        return (Get($"push_title_night_{idx}", nickname), Get($"push_night_{idx}", nickname));
    }
}