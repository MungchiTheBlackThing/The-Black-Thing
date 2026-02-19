using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif

#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

public static class NotificationService
{

    const string KEY_PERMISSION_REQUESTED = "PushPermissionRequestedOnce";

    const string ANDROID_CHANNEL_ID = "default_channel";
    static readonly TimeSpan MinLeadTime = TimeSpan.FromSeconds(2);

    static Runner _runner;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    static readonly HashSet<int> _scheduledIds = new HashSet<int>(); // 디버그용(세션 내)
#endif

    // ----------------------------
    // Init / Permission
    // ----------------------------
    public static void Init()
    {
        EnsureRunner();

#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel
        {
            Id = ANDROID_CHANNEL_ID,
            Name = "Default",
            Importance = Importance.Default,
            Description = "Game notifications"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
        SyncPermissionState();
    }

    // 외부 조회용(원하면 강제 Sync)
    public static PushPermissionState GetPermissionState(bool forceSync = true)
    {
        if (forceSync) SyncPermissionState();
        return PushPermission.State;
    }

    // 서비스가 실행까지 책임
    public static void RequestPermissionIfNeeded(Action<PushPermissionState> onDone)
    {
        EnsureRunner();
        _runner.StartCoroutine(RequestPermissionRoutine(onDone));
    }

    // ----------------------------
    // Schedule (type+chapter only)
    // ----------------------------

    // 상대시간
    public static bool ScheduleAfterSeconds(
        PushIdType type, int chapter,
        string title, string body, double seconds,
        bool showInForeground = false)
    {
        if (PushPermission.State != PushPermissionState.Granted) return false;
        if (seconds < MinLeadTime.TotalSeconds) return false;

        int id = PushIds.Make(type, chapter);

#if UNITY_ANDROID
        var notif = new AndroidNotification
        {
            Title = title,
            Text = body,
            FireTime = DateTime.Now.AddSeconds(seconds)
        };
        AndroidNotificationCenter.SendNotificationWithExplicitID(notif, ANDROID_CHANNEL_ID, id);
        TrackId(id);
        return true;

#elif UNITY_IOS
        var trigger = new iOSNotificationTimeIntervalTrigger
        {
            TimeInterval = TimeSpan.FromSeconds(seconds),
            Repeats = false
        };

        var notif = new iOSNotification
        {
            Identifier = id.ToString(),
            Title = title,
            Body = body,
            ShowInForeground = showInForeground,
            ForegroundPresentationOption = showInForeground
                ? (PresentationOption.Alert | PresentationOption.Sound)
                : PresentationOption.None,
            Trigger = trigger
        };

        iOSNotificationCenter.ScheduleNotification(notif);
        TrackId(id);
        return true;
#else
        return false;
#endif
    }

    // 절대시간
    public static bool ScheduleAtLocalTime(
        PushIdType type, int chapter,
        string title, string body, DateTime fireTimeLocal,
        bool showInForeground = false)
    {
        if (PushPermission.State != PushPermissionState.Granted) return false;

        var now = DateTime.Now;
        if (fireTimeLocal <= now + MinLeadTime) return false;

        int id = PushIds.Make(type, chapter);

#if UNITY_ANDROID
        var notif = new AndroidNotification
        {
            Title = title,
            Text = body,
            FireTime = fireTimeLocal
        };
        AndroidNotificationCenter.SendNotificationWithExplicitID(notif, ANDROID_CHANNEL_ID, id);
        TrackId(id);
        return true;

#elif UNITY_IOS
        var trigger = new iOSNotificationCalendarTrigger
        {
            Year = fireTimeLocal.Year,
            Month = fireTimeLocal.Month,
            Day = fireTimeLocal.Day,
            Hour = fireTimeLocal.Hour,
            Minute = fireTimeLocal.Minute,
            Second = fireTimeLocal.Second,
            Repeats = false
        };

        var notif = new iOSNotification
        {
            Identifier = id.ToString(),
            Title = title,
            Body = body,
            ShowInForeground = showInForeground,
            ForegroundPresentationOption = showInForeground
                ? (PresentationOption.Alert | PresentationOption.Sound)
                : PresentationOption.None,
            Trigger = trigger
        };

        iOSNotificationCenter.ScheduleNotification(notif);
        TrackId(id);
        return true;
#else
        return false;
#endif
    }

    // ----------------------------
    // Cancel (type+chapter only)
    // ----------------------------
    public static void Cancel(PushIdType type, int chapter)
    {
        int id = PushIds.Make(type, chapter);
        CancelById(id);
    }

    // Missing 같은 챕터 비의존 취소용
    public static void CancelGlobal(PushIdType type)
    {
        int id = PushIds.Make(type, 0);
        CancelById(id);
    }

    static void CancelById(int id)
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelNotification(id);
#elif UNITY_IOS
        iOSNotificationCenter.RemoveScheduledNotification(id.ToString());
#endif
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        _scheduledIds.Remove(id);
#endif
    }

    public static void CancelAll()
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllScheduledNotifications();
        AndroidNotificationCenter.CancelAllDisplayedNotifications();
#elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
        iOSNotificationCenter.RemoveAllDeliveredNotifications();
#endif
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        _scheduledIds.Clear();
#endif
    }

    // ----------------------------
    // Settings jump
    // ----------------------------
    public static void OpenAppNotificationSettings()
    {
#if UNITY_IOS
        iOSNotificationCenter.OpenNotificationSettings();
#elif UNITY_ANDROID
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intent = new AndroidJavaObject("android.content.Intent"))
            {
                var uriClass = new AndroidJavaClass("android.net.Uri");
                var uri = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", Application.identifier, null);

                intent.Call<AndroidJavaObject>("setAction", "android.settings.APPLICATION_DETAILS_SETTINGS");
                intent.Call<AndroidJavaObject>("setData", uri);
                currentActivity.Call("startActivity", intent);
            }
        }
        catch { }
#endif
    }

    // ----------------------------
    // Internals
    // ----------------------------
    static void SyncPermissionState()
    {
#if UNITY_IOS
        var settings = iOSNotificationCenter.GetNotificationSettings();
        switch (settings.AuthorizationStatus)
        {
            case AuthorizationStatus.Authorized:
            case AuthorizationStatus.Provisional:
                PushPermission.State = PushPermissionState.Granted;
                break;
            case AuthorizationStatus.Denied:
                PushPermission.State = PushPermissionState.Denied;
                break;
            default:
                PushPermission.State = PushPermissionState.Unknown;
                break;
        }

#elif UNITY_ANDROID
        if (GetAndroidApiLevel() >= 33)
        {
            var status = AndroidNotificationCenter.UserPermissionToPost;
            switch (status)
            {
                case PermissionStatus.Allowed:
                    PushPermission.State = PushPermissionState.Granted;
                    break;
                case PermissionStatus.Denied:
                case PermissionStatus.NotificationsBlockedForApp:
                    PushPermission.State = PushPermissionState.Denied;
                    break;
                default:
                    PushPermission.State = PushPermissionState.Unknown;
                    break;
            }
        }
        else
        {
            PushPermission.State = PushPermissionState.Granted;
        }
#else
        PushPermission.State = PushPermissionState.Granted;
#endif
    }

    static IEnumerator RequestPermissionRoutine(Action<PushPermissionState> onDone)
    {
        SyncPermissionState();

        if (PushPermission.State == PushPermissionState.Granted)
        {
            onDone?.Invoke(PushPermissionState.Granted);
            yield break;
        }

        if (PushPermission.State == PushPermissionState.Denied)
        {
            onDone?.Invoke(PushPermissionState.Denied);
            yield break;
        }

        PlayerPrefs.SetInt(KEY_PERMISSION_REQUESTED, 1);
        PlayerPrefs.Save();

#if UNITY_IOS
        var req = new AuthorizationRequest(
            AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound, true);

        while (!req.IsFinished) yield return null;

        PushPermission.State = req.Granted ? PushPermissionState.Granted : PushPermissionState.Denied;
        onDone?.Invoke(PushPermission.State);

#elif UNITY_ANDROID
        if (GetAndroidApiLevel() >= 33)
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            yield return new WaitUntil(() => Application.isFocused);
            yield return null; // 한 프레임 더 대기

            SyncPermissionState();
            onDone?.Invoke(PushPermission.State);
        }
        else
        {
            PushPermission.State = PushPermissionState.Granted;
            onDone?.Invoke(PushPermissionState.Granted);
        }
#else
        PushPermission.State = PushPermissionState.Granted;
        onDone?.Invoke(PushPermissionState.Granted);
        yield break;
    
#endif
    }


    public static bool HasRequestedPermissionOnce()
    {
        return PlayerPrefs.GetInt(KEY_PERMISSION_REQUESTED, 0) == 1;
    }

    static void EnsureRunner()
    {
        if (_runner != null) return;

        var go = new GameObject("[NotificationService]");
        UnityEngine.Object.DontDestroyOnLoad(go);
        _runner = go.AddComponent<Runner>();
    }

    static void TrackId(int id)
    {

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_scheduledIds.Contains(id))
            Debug.LogWarning($"[NotificationService] Notification ID reused (overwrite): {id}");
        _scheduledIds.Add(id);
#endif
    }

    

#if UNITY_ANDROID
    static int GetAndroidApiLevel()
    {
        try
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                return version.GetStatic<int>("SDK_INT");
        }
        catch { return 0; }
    }
#endif

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static void ResetPermissionRequestedFlag()
    {
        PlayerPrefs.DeleteKey(KEY_PERMISSION_REQUESTED);
        PlayerPrefs.Save();
    }
#endif

    sealed class Runner : MonoBehaviour
    {
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) return;

            var prev = PushPermission.State;
            SyncPermissionState();

            if (prev != PushPermissionState.Granted
                && PushPermission.State == PushPermissionState.Granted
                && PlayerPrefs.GetInt("PushEnabled", 0) == 0)
            {
                PlayerPrefs.SetInt("PushEnabled", 1);
                PlayerPrefs.Save();
                var gm = UnityEngine.Object.FindObjectOfType<GameManager>();
                if (gm != null) PushScheduler.ScheduleForCurrentPhase(gm);
            }
        }

        void OnApplicationPause(bool paused)
        {
            if (!paused) SyncPermissionState();
        }
    }
}