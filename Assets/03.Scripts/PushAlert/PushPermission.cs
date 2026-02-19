using UnityEngine;

public enum PushPermissionState { Unknown = 0, Granted = 1, Denied = 2 }

public static class PushPermission
{
    public static PushPermissionState State { get; set; } = PushPermissionState.Unknown;
}
