using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class InputGuard
{
    public static bool WorldInputLocked = false;

    static readonly List<RaycastResult> _results = new List<RaycastResult>();
    static PointerEventData _ped;

    public static bool BlockWorldInput()
    {
        if (WorldInputLocked) return true;
        return IsPointerOverUI();
    }

    public static bool IsPointerOverUI()
    {
        var es = EventSystem.current;
        if (es == null) return false;

        // 터치/마우스 모두 현재 포지션 가져오기
        Vector2 pos;

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
            pos = Input.GetTouch(0).position;
        else
            pos = Input.mousePosition; // 에디터/예외 대비
#else
        pos = Input.mousePosition;
#endif

        if (_ped == null) _ped = new PointerEventData(es);
        _ped.Reset();
        _ped.position = pos;

        _results.Clear();
        es.RaycastAll(_ped, _results);

        return _results.Count > 0;
    }
}
