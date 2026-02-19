using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

public static class PushNudgeController
{
    const string KEY_SHOWN_UNKNOWN = "PushNudgeShownUnknownOnce";
    const string KEY_LAST_SHOWN_UTC = "PushNudgeLastShownUtc";
    const string KEY_PUSH_ENABLED = "PushEnabled";
    static readonly TimeSpan CooldownDenied = TimeSpan.FromDays(4);

    static PushNudgePopup _unknownPopup;
    static PushNudgePopup _deniedPopup;

    public static void RegisterPopups(PushNudgePopup unknownPopup, PushNudgePopup deniedPopup)
    {
        _unknownPopup = unknownPopup;
        _deniedPopup = deniedPopup;
    }

    public static void TryShow(GameManager gm)
    {
        if (PlayerPrefs.GetInt(KEY_PUSH_ENABLED, 0) == 1) return;

        var perm = NotificationService.GetPermissionState(forceSync: true);

        if (perm == PushPermissionState.Granted)
        {
            PlayerPrefs.SetInt(KEY_PUSH_ENABLED, 1);
            PlayerPrefs.Save();
            PushScheduler.ScheduleForCurrentPhase(gm);
            return;
        }

        if (perm == PushPermissionState.Unknown)
        {
            if (PlayerPrefs.GetInt(KEY_SHOWN_UNKNOWN, 0) == 1) return;
            PlayerPrefs.SetInt(KEY_SHOWN_UNKNOWN, 1);
            PlayerPrefs.Save();
            ShowUnknownNudge(gm);
            return;
        }

        if (perm == PushPermissionState.Denied)
        {
            if (!IsDeniedCooldownOver()) return;
            UpdateLastShown();
            ShowDeniedNudge(gm);
        }
    }

    // 설정 토글 전용 - Unknown 체크 없이 무조건 팝업
    public static void TryShowFromSettings(GameManager gm, Action onGranted = null)
    {
        if (PlayerPrefs.GetInt(KEY_PUSH_ENABLED, 0) == 1) return;

        var perm = NotificationService.GetPermissionState(forceSync: true);

        if (perm == PushPermissionState.Granted)
        {
            PlayerPrefs.SetInt(KEY_PUSH_ENABLED, 1);
            PlayerPrefs.Save();
            PushScheduler.ScheduleForCurrentPhase(gm);
            onGranted?.Invoke();
            return;
        }

        if (perm == PushPermissionState.Unknown)
            ShowUnknownNudge(gm, onGranted); // 앱 내에서 결과 받을 수 있으니까 콜백 전달

        if (perm == PushPermissionState.Denied)
            ShowDeniedNudge(gm); // 설정 앱으로 넘어가니까 콜백 없음, OnApplicationFocus가 처리
    }

    static string L(string key) =>
        LocalizationSettings.StringDatabase.GetLocalizedString("SystemUIText", key);

    static void ShowUnknownNudge(GameManager gm, Action onGranted = null)
    {
        if (_unknownPopup == null)
        {
            Debug.LogWarning("[PushNudgeController] unknownPopup not registered");
            return;
        }

        _unknownPopup.Show(
            text: L("push_unknown"),
            confirmLabel: L("push_unknown_yes"),
            cancelLabel: L("push_unknown_no"),
            onConfirm: () =>
            {
                NotificationService.RequestPermissionIfNeeded(state =>
                {
                    if (state == PushPermissionState.Granted)
                    {
                        PlayerPrefs.SetInt(KEY_PUSH_ENABLED, 1);
                        PlayerPrefs.Save();
                        PushScheduler.ScheduleForCurrentPhase(gm);
                        onGranted?.Invoke();
                    }
                    else
                    {
                        PlayerPrefs.SetInt(KEY_PUSH_ENABLED, 0);
                        PlayerPrefs.Save();
                    }
                });
            },
            onCancel: () => { }
        );
    }

    static void ShowDeniedNudge(GameManager gm)
    {
        if (_deniedPopup == null)
        {
            Debug.LogWarning("[PushNudgeController] deniedPopup not registered");
            return;
        }

        _deniedPopup.Show(
            text: L("push_denied"),
            confirmLabel: L("push_denied_yes"),
            cancelLabel: L("push_denied_no"),
            onConfirm: () => NotificationService.OpenAppNotificationSettings(),
            onCancel: () => { }
        );
    }

    static bool IsDeniedCooldownOver()
    {
        string saved = PlayerPrefs.GetString(KEY_LAST_SHOWN_UTC, "");
        if (string.IsNullOrEmpty(saved)) return true;
        var last = DateTime.FromBinary(Convert.ToInt64(saved));
        return (DateTime.UtcNow - last) >= CooldownDenied;
    }

    static void UpdateLastShown()
    {
        PlayerPrefs.SetString(KEY_LAST_SHOWN_UTC, DateTime.UtcNow.ToBinary().ToString());
        PlayerPrefs.Save();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(KEY_SHOWN_UNKNOWN);
        PlayerPrefs.DeleteKey(KEY_LAST_SHOWN_UTC);
        PlayerPrefs.DeleteKey(KEY_PUSH_ENABLED);
        NotificationService.ResetPermissionRequestedFlag();
        PlayerPrefs.Save();
    }
#endif
}