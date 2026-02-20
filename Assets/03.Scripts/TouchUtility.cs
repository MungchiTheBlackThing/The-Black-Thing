using System;
using UnityEngine;
using UnityEngine.EventSystems;

// 터치/드래그/스와이프 관련 유틸리티
public static class TouchUtility
{
    public enum SwipeDirection
    {
        None,
        Left,   // 왼쪽으로 드래그 
        Right   // 오른쪽으로 드래그
    }

    // 드래그 방향에 따라 이전/다음페이지로 이동
    // 사용법: OnEndDrag 이벤트에서 TouchUtility.HandleHorizontalSwipe(eventData, NextPage, PrevPage);
    public static void HandleHorizontalSwipe(
        PointerEventData eventData,
        Action onSwipeLeft,
        Action onSwipeRight,
        float verticalThresholdRatio = 0.6f,
        float minSwipeScreenRatio = 0.15f)
    {
        var dir = GetHorizontalSwipeDirection(eventData, verticalThresholdRatio, minSwipeScreenRatio);
        if (dir == SwipeDirection.Left) onSwipeLeft?.Invoke();
        else if (dir == SwipeDirection.Right) onSwipeRight?.Invoke();
    }

    // PointerEventData로부터 가로 스와이프 방향을 판별
    public static SwipeDirection GetHorizontalSwipeDirection(
        PointerEventData eventData,
        float verticalThresholdRatio = 0.6f,
        float minSwipeScreenRatio = 0.15f)
    {
        Vector2 start = eventData.pressPosition;
        Vector2 end = eventData.position;

        float dx = end.x - start.x;
        float dy = end.y - start.y;

        // 1) 각도(대각선/스크롤) 필터 강화: dy가 dx의 0.6배보다 크면 취소
        if (Mathf.Abs(dy) > Mathf.Abs(dx) * verticalThresholdRatio)
            return SwipeDirection.None;

        // 2) 거리 기준을 화면 비율로: 화면 너비의 15% 미만이면 취소
        float minSwipe = Screen.width * minSwipeScreenRatio;
        if (Mathf.Abs(dx) < minSwipe)
            return SwipeDirection.None;

        if (dx < 0f) return SwipeDirection.Left;
        if (dx > 0f) return SwipeDirection.Right;
        return SwipeDirection.None;
    }

    public static bool IsDragBeyondThreshold(Vector2 start, Vector2 end, float thresholdPixels)
    {
        return Vector2.Distance(start, end) >= thresholdPixels;
    }
}
