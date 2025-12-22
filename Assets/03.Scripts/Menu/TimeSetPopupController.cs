using System.Collections;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeSettingPopupController : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Button ampmButton;  // ButtonAM
    [SerializeField] private GameObject textAM;  // TextAM 오브젝트
    [SerializeField] private GameObject textPM;  // TextPM 오브젝트
    [SerializeField] private ScrollRect hourScroll;
    [SerializeField] private ScrollRect minuteScroll;


    private int _originalHour24;
    private int _originalMinute;

    private int _tempHour12 = 12;
    private int _tempMinute = 0;
    private bool _tempIsPM = false;

    [Header("Scroll Snap")]
    [SerializeField] private bool enableSnap = true;
    [SerializeField] private float snapSpeed = 18f;
    [SerializeField] private float stopVelocityThreshold = 80f;


    private System.Action<int,int> _onApplyTime;

    void Awake()
    {
        if (ampmButton != null)
            ampmButton.onClick.AddListener(ToggleAMPM);

        if (hourScroll != null) hourScroll.onValueChanged.AddListener(_ => OnHourScrollChanged());
        if (minuteScroll != null) minuteScroll.onValueChanged.AddListener(_ => OnMinuteScrollChanged());
    }

    public void Open(int initialHour24, int initialMinute, System.Action<int,int> onApplyTime)
    {
        _onApplyTime = onApplyTime;

        _originalHour24 = Mathf.Clamp(initialHour24, 0, 23);
        _originalMinute = Mathf.Clamp(initialMinute, 0, 59);

        Hour24ToTemp(_originalHour24, out _tempHour12, out _tempIsPM);
        _tempMinute = _originalMinute;

        gameObject.SetActive(true);          // 먼저 켬

        Canvas.ForceUpdateCanvases();         // 레이아웃 확정

        JumpToHour(_tempHour12);              // 스크롤을 값 위치로 이동
        JumpToMinute(_tempMinute);

        ApplyTempToUI();                      // 마지막에 UI 반영
    }

    public void Cancel()
    {
        Hour24ToTemp(_originalHour24, out _tempHour12, out _tempIsPM);
        _tempMinute = _originalMinute;
        gameObject.SetActive(false);
    }

    public void Save()
    {
        int hour24 = TempToHour24(_tempHour12, _tempIsPM);
        _onApplyTime?.Invoke(hour24, _tempMinute);
        gameObject.SetActive(false);
    }

    private void ToggleAMPM()
    {
        _tempIsPM = !_tempIsPM;
        ApplyTempToUI();
    }


    private void OnHourScrollChanged()
    {
        _tempHour12 = ReadSelectedIntFromScroll(hourScroll, fallback: _tempHour12);
        ApplyTempToUI();
    }

    private void OnMinuteScrollChanged()
    {
        _tempMinute = ReadSelectedIntFromScroll(minuteScroll, fallback: _tempMinute);
        ApplyTempToUI();
    }

    private int ReadSelectedIntFromScroll(ScrollRect sr, int fallback)
    {
        if (sr == null || sr.content == null || sr.viewport == null) return fallback;
        if (sr.content.childCount == 0) return fallback;

        int idx = GetClosestIndexToCenter(sr);
        var t = sr.content.GetChild(idx).GetComponent<TMP_Text>();
        if (t == null) return fallback;

        // "02"도 파싱되게
        if (int.TryParse(t.text, out int v)) return v;
        return fallback;
    }

    private int GetClosestIndexToCenter(ScrollRect sr)
    {
        var viewport = sr.viewport;
        var content = sr.content;

        var vpRect = viewport.rect;
        Vector3 vpCenter = viewport.TransformPoint(new Vector3(vpRect.center.x, vpRect.center.y, 0));

        int best = 0;
        float bestDist = float.MaxValue;

        for (int i = 0; i < content.childCount; i++)
        {
            var child = content.GetChild(i) as RectTransform;
            Vector3 childCenter = child.TransformPoint(child.rect.center);
            float dist = Mathf.Abs(childCenter.y - vpCenter.y);
            if (dist < bestDist) { bestDist = dist; best = i; }
        }
        return best;
    }

    private void ApplyTempToUI()
    {

        if (textAM != null) textAM.SetActive(!_tempIsPM);
        if (textPM != null) textPM.SetActive(_tempIsPM);

    }

    private static int TempToHour24(int hour12, bool isPM)
    {
        hour12 = Mathf.Clamp(hour12, 1, 12);
        if (isPM) return (hour12 == 12) ? 12 : hour12 + 12;
        return (hour12 == 12) ? 0 : hour12;
    }

    private static void Hour24ToTemp(int hour24, out int hour12, out bool isPM)
    {
        hour24 = Mathf.Clamp(hour24, 0, 23);
        isPM = hour24 >= 12;
        if (hour24 == 0) hour12 = 12;
        else if (hour24 < 12) hour12 = hour24;
        else if (hour24 == 12) hour12 = 12;
        else hour12 = hour24 - 12;
    }

    void LateUpdate()
    {
        if (!enableSnap) return;

        SnapIfStopped(hourScroll);
        SnapIfStopped(minuteScroll);
    }

    private void SnapIfStopped(ScrollRect sr)
    {
        if (sr == null || sr.content == null || sr.viewport == null) return;

        // 드래그 중이면 스냅 금지
        if (Input.GetMouseButton(0)) return;

        // 아직 충분히 멈추지 않았으면 건드리지 않음
        if (sr.velocity.magnitude > stopVelocityThreshold) return;

        int idx = GetClosestIndexToCenter(sr);
        Vector2 target = GetAnchoredPosToCenterChild(sr, idx);

        sr.content.anchoredPosition =
            Vector2.Lerp(sr.content.anchoredPosition, target, Time.unscaledDeltaTime * snapSpeed);

        // 거의 도착하면 고정 + 속도 0
        if (Vector2.Distance(sr.content.anchoredPosition, target) < 0.5f)
        {
            sr.content.anchoredPosition = target;
            sr.velocity = Vector2.zero;

            // 스냅 완료 시 값 동기화
            if (sr == hourScroll)
                _tempHour12 = ReadSelectedIntFromScroll(hourScroll, _tempHour12);
            else if (sr == minuteScroll)
                _tempMinute = ReadSelectedIntFromScroll(minuteScroll, _tempMinute);

            ApplyTempToUI();
        }
    }


    private Vector2 GetAnchoredPosToCenterChild(ScrollRect sr, int childIndex)
    {
        var viewport = sr.viewport;
        var content = sr.content;

        var child = content.GetChild(childIndex) as RectTransform;

        var vpRect = viewport.rect;
        Vector3 vpCenterWorld = viewport.TransformPoint(vpRect.center);

        Vector3 childCenterWorld = child.TransformPoint(child.rect.center);

        Vector3 deltaWorld = vpCenterWorld - childCenterWorld;
        Vector3 deltaLocal = content.InverseTransformVector(deltaWorld);

        return content.anchoredPosition + new Vector2(0, deltaLocal.y);
    }

    private void JumpToHour(int hour12)
    {
        int index = Mathf.Clamp(hour12, 1, 12) - 1;
        SetScrollToIndex(hourScroll, index);
    }

    private void JumpToMinute(int minute)
    {
        int idx = Mathf.RoundToInt(minute / 10f);
        idx = Mathf.Clamp(idx, 0, 5);
        SetScrollToIndex(minuteScroll, idx);
    }

    private void SetScrollToIndex(ScrollRect sr, int index)
    {
        if (sr == null || sr.content == null || sr.viewport == null) return;
        if (sr.content.childCount == 0) return;

        index = Mathf.Clamp(index, 0, sr.content.childCount - 1);

        // 레이아웃 확정
        Canvas.ForceUpdateCanvases();

        RectTransform child = sr.content.GetChild(index) as RectTransform;
        if (child == null) return;

        // Viewport 중심과 child 중심을 맞추기
        Vector2 vpCenter = sr.viewport.rect.center;
        Vector2 childCenter =
            (Vector2)sr.viewport.InverseTransformPoint(child.TransformPoint(child.rect.center));

        Vector2 offset = vpCenter - childCenter;
        sr.content.anchoredPosition += offset;

        sr.velocity = Vector2.zero;
    }



}

