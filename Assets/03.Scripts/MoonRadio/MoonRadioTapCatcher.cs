using UnityEngine;
using UnityEngine.EventSystems;

public class MoonRadioTapCatcher : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Target")]
    [SerializeField] private MoonChatClickController chatController;

    [Header("Tap Settings")]
    [SerializeField] private float dragThresholdPixels = 18f; // 손가락 기준 12~24 사이 추천

    private Vector2 downPos;
    private bool moved;

    public void OnPointerDown(PointerEventData eventData)
    {
        downPos = eventData.position;
        moved = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그가 일정 이상이면 탭 취소
        if (moved) return;

        float dist = Vector2.Distance(downPos, eventData.position);
        if (dist >= dragThresholdPixels)
            moved = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (moved) return;              // 드래그로 간주
        if (chatController == null) return;

        chatController.RunScript();     // 다음 대사
    }
}
