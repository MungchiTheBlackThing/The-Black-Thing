using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UITutorial : MonoBehaviour
{
    public List<GameObject> Guideline = new List<GameObject>();
    [SerializeField] MenuController MenuController;
    [SerializeField] GameObject menuBut;
    [SerializeField] GameObject progressBut;
    [SerializeField] ProgressUIController progressUIController;
    [SerializeField] GameObject icon;
    [SerializeField] GameObject DayProgress;
    [SerializeField] GameObject Subicon;
    [SerializeField] PlayerController player;

    CanvasGroup tutorialMaskGroup;
    CanvasGroup Spider;
    CanvasGroup Progress;
    GameObject preparent;
    Vector3 originalPosition;

    int presibling;
    private bool G2 = false;
    private bool G3 = false;
    private bool G4 = false;
    private bool G5 = false;
    private bool G6 = false;

    private bool _guide1Ready = false;     // Guide1을 정상적으로 시작했는지
    private bool _guide1UIShown = false;   // Guide1의 Guideline[0]이 실제로 켜졌는지(보험)

    int index = 0;
    // Start is called before the first frame update
    private int step = 0;

    private bool _menuOpenedSignal = false;

    private bool _progressClicked = false;

    void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Guideline.Add(transform.GetChild(i).gameObject);
        }
        tutorialMaskGroup = this.GetComponent<CanvasGroup>();
        if (menuBut) Spider = menuBut.GetComponent<CanvasGroup>();
        if (progressBut) Progress = progressBut.GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        ResetState();
        _menuOpenedSignal = false;
        MenuController.OnMenuOpened += HandleMenuOpened;

        ScreenShield.Off();
        StartCoroutine(StartGuide1());
    }

    void ResetState()
    {
        index = 0;
        G2 = false;
        G3 = false;
        G4 = false;
        G5 = false;
        G6 = false;
        if (progressUIController) progressUIController.guide1 = false;
        if (progressUIController) progressUIController.guide2 = false;
        _guide1Ready = false;
        _guide1UIShown = false;
        _menuOpenedSignal = false;

    // 여기 추가: 가이드라인 전부 끄기
        for (int i = 0; i < Guideline.Count; i++)
            if (Guideline[i]) Guideline[i].SetActive(false);
    }

    private void Update()
    {

        if (!G3 && G2 && _progressClicked)
        {
            StartCoroutine(Guide3());
            G3 = true;
            _progressClicked = false;
        }

        if (!G4 && progressUIController.guide1)
        {
            Guide4();
            G4 = true;
            progressUIController.guide1 = false;
        }

        if (!G5 && progressUIController.guide2)
        {
            Guide5();
            G5 = true;
            progressUIController.guide2 = false;
        }
    }

    [SerializeField] private RectTransform _topLayer; // Canvas 최상단 패널(예: TutorialTopLayer) 드래그
    private GameObject _ch1Clone;

    private GameObject CreateCh1CloneOnTop(string originalName)
    {
        var originalGO = GameObject.Find(originalName);
        if (!originalGO) { Debug.LogError($"[UITutorial] '{originalName}' not found"); return null; }

        var src = originalGO.GetComponent<RectTransform>();
        if (!src) { Debug.LogError("[UITutorial] original has no RectTransform"); return null; }

        if (_topLayer == null)
        {
            // 대충이라도 최상단: 현재 캔버스의 루트 RectTransform
            var canvas = GetComponentInParent<Canvas>();
            _topLayer = canvas ? canvas.GetComponent<RectTransform>() : (RectTransform)transform;
        }

        // 이미 있으면 재사용
        if (_ch1Clone) Destroy(_ch1Clone);

        // 1) 클론 생성
        _ch1Clone = Instantiate(originalGO, _topLayer);
        _ch1Clone.name = originalGO.name + "_TUTO_CLONE";

        // 2) 좌표/크기 복사 (스크린 기준으로 복사 -> parent가 달라도 맞게)
        var dst = _ch1Clone.GetComponent<RectTransform>();

        // dst를 "센터 기준"으로 고정해두면 기기별 삐뚤어짐이 줄어듦
        dst.anchorMin = dst.anchorMax = new Vector2(0.5f, 0.5f);
        dst.pivot = new Vector2(0.5f, 0.5f);

        Vector3 srcCenterWorld = src.TransformPoint(src.rect.center);
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, srcCenterWorld);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_topLayer, screen, null, out var local))
            dst.anchoredPosition = local;

        dst.sizeDelta = src.sizeDelta;

        // 회전/스케일도 필요하면
        dst.localRotation = Quaternion.identity;
        dst.localScale = src.localScale;;

        // 3) 최상단으로
        _topLayer.SetAsLastSibling();
        dst.SetAsLastSibling();

        // 4) 클릭/드래그 막고 싶으면 여기서 GraphicRaycaster / Button 비활성 등 조정 가능
        // 예: 원본의 버튼 이벤트는 제거하고 튜토용 이벤트만 넣기

        return _ch1Clone;
    }

    private IEnumerator WaitForUI(string nameContains, float timeout)
    {
        float t = 0f;
        while (t < timeout)
        {
            var go = FindByNameContains(nameContains); 
            if (go != null) yield break;

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        Debug.LogWarning($"[UITutorial] WaitForUI timeout: {nameContains}");
    }

    private GameObject FindByNameContains(string key)
    {
        // 비활성 포함 전체 탐색
        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < all.Length; i++)
        {
            var go = all[i];
            if (go == null) continue;

            // 에디터/프리팹 에셋 제외(런타임 씬 오브젝트만)
            if (!go.scene.IsValid()) continue;

            // key가 "ch1"이면 "ch1(Clone)"도 잡힘
            if (go.name == key || go.name.Contains(key))
                return go;
        }
        return null;
    }


    IEnumerator StartGuide1()
    {
        yield return new WaitForSeconds(1f);
        step = 0;
        Guideline[0].SetActive(true);
        Guide1();
    }
    public void Guide1()
    {
        preparent = menuBut.transform.parent.gameObject;
        presibling = menuBut.transform.GetSiblingIndex();
        Debug.Log("Guide1");
        menuBut.transform.SetParent(this.transform);
        menuBut.transform.SetAsLastSibling();
    }

    public void Guide2()
    {
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);
        menuBut.transform.SetParent(preparent.transform);
        menuBut.transform.SetSiblingIndex(presibling);
        preparent = progressBut.transform.parent.gameObject;
        progressBut.transform.SetParent(this.transform);
        progressBut.transform.SetAsLastSibling();
        Debug.Log("Guide2");

        _progressClicked = false;
        var btn = progressBut.GetComponent<Button>();
        if (btn)
        {
            btn.onClick.RemoveListener(OnClickProgressOnce);
            btn.onClick.AddListener(OnClickProgressOnce);
        }
    }

    private void OnClickProgressOnce()
    {
        _progressClicked = true;

        var btn = progressBut.GetComponent<Button>();
        if (btn) btn.onClick.RemoveListener(OnClickProgressOnce); // 1회성
    }

    public IEnumerator Guide3()
    {
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);

        progressBut.transform.SetParent(preparent.transform);
        progressBut.transform.SetSiblingIndex(presibling);

        progressUIController.tutorial = true;
        yield return StartCoroutine(WaitForUI("ch1", 1.0f));
        Canvas.ForceUpdateCanvases();

        var clone = CreateCh1CloneOnTop("ch1"); 
        if (clone != null)
        {
            var btn = clone.GetComponent<Button>();
            if (btn) 
            { 
                btn.onClick.RemoveAllListeners(); 
                btn.onClick.AddListener(OnClickCh1Clone); 
            }
        }
    }

    private void OnClickCh1Clone()
    {
        // Guide3에서 첫 클릭이면 -> Guide4로만
        if (!G4)
        {
            Guide4();
            G4 = true;
            return;
        }

        // Guide4에서 두 번째 클릭이면 -> 실제 기능 실행 + Guide5로 트리거
        progressUIController.onClickdragIcon(); // 기존 기능 실행(하루 진행도 열기 등)
        // onClickdragIcon 안에서 guide2 = true가 켜지니까 Update에서 Guide5로 넘어감
    }

    
    public void Guide4()
    {
        
        if (index == 2)
        {
            progressUIController.tutorial = false;
            Guideline[index].SetActive(false);
            index++;
            Guideline[index].SetActive(true);
        }
    }
    public void Guide5()
    {
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);
        if (_ch1Clone != null)
        {
            Destroy(_ch1Clone);
            _ch1Clone = null;
        }
        preparent = DayProgress.transform.parent.gameObject;
        DayProgress.transform.SetParent(this.transform);
        DayProgress.transform.SetAsLastSibling();

        if (Guideline[index] != null)
            Guideline[index].transform.SetAsLastSibling();
    }
    public void Guide6()
    {
        if (index >= 4 && index < 7)
        {
            Guideline[index].SetActive(false);
            index++;
            Guideline[index].SetActive(true);
            if (index == 6)
            {
                DayProgress.transform.SetParent(preparent.transform);
                DayProgress.transform.SetSiblingIndex(1);
                Subicon.transform.SetParent(this.transform);
                Subicon.transform.SetAsLastSibling();
            }
        }
    }
    public void Guide7() // 서브 바로보기 튜토 추가할 거면 여기 침습!
    {
        if (index >= 7 && index < transform.childCount)
        {
            if (index == 7)
            {
                Subicon.transform.SetParent(preparent.transform);
                Subicon.transform.SetSiblingIndex(3);
            }
            Guideline[index].SetActive(false);
            index++;
            if (index >= transform.childCount)
            {
                ScreenShield.Off();
                MenuController.moldOn();
                MenuController.nextandoff();
                this.gameObject.SetActive(false);
                // player.NextPhase();
            }
            else
            {
                Guideline[index].SetActive(true);
            }
            Debug.Log(index);
            MenuController.testskipoff();
        }
    }
    public void OnDisable()
    {
        MenuController.OnMenuOpened -= HandleMenuOpened;
        ScreenShield.Off();

        if (_ch1Clone != null)
        {
            Destroy(_ch1Clone);
            _ch1Clone = null;
        }
    }

    private void HandleMenuOpened()
    {
        if (step != 0) return;
        step = 1;
        Guide2();
        G2 = true;
    }
}
