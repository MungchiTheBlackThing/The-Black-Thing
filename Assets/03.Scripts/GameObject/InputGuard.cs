using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class InputGuard
{
    public static bool WorldInputLocked = false;

    public static bool BlockWorldInput()
    {
        if (WorldInputLocked) return true;
        return IsPointerOverUI(); // 기존 함수 재사용
    }
    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // Mobile: touch
        if (Input.touchCount > 0)
        {
            // 첫 터치 기준 (대부분 이걸로 충분)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        // PC: mouse
        return EventSystem.current.IsPointerOverGameObject();
    }
}
