using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubTutorial : MonoBehaviour
{
    public List<GameObject> Guideline = new List<GameObject>();
    [SerializeField] MenuController MenuController;
    [SerializeField] GameObject menuBut;
    [SerializeField] GameObject progressBut;
    [SerializeField] ProgressUIController progressUIController;
    [SerializeField] GameObject icon;
    [SerializeField] GameObject DayProgress;
    [SerializeField] GameObject Subicon;
    [SerializeField] GameManager gameManager;

    [SerializeField] private GameObject guide0BubbleRoot;
    [SerializeField] private Button guide0Button;
    [SerializeField] private TextMeshProUGUI guide0Text;

    [SerializeField] private RectTransform _topLayer; // Canvas 최상단 패널
    private GameObject _ch1Clone;
    CanvasGroup tutorialMaskGroup;
    CanvasGroup Spider;
    CanvasGroup Progress;
    GameObject preparent;
    Vector3 originalPosition;


    int presibling;
    private Coroutine guide0UnlockRoutine;
    private bool G2 = false;
    private bool G3 = false;
    private bool G4 = false;
    private bool G5 = false;
    private bool G6 = false;
    int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        MenuController.isprogress = false;
        progressUIController.guide1 = false;
        progressUIController.guide2 = false;
        ScreenShield.On();
        subtutoinit();
        index = 0;

        Guideline.RemoveAll(g => g == null);

        for (int i = 0; i < Guideline.Count; i++)
        {
            Guideline[i].SetActive(false);
        }
        tutorialMaskGroup = this.GetComponent<CanvasGroup>();
        Spider = menuBut.GetComponent<CanvasGroup>();
        Progress = progressBut.GetComponent<CanvasGroup>();
    }
    private void Update()
    {
        if (!G2 && MenuController.isOpening)
        {
            Guide2();
            G2 = true;  // �� �� ����� �Ŀ��� ����
        }

        if (!G3 && MenuController.isprogress)
        {
            StartCoroutine(Guide3());
            G3 = true;
            MenuController.isprogress = false;
        }

        if (!G5 && progressUIController.guide2)
        {
            Guide5();
            G5 = true;
            progressUIController.guide2 = false;
        }
    }
    public void subtutoinit()
    {
        G2 = false;
        G3 = false;
        G4 = false;
        G5 = false;
        G6 = false;
    }

    public void guidestart()
    {
        this.GetComponent<Image>().enabled = false;
        Guide0();
    }
    public IEnumerator guide()
    {
        yield return new WaitForSeconds(1f);
        Guide0();
    }
    public void Guide0() // 앙증맞은 폴라로이드네
    {
        index = 0;
        InputGuard.WorldInputLocked = true; 
        
        GameObject step0 = Guideline[0];
        step0.SetActive(true);

        // CanvasGroup 확보
        CanvasGroup cg = step0.GetComponent<CanvasGroup>();
        if (cg == null) cg = step0.AddComponent<CanvasGroup>();

        // Button 확보
        Button b = step0.GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(Guide1);

        // 연출 - 딜레이 후 클릭 가능
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        b.interactable = false;

        if (guide0UnlockRoutine != null)
            StopCoroutine(guide0UnlockRoutine);

        StartCoroutine(FadeInGuide0(cg, b, 0.6f));
    }
    private IEnumerator FadeInGuide0(CanvasGroup cg, Button b, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / duration);
            yield return null;
        }

        // 완전히 보인 뒤에만 클릭 가능
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        b.interactable = true;
    }

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

        // 회전/스케일
        dst.localRotation = Quaternion.identity;
        dst.localScale = src.localScale;

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

            // key가 "ch1"이면 "ch1(Clone)"도
            if (go.name == key || go.name.Contains(key))
                return go;
        }
        return null;
    }

    public void Guide1() // 다시 이쪽으로 이쪽으로
    {
        var btn = menuBut.GetComponent<Button>();
        if (btn != null)
            btn.interactable = true;
        // Guide0(폴라로이드) 끄고 다음 단계로
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);

        this.GetComponent<Image>().enabled = true;
        this.GetComponent<Button>().interactable = true;

        preparent = menuBut.transform.parent.gameObject;
        presibling = 1;
        Debug.Log("Guide1");
        menuBut.transform.SetParent(this.transform);
        menuBut.transform.SetAsLastSibling();
    }


    public void Guide2() // 여기다 여기
    {
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);
        menuBut.transform.SetParent(preparent.transform);
        menuBut.transform.SetSiblingIndex(presibling);
        preparent = progressBut.transform.parent.gameObject;
        progressBut.transform.SetParent(this.transform);
        progressBut.transform.SetAsLastSibling();
        G2 = true;
        Debug.Log("Guide2");
    }

    public IEnumerator Guide3()
    {
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);

        progressBut.transform.SetParent(preparent.transform);
        progressBut.transform.SetSiblingIndex(presibling);

        progressUIController.tutorial = false;
        yield return new WaitForSeconds(0.1f);

        Canvas.ForceUpdateCanvases();

        var clone = CreateCh1CloneOnTop("ch1"); 
        if (clone != null)
        {
            var btn = clone.GetComponent<Button>();
            if (btn) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(progressUIController.onClickdragIcon); }
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

        preparent = Subicon.transform.parent.gameObject;
        Subicon.transform.SetParent(this.transform);
        Subicon.transform.SetAsLastSibling();
        Debug.Log("Guide5");
    }

    public void Guide6()
    {
        if (index >= 4)  
        {
            if (index < Guideline.Count)
            {
                Guideline[index].SetActive(false);  
            }

            index++; 

            if (index < Guideline.Count) 
            {
                Guideline[index].SetActive(true);
            }
            if (index >= Guideline.Count)
            {
                Subicon.transform.SetParent(preparent.transform);
                Subicon.transform.SetSiblingIndex(3);
                ScreenShield.Off();
                SubDialogue.isSubmoldtutoend = true;
                InputGuard.WorldInputLocked = false; 
                this.gameObject.SetActive(false);
                var menu = gameManager?.Menu ?? FindObjectOfType<MenuController>(true);
                menu?.allon();
                return;
            }
            Debug.Log(index);
        }
    }
}
