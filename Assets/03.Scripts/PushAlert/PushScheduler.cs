using System;
using UnityEngine;

public static class PushScheduler
{
    const double MIN_DELAY_SEC = 5.0;
    const double REMINDER_6H = 6 * 3600.0;

    public static void ScheduleForCurrentPhase(GameManager gm)
    {
        if (PlayerPrefs.GetInt("PushEnabled", 0) != 1) return;
        if (PushPermission.State != PushPermissionState.Granted) return;

        int chapter = gm.Chapter;
        GamePatternState phase = gm.Pattern;
        double remaining = gm.GetPhaseRemainingSeconds();
        double t = Math.Max(remaining, MIN_DELAY_SEC);

        switch (phase)
        {
            case GamePatternState.Watching:
                ScheduleA(chapter, t);
                break;

            case GamePatternState.Thinking:
                ScheduleB(chapter, t);
                break;

            case GamePatternState.Writing:
                ScheduleNight(t);
                break;

            case GamePatternState.Sleeping:
                ScheduleWatching(chapter + 1, gm.dayStartHour, gm.dayStartMinute);
                break;

            default:
                break;
        }
    }

    static void ScheduleWatching(int chapter, int hour, int minute)
    {
        if (chapter > 14) return; // 마지막 챕터 이후엔 예약 안 함
        var (title, body) = PushText.GetWatching(chapter);

        var now = DateTime.Now;
        var fireTime = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

        // 오늘 시각이 이미 지났으면 내일로
        if (fireTime <= now)
            fireTime = fireTime.AddDays(1);

        NotificationService.Cancel(PushIdType.Watching, chapter);
        NotificationService.ScheduleAtLocalTime(PushIdType.Watching, chapter, title, body, fireTime);
    }

    static void ScheduleA(int chapter, double t)
    {
        var (title, body) = PushText.GetA(chapter);
        var (title6, body6) = PushText.GetA6(chapter);

        NotificationService.Cancel(PushIdType.A, chapter);
        NotificationService.Cancel(PushIdType.A6, chapter);

        NotificationService.ScheduleAfterSeconds(PushIdType.A, chapter, title, body, t);
        NotificationService.ScheduleAfterSeconds(PushIdType.A6, chapter, title6, body6, t + REMINDER_6H);
    }

    static void ScheduleB(int chapter, double t)
    {
        var (title, body) = PushText.GetB(chapter);
        var (title6, body6) = PushText.GetB6(chapter);

        NotificationService.Cancel(PushIdType.B, chapter);
        NotificationService.Cancel(PushIdType.B6, chapter);

        NotificationService.ScheduleAfterSeconds(PushIdType.B, chapter, title, body, t);
        NotificationService.ScheduleAfterSeconds(PushIdType.B6, chapter, title6, body6, t + REMINDER_6H);
    }

    static void ScheduleNight(double t)
    {
        var (title, body) = PushText.GetNight();

        NotificationService.CancelGlobal(PushIdType.Night);
        NotificationService.ScheduleAfterSeconds(PushIdType.Night, 0, title, body, t);
    }
}