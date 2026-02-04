using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private GameObject TutoCh1Object;

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

        if (!G3 && G2 && MenuController.isprogress)
        {
            StartCoroutine(Guide3());
            G3 = true;
            MenuController.isprogress = false;
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
        presibling = 1;
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
    }

    public IEnumerator Guide3()
    {
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);

        progressBut.transform.SetParent(preparent.transform);
        progressBut.transform.SetSiblingIndex(presibling);

        progressUIController.tutorial = true;
        yield return new WaitForSeconds(0.1f);

        TutoCh1Object.SetActive(true);
        TutoCh1Object.transform.SetAsLastSibling();
        TutoCh1Object.GetComponent<Button>().onClick.AddListener(progressUIController.onClickdragIcon);
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
        TutoCh1Object.SetActive(false);
        preparent = DayProgress.transform.parent.gameObject;
        DayProgress.transform.SetParent(this.transform);
        DayProgress.transform.SetAsLastSibling();
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
    }

    private void HandleMenuOpened()
    {
        if (step != 0) return;
        step = 1;
        Guide2();
        G2 = true;
    }
}
