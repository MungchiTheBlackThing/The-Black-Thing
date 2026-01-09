using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;

public class Day8SleepEventController : MonoBehaviour
{

    [Header("Run Condition (optional)")]
    [SerializeField] private GameManager gm;          // 조건 체크용(연결 가능하면 연결)
    [SerializeField] private PlayerController pc;     // 조건 체크용(연결 가능하면 연결)

    [Header("Input Lock")]
    [SerializeField] private ScrollManager scrollManager; // 맵 드래그 차단
    [SerializeField] private GameObject inputBlocker;     // 전체 클릭 차단 패널(선택)

    [Header("UI Hide During Event")]
    [SerializeField] private GameObject[] uiToHide;       // 이벤트 동안 숨길 UI(선택)

    [Header("Camera Cutscene")]
    [SerializeField] private float startDelay = 2f; // Day8 시작 딜레이

    [SerializeField] private Camera eventCamera;
    [SerializeField] private float camMoveDuration = 4f;
    [SerializeField] private float camHoldDuration = 2f;
    [SerializeField] private float duration = 2.5f;

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource eerieSource;
    [SerializeField] private float bgmCutFade = 0.08f;

    [Header("Mold Object")]
    [SerializeField] private List<GameObject> moldObjects = new List<GameObject>();

    [Header("Sub Dialogues (Hierarchy toggle)")]
    [Tooltip("EventDay8/0~8 오브젝트를 순서대로 넣기")]
    [SerializeField] private List<GameObject> subDialogueObjects = new List<GameObject>(9);

    [Tooltip("전체화면 클릭으로 다음 라인 넘기는 SubClick 오브젝트")]
    [SerializeField] private GameObject subClickObject;

    [Tooltip("SubClick 오브젝트의 Button")]
    [SerializeField] private Button subClickButton;

    [Header("Popups (Scene-localized; toggle only)")]
    [SerializeField] private GameObject popupPhase1;
    [SerializeField] private Button popup1ThrowButton;
    [SerializeField] private Button popup1KeepButton;

    [SerializeField] private GameObject popupPhase2;
    [SerializeField] private Button popup2OptionAButton;
    [SerializeField] private Button popup2OptionBButton;

    [SerializeField] private GameObject popupPhase3;
    [SerializeField] private Button popup3SingleButton;

    [Header("Alerts (Spawn 30 then clear)")]
    [SerializeField] private GameObject alertsRoot;
    [SerializeField] private GameObject alertItemPrefab;
    [SerializeField] private RectTransform alertSpawnArea;
    [SerializeField] private int alertCount = 30;
    [SerializeField] private float alertSpawnInterval = 0.03f;
    [SerializeField] private float alertLifetime = 2.5f;

    [Header("Alert Localization")]
    [SerializeField] private string alertStringTable = "UI";
    [SerializeField] private string[] alertKeys = new[] { "DAY8_ALERT_01", "DAY8_ALERT_02", "DAY8_ALERT_03" };


    // ===== runtime =====
    private bool isRunning;
    public bool IsRunning => isRunning;   // GameManager에서 ShowSubDial 막는 데 사용 가능

    private Vector3 camStartPos;
    private Quaternion camStartRot;

    // GameManager가 호출할 엔트리포인트 (이름 통일!)
    public void TryStart()
    {
        if (isRunning) return;
        if (!ShouldRun()) return;

        isRunning = true;
        if (gm != null) gm.PausePhaseTimer();


        StartCoroutine(DelayedStart());

    }

    // 조건: Day==8, Phase==Sleeping, Completed==false
    public bool ShouldRun()
    {
        var info = (pc != null) ? pc.GetPlayerInfo() : null;
        if (info != null && info.isDay8SleepEventCompleted) return false;


        if (gm == null || pc == null) return false;

        bool dayOk = (pc.GetChapter() == 8);
        bool phaseOk = (gm.Pattern == GamePatternState.Sleeping);
        return dayOk && phaseOk;
    }
    private IEnumerator DelayedStart()
    {
        yield return WaitUnscaled(startDelay);
        yield return RunEvent();
    }

    private IEnumerator RunEvent()
    {

        DeactivateAll();
        InputGuard.WorldInputLocked = true;

        LockMapInput(true);
        HideUI(true);
        SetActiveSafe(inputBlocker, true);

        CacheCameraStart();

        // 1~5s 카메라 이동 (왼쪽 끝으로)
        yield return MoveCameraXTo(leftEndX, camMoveDuration);

        // 5~7s 클로즈업(선택: 오쏘 사이즈 줄여서 확대) + 홀드
        yield return SetOrthoSize(closeUpOrthoSize, 0.15f); // 순간 줌처럼 보이게 짧게
        yield return WaitUnscaled(camHoldDuration);

        // 7s: BGM 컷 + 기이한 음
        yield return CutBgmAndStartEerie();

        // Sub 0~2
        yield return PlaySubRangeByClick(0, 2);

        // Popup 1
        bool threw = false;
        yield return WaitPopupPhase1(v => threw = v);

        if (threw)
        {
            yield return SuccessAndFinish();
            yield break;
        }

        // Sub 4~6
        yield return PlaySubRangeByClick(3, 5);

        // Popup 2
        yield return WaitPopupPhase2();

        // Alerts 30
        yield return PlayAlertsSpawn30AndClear();

        // Popup 3
        yield return WaitPopupPhase3();

        // Success
        yield return SuccessAndFinish();
    }

    [SerializeField] private float leftEndX = -5.25f; // ScrollManager Cam Limit Value X
    [SerializeField] private float closeUpOrthoSize = 5.2f; // 클로즈업 때 더 확대하고 싶으면
    private float originalOrthoSize;

    private void CacheCameraStart()
    {
        if (eventCamera == null) return;
        camStartPos = eventCamera.transform.position;
        camStartRot = eventCamera.transform.rotation;

        // 오쏘 사이즈도 원복 가능하게 캐시
        originalOrthoSize = eventCamera.orthographicSize;
    }

    private IEnumerator MoveCameraXTo(float targetX, float duration)
    {
        if (eventCamera == null) yield break;

        Vector3 from = eventCamera.transform.position;
        Vector3 to = new Vector3(targetX, from.y, from.z);

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            eventCamera.transform.position = Vector3.Lerp(from, to, a);
            yield return null;
        }
        eventCamera.transform.position = to;
    }

    private IEnumerator SetOrthoSize(float to, float duration)
    {
        if (eventCamera == null) yield break;
        if (!eventCamera.orthographic) yield break;

        float from = eventCamera.orthographicSize;
        if (duration <= 0.001f)
        {
            eventCamera.orthographicSize = to;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            eventCamera.orthographicSize = Mathf.Lerp(from, to, a);
            yield return null;
        }
        eventCamera.orthographicSize = to;
    }


    // ===== Sub Dialogue =====
    private IEnumerator PlaySubRangeByClick(int startIndex, int endIndex)
    {
        SetSubClick(true);

        if (subClickButton == null && subClickObject != null)
            subClickButton = subClickObject.GetComponentInChildren<Button>(true);

        if (subClickButton == null)
        {
            Debug.LogError("[Day8SleepEvent] SubClick Button not assigned.");
            yield break;
        }

        startIndex = Mathf.Max(0, startIndex);
        endIndex = Mathf.Min(subDialogueObjects.Count - 1, endIndex);

        int current = startIndex;

        ActivateSubLine(current);

        bool clicked = false;
        subClickButton.onClick.RemoveAllListeners();
        subClickButton.onClick.AddListener(() => clicked = true);

        while (current <= endIndex)
        {
            clicked = false;
            while (!clicked) yield return null;

            SetActiveSafe(subDialogueObjects[current], false);
            current++;

            if (current <= endIndex)
                ActivateSubLine(current);
        }

        SetSubClick(false);
    }

    private void ActivateSubLine(int index)
    {
        for (int i = 0; i < subDialogueObjects.Count; i++)
            SetActiveSafe(subDialogueObjects[i], i == index);
    }

    private void SetSubClick(bool active)
    {
        SetActiveSafe(subClickObject, active);
    }

    // ===== Popups =====
    private IEnumerator WaitPopupPhase1(System.Action<bool> onPickedThrow)
    {
        SetSubClick(false);
        ActivateOnly(popupPhase1);

        if (popup1ThrowButton == null || popup1KeepButton == null)
        {
            Debug.LogError("[Day8SleepEvent] Popup1 buttons not assigned.");
            onPickedThrow?.Invoke(true);
            yield break;
        }

        bool picked = false;

        popup1ThrowButton.onClick.RemoveAllListeners();
        popup1KeepButton.onClick.RemoveAllListeners();

        popup1ThrowButton.onClick.AddListener(() => { onPickedThrow?.Invoke(true); picked = true; });
        popup1KeepButton.onClick.AddListener(() => { onPickedThrow?.Invoke(false); picked = true; });

        while (!picked) yield return null;

        SetActiveSafe(popupPhase1, false);
    }

    private IEnumerator WaitPopupPhase2()
    {
        SetSubClick(false);
        ActivateOnly(popupPhase2);

        if (popup2OptionAButton == null || popup2OptionBButton == null)
        {
            Debug.LogError("[Day8SleepEvent] Popup2 buttons not assigned.");
            yield break;
        }

        bool picked = false;
        popup2OptionAButton.onClick.RemoveAllListeners();
        popup2OptionBButton.onClick.RemoveAllListeners();

        popup2OptionAButton.onClick.AddListener(() => picked = true);
        popup2OptionBButton.onClick.AddListener(() => picked = true);

        while (!picked) yield return null;

        SetActiveSafe(popupPhase2, false);
    }

    private IEnumerator WaitPopupPhase3()
    {
        SetSubClick(false);
        ActivateOnly(popupPhase3);

        if (popup3SingleButton == null)
        {
            Debug.LogError("[Day8SleepEvent] Popup3 button not assigned.");
            yield break;
        }

        bool picked = false;
        popup3SingleButton.onClick.RemoveAllListeners();
        popup3SingleButton.onClick.AddListener(() => picked = true);

        while (!picked) yield return null;

        SetActiveSafe(popupPhase3, false);
    }

    // ===== Alerts =====
    private IEnumerator PlayAlertsSpawn30AndClear()
    {
        SetSubClick(false);
        SetActiveSafe(alertsRoot, true);

        if (alertsRoot == null || alertItemPrefab == null || alertSpawnArea == null)
            yield break;

        var spawned = new List<GameObject>(alertCount);

        for (int i = 0; i < alertCount; i++)
        {
            string msg = GetLocalizedAlert(i);

            GameObject go = Instantiate(alertItemPrefab, alertsRoot.transform);
            spawned.Add(go);

            TMP_Text t = go.GetComponentInChildren<TMP_Text>(true);
            if (t != null)
            {
                t.text = msg;
                t.gameObject.SetActive(true);
            }

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = RandomPointInRect(alertSpawnArea);

            yield return WaitUnscaled(alertSpawnInterval);
        }

        yield return WaitUnscaled(alertLifetime);

        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i] != null)
                spawned[i].SetActive(false);

        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i] != null)
                Destroy(spawned[i]);

        SetActiveSafe(alertsRoot, false);
    }

    private string GetLocalizedAlert(int index)
    {
        if (alertKeys == null || alertKeys.Length == 0) return string.Empty;
        string key = alertKeys[index % alertKeys.Length];
        if (string.IsNullOrEmpty(key)) return string.Empty;

        return LocalizationSettings.StringDatabase.GetLocalizedString(alertStringTable, key);
    }

    private Vector2 RandomPointInRect(RectTransform rect)
    {
        Rect r = rect.rect;
        float x = Random.Range(r.xMin, r.xMax);
        float y = Random.Range(r.yMin, r.yMax);
        return new Vector2(x, y);
    }

    private void HideBreadMoldInScene()
    {
        // 씬에 존재하는 곰팡이 인스턴스 타격
        foreach (var ch_bread in FindObjectsOfType<ChBreadObject>(true))
            ch_bread.gameObject.SetActive(false);
    }


    // ===== Success =====
    private IEnumerator SuccessAndFinish()
    {

        if (pc != null)
        {
            var info = pc.GetPlayerInfo();
            if (info != null)
            {
                info.isDay8SleepEventCompleted = true;
                pc.SavePlayerInfo();
            }
        }

        HideBreadMoldInScene();

        yield return PlaySubRangeByClick(6, 8);

        yield return ReturnCamera(duration);


        RestoreBgm();

        SetActiveSafe(inputBlocker, false);
        HideUI(false);
        LockMapInput(false);

        DeactivateAll();
        InputGuard.WorldInputLocked = false;
        isRunning = false;
        StartCoroutine(ResumeTimerNextFrame());

    }
    private IEnumerator ResumeTimerNextFrame()
    {
        yield return null; // 1 frame
        if (gm != null) gm.RestartSleepingTimer();

    }


    // ===== Camera / Audio =====

    private IEnumerator MoveCameraTo(Transform target, float duration)
    {
        if (eventCamera == null || target == null) yield break;

        if (duration <= 0.001f)
        {
            eventCamera.transform.position = target.position;
            eventCamera.transform.rotation = target.rotation;
            yield break;
        }

        Vector3 fromPos = eventCamera.transform.position;
        Quaternion fromRot = eventCamera.transform.rotation;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            eventCamera.transform.position = Vector3.Lerp(fromPos, target.position, a);
            eventCamera.transform.rotation = Quaternion.Slerp(fromRot, target.rotation, a);
            yield return null;
        }

        eventCamera.transform.position = target.position;
        eventCamera.transform.rotation = target.rotation;
    }

    private IEnumerator ReturnCamera(float duration)
    {
        if (eventCamera == null) yield break;

        Vector3 fromPos = eventCamera.transform.position;
        Quaternion fromRot = eventCamera.transform.rotation;
        float fromSize = eventCamera.orthographicSize;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            eventCamera.transform.position = Vector3.Lerp(fromPos, camStartPos, a);
            eventCamera.transform.rotation = Quaternion.Slerp(fromRot, camStartRot, a);
            eventCamera.orthographicSize = Mathf.Lerp(fromSize, originalOrthoSize, a);
            yield return null;
        }

        eventCamera.transform.position = camStartPos;
        eventCamera.transform.rotation = camStartRot;
        eventCamera.orthographicSize = originalOrthoSize;
    }

    private IEnumerator CutBgmAndStartEerie()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            float from = bgmSource.volume;
            float t = 0f;
            while (t < bgmCutFade)
            {
                t += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(from, 0f, t / bgmCutFade);
                yield return null;
            }
            bgmSource.Stop();
            bgmSource.volume = from;
        }

        if (eerieSource != null && !eerieSource.isPlaying)
            eerieSource.Play();
    }

    private void RestoreBgm()
    {
        if (eerieSource != null && eerieSource.isPlaying)
            eerieSource.Stop();

        if (bgmSource != null && !bgmSource.isPlaying)
            bgmSource.Play();
    }

    // ===== UI / Input helpers =====
    private void LockMapInput(bool locked)
    {
        if (scrollManager == null) return;
        if (locked) scrollManager.stopscroll();
        else scrollManager.scrollable();
    }

    private void HideUI(bool hide)
    {
        if (uiToHide == null) return;
        foreach (var go in uiToHide)
            if (go != null) go.SetActive(!hide);
    }

    private void DeactivateAll()
    {
        for (int i = 0; i < subDialogueObjects.Count; i++)
            SetActiveSafe(subDialogueObjects[i], false);

        SetSubClick(false);

        SetActiveSafe(popupPhase1, false);
        SetActiveSafe(popupPhase2, false);
        SetActiveSafe(popupPhase3, false);
        SetActiveSafe(alertsRoot, false);
    }

    private void ActivateOnly(GameObject panel)
    {
        SetActiveSafe(popupPhase1, panel == popupPhase1);
        SetActiveSafe(popupPhase2, panel == popupPhase2);
        SetActiveSafe(popupPhase3, panel == popupPhase3);
        SetActiveSafe(alertsRoot, panel == alertsRoot);
    }

    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    private IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;

        duration = Mathf.Max(0.0001f, duration);
        cg.alpha = from;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            cg.alpha = Mathf.Lerp(from, to, a);
            yield return null;
        }

        cg.alpha = to;
    }

    private IEnumerator WaitUnscaled(float sec)
    {
        if (sec <= 0f) yield break;
        float t = 0f;
        while (t < sec)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void OnDisable()
    {
        if (!isRunning) return;

        StopAllCoroutines();

        // 완료 플래그는 저장하지 않음 (요구사항)
        DeactivateAll();

        SetActiveSafe(inputBlocker, false);
        HideUI(false);
        LockMapInput(false);
        RestoreBgm();
        InputGuard.WorldInputLocked = false;

        if (gm != null)
            gm.RestartSleepingTimer();

        isRunning = false;
    }

}
