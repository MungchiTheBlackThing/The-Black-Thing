using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SubTuto : MonoBehaviour
{
    [Header("UI Roots")]
    [SerializeField] private Canvas _mainCanvas;           // Screen 캔버스(하나면 비워도 됨)
    [SerializeField] private RectTransform _canvasRect;    // 자동 세팅됨
    private RectTransform _subPanelRect;

    [SerializeField] SubDialogue subDialogue;
    [SerializeField] SubPanel subPanel;
    [SerializeField] TouchGuide touch;
    [SerializeField] GameObject nickname;
    [SerializeField] TutorialManager tutorialManager;
    [SerializeField] CameraZoom cameraZoom;
    [SerializeField] public GameObject UIBalloon;
    [SerializeField] public Moonnote moonnote;
    [SerializeField] GameObject SystemUI;
    [SerializeField] PlayerController playerController;
    [SerializeField] DotController dotController;

    [SerializeField] private Vector2 _offsetDoorOpen  = new Vector2(80f, 0f); // 예: 오른쪽 80px
    private bool tuto11chk = false;

    public string prefabPath = "TouchGuide";

    Vector3 guide1 = new Vector3(-810, -145, 0);
    Vector3 guide2 = new Vector3(-1095, -195, 0);
    Vector3 guide3 = new Vector3(-800, -470, 0);

    private void Awake()
    {
        if (subPanel != null) _subPanelRect = subPanel.GetComponent<RectTransform>();

        if (_mainCanvas == null) _mainCanvas = FindObjectOfType<Canvas>();
        if (_mainCanvas != null) _canvasRect = _mainCanvas.GetComponent<RectTransform>();
    }


    public static class UIAnchorUtil
    {
        // ui: 움직일 UI (TouchGuide의 RectTransform)
        // uiRoot: ui의 부모(= subPanel RectTransform)
        // canvas: 최상위 캔버스 RectTransform
        // anchor: 문 기준 Transform(월드든 UI든)
        // cam: 메인 카메라 (Overlay여도 넣는 게 안전)
        public static void Snap(
            RectTransform ui,
            RectTransform uiRoot,
            RectTransform canvas,
            Transform anchor,
            Camera cam,
            Vector2 offset)
        {
            if (!ui || !uiRoot || !canvas || anchor == null) return;

            // 1) 앵커가 UI(RectTransform)면: UI의 월드좌표를 uiRoot 로컬로 변환
            var anchorRT = anchor as RectTransform;
            if (anchorRT != null)
            {
                // 앵커의 "가운데"를 기준점으로
                Vector3 anchorWorld = anchorRT.TransformPoint(anchorRT.rect.center);
                Vector3 localOnRoot = uiRoot.InverseTransformPoint(anchorWorld);
                ui.anchoredPosition = (Vector2)localOnRoot + offset;
                return;
            }

            // 2) 앵커가 월드 Transform이면: 월드 -> 스크린 -> 캔버스 로컬 -> uiRoot 로컬
            // 월드->스크린은 무조건 월드카메라 필요
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            Vector2 screen = cam.WorldToScreenPoint(anchor.position);

            // Overlay는 여기서만 null camera가 안전
            Camera uiCam = (canvas.GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay) ? null : cam;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen, uiCam, out var canvasLocal))
                return;

            Vector3 worldOnCanvas = canvas.TransformPoint(canvasLocal);
            Vector3 local = uiRoot.InverseTransformPoint(worldOnCanvas);
            ui.anchoredPosition = (Vector2)local + offset;
        }
    }

    private IEnumerator SnapNextFrame(RectTransform ui, Transform worldAnchor, Vector2 offset)
    {
        if (ui == null) yield break;

        // 0) 1프레임 튐 방지: 스냅 전 숨김
        var cg = ui.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        yield return null; // 레이아웃 안정화 1프레임

        if (worldAnchor == null || _subPanelRect == null || _canvasRect == null)
        {
            Debug.LogError($"[SubTuto] Snap failed. anchor={(worldAnchor!=null)} subPanelRect={(_subPanelRect!=null)} canvasRect={(_canvasRect!=null)}");
            yield break;
        }

        var cam = Camera.main;
        UIAnchorUtil.Snap(ui, _subPanelRect, _canvasRect, worldAnchor, cam, offset);

        // 1) 스냅 후 표시 + 클릭
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }
    }

    private bool TryResolveDoorAnchors(out Transform close, out Transform open)
    {
        close = null;
        open = null;

        // DoorController가 붙어있는 애들 중 "활성"인 door를 잡는다
        var doors = FindObjectsOfType<DoorController>(true); // includeInactive = true
        foreach (var d in doors)
        {
            if (!d.gameObject.activeInHierarchy) continue;

            // 위로 올라가면서 fix_door 루트 찾기
            Transform t = d.transform;
            while (t.parent != null && t.name != "fix_door")
                t = t.parent;

            if (t.name != "fix_door") continue;
            if (!t.gameObject.activeInHierarchy) continue;

            close = t.Find("fix_door_close");
            open  = t.Find("fix_door_open");

            if (close != null && open != null)
            {
                //Debug.Log($"[SubTuto] Resolved fix_door={t.GetInstanceID()} close={close.name} open={open.name}");
                return true;
            }
        }

        return false;
    }


    public void tutorial_2(GameObject selectedDot, int determine, int index)
    {
        var prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null) { Debug.LogError("프리팹을 찾을 수 없습니다!"); return; }

        var instance = Instantiate(prefab, subPanel.transform);
        instance.SetActive(true);
        touch = instance.GetComponent<TouchGuide>();

        if (!TryResolveDoorAnchors(out var close, out var open))
            Debug.LogError("[SubTuto] Cannot resolve door anchors (close/open).");

        StartCoroutine(SnapNextFrame(instance.GetComponent<RectTransform>(), close, Vector2.zero));
        touch.tuto2(selectedDot, determine);
    }

    public void tutorial_3(GameObject selectedDot, int determine, int index)
    {
        var prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null) { Debug.LogError("프리팹을 찾을 수 없습니다!"); return; }

        var instance = Instantiate(prefab, subPanel.transform);
        instance.SetActive(true);
        touch = instance.GetComponent<TouchGuide>();

        if (!TryResolveDoorAnchors(out var close, out var open))
            Debug.LogError("[SubTuto] Cannot resolve door anchors (close/open).");

        StartCoroutine(SnapNextFrame(instance.GetComponent<RectTransform>(), open, _offsetDoorOpen));
        touch.tuto3(selectedDot, determine);
    }

    public void tutorial_4(GameObject selectedDot, int determine, int index)
    {
        GameObject door = GameObject.Find("fix_door");
        door.transform.GetChild(1).GetComponent<DoorController>().open();
        subPanel.clickon();

        if (determine == 0)
            subPanel.dotballoon(selectedDot);
        else
            subPanel.playerballoon(selectedDot);
    }

    public void tutorial_5(GameObject selectedDot, int determine, int index)
    {
        RecentManager.Save(selectedDot, determine, index); // 저장
        if (determine == 0)
            subPanel.dotballoon(selectedDot);
        else
            subPanel.playerballoon(selectedDot);

        nickname.SetActive(true);
    }

    public void tutorial_7(GameObject selectedDot, int determine, int index)
    {
        RecentData data = RecentManager.Load();
        if (data.tutonum == 0)
        {
            cameraZoom.Zoom();
            subPanel.gameObject.SetActive(false);
        }
        else
        {
            subPanel.dotballoon(selectedDot);
        }
        RecentManager.Save(selectedDot, determine, index); // 저장
    }

    public void tutorial_8(GameObject selectedDot, int determine, int index)
    {
        //RecentManager.Save(selectedDot, determine, index); // 저장
        RecentData data = RecentManager.Load();
        data.value = 1;
        RecentManager.Save(selectedDot, 1, 69); // 저장
        tutorialManager.Dot.ChangeState(DotPatternState.Phase, "anim_watching", 1.5f);
        moonnote = GameObject.FindWithTag("moonnote").GetComponent<Moonnote>();
        StartCoroutine(Scroallable());
    }
    public void tutorial_9(GameObject selectedDot, int determine, int index)
    {
        //RecentManager.Save(selectedDot, determine, index); // 저장
        if (!tutorialManager)
        {
            Subcontinue(106);
        }
        else //튜토리얼 끝
        {
            dotController.tutorial = false;
            Debug.Log("3");
            playerController.NextPhase();
            playerController.WritePlayerFile();
            RecentManager.tutoSceneEnd();
            LoadSceneManager.Instance.LoadScene("Tutorial", "MainScene", 1);
        }
    }

    public void tutorial_10(GameObject selectedDot, int determine, int index)
    {
        //RecentManager.Save(selectedDot, determine, index); // 저장
        if (playerController.GetCurrentPhase() == 5)
        {
            subPanel.dotballoon(selectedDot);
        }
        else
        {
            subDialogue.TutoExit();
            playerController.NextPhase();
            playerController.WritePlayerFile();
        }
    }

    public void tutorial_11(GameObject selectedDot, int determine, int index)
    {
        //RecentManager.Save(selectedDot, determine, index); // 저장
        if (!dotController.isEndPlay)
        {
            dotController.GoSleep();
            StartCoroutine(subcon());
        }
        else
        {
            subPanel.dotballoon(selectedDot);
        }
    }

    private IEnumerator subcon()
    {
        yield return new WaitForSeconds(4f);
        subPanel.dialogueIndex = 130;
        subPanel.ShowNextDialogue();
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOperation.isDone)
        {
            Debug.Log($"Loading progress: {asyncOperation.progress * 100}%");
            yield return null;
        }
    }

    public void tuto7()
    {
        tutorialManager.ChangeGameState(TutorialState.Main);
    }

    public void zoomout()
    {
        tutorialManager.ChangeGameState(TutorialState.Sub);
    }

    public void Subcontinue(int index = -1)
    {
        RecentData data = RecentManager.Load();
        if (data != null && data.isContinue == 1)
        {
            GameObject targetObj = subPanel.FindPanelObjectByName(data.objectName);
            if (targetObj == null)
            {
                Debug.LogWarning($"오브젝트 {data.objectName} 를 찾을 수 없습니다.");
                return;
            }

            subPanel.prePos = dotController.Position;

            if (subDialogue.currentDialogueList == null || subDialogue.currentDialogueList.Count == 0 && index != -1)
            {
                Debug.Log("서브이어 1" + data.index);
                subDialogue.StartSub("tutorial_sub", data.index);
            }
            else
            {
                Debug.Log("서브이어 2" +  index);
                subDialogue.StartSub("tutorial_sub", index);
            }
        }
        else
        {
            Debug.Log("이어할 튜토리얼 데이터 없음");
        }
    }

    public IEnumerator Scroallable()
    {
        yield return new WaitForSeconds(3f);
        //dot.GetComponent<DotController>().Invisible();
        UIBalloon.SetActive(true);
        moonnote.anion(UIBalloon);
        SystemUI.SetActive(true);
        var menu = FindObjectOfType<MenuController>();
        menu?.SetMenuButtonVisible(true);
        menu?.SetMenuButtonInteractable(false); // 튜토 씬에서 꺼 둔 메뉴 여기서 다시 켜기 (보이게만), 여기서 처음으로 메뉴 버튼 노출!
    }

    public void skiptouchGuide()
    {
        GameObject touchguide = Resources.Load<GameObject>(prefabPath);

        GameObject instance = Instantiate(touchguide, subPanel.gameObject.transform);
        var rt = instance.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(-768f, -483f);
        rt.localRotation = Quaternion.Euler(180f, 0f, 60.685f);
        
        instance.SetActive(true);
        touch = instance.GetComponent<TouchGuide>();

        touch.skipGuide();
    }
}