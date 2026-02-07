using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    [SerializeField]
    GameObject MenuBut;
    [SerializeField]
    GameObject ExitBut;


    [SerializeField]
    GameObject Icon;

    [SerializeField]
    GameObject DayProgressUI;
    [SerializeField]
    GameObject MenuDefault;
    [SerializeField]
    GameObject Helper;
    [SerializeField]
    GameObject MyPageUI;
    [SerializeField]
    GameObject TimeUI;
    [SerializeField]
    GameObject Default;
    [SerializeField]
    GameObject checkList;
    [SerializeField]
    GameObject Replay;
    [SerializeField]
    GameObject tutoalert;


    [SerializeField]
    Animator MenuButAnim;

    public bool isOpening = false;
    public bool isprogress = false;
    TranslateManager translator;

    [SerializeField]
    TMP_Text[] menu;
    [SerializeField]
    TMP_Text[] mypage;
    [SerializeField]
    TMP_Text[] community;
    [SerializeField]
    TMP_Text[] credit;
    [SerializeField]
    TMP_Text[] progress;

    [SerializeField]
    ObjectManager objectManager;

    [SerializeField]
    PlayerController PlayerController;

    private int phase;

    public bool UItutoEnd = false;

    public static event Action OnMenuOpened;
    private void Start()
    {
        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();

        translator.translatorDel += Translate;


        objectManager.activeSystemUIDelegate += CallbackActiveSystemUI;
    }
    public void CallbackActiveSystemUI(bool InActive)
    {
        if (GameManager.isend)
        {
            gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(InActive);
    }

    public void OnEnable()
    {
        isOpening = false;
        isprogress = false;
        if (PlayerController && PlayerController.GetChapter() >= 2)
        {
            tuto();
        }
    }

    public void Translate(LANGUAGE language, TMP_FontAsset font)
    {
        //번역한다.
        Debug.Log("Menu 번역합니다.\n");

        int Idx = (int)language;

        //타이틀
        menu[0].text = DataManager.Instance.Settings.menu.title[Idx];

        //아이콘
        menu[1].text = DataManager.Instance.Settings.menu.howto[Idx];
        menu[2].text = DataManager.Instance.Settings.menu.progress[Idx];
        menu[3].text = DataManager.Instance.Settings.menu.mypage[Idx];

        //타이틀 
        mypage[0].text = DataManager.Instance.Settings.menuMyPage.settings[Idx];

        //이름
        mypage[1].text = DataManager.Instance.Settings.settings.name[Idx];
        //BGM
        mypage[2].text = DataManager.Instance.Settings.settings.BGM[Idx];
        //SFX
        mypage[3].text = DataManager.Instance.Settings.settings.SFX[Idx];
        //Alert
        mypage[4].text = DataManager.Instance.Settings.settings.alert[Idx];
        //namechanged
        mypage[5].text = DataManager.Instance.Settings.settings.namechanged[Idx];
        //namesetting
        mypage[6].text = DataManager.Instance.Settings.settings.namesetting.title[Idx];
        mypage[7].text = DataManager.Instance.Settings.settings.namesetting.placeholder[Idx];
        mypage[8].text = DataManager.Instance.Settings.settings.namesetting.no[Idx];
        mypage[9].text = DataManager.Instance.Settings.settings.namesetting.yes[Idx];
        //push off
        mypage[10].text = DataManager.Instance.Settings.settings.pushoff.title[Idx];
        mypage[11].text = DataManager.Instance.Settings.settings.pushoff.no[Idx];
        mypage[12].text = DataManager.Instance.Settings.settings.pushoff.yes[Idx];

        //Language
        mypage[13].text = DataManager.Instance.Settings.settings.language[Idx];

        //타이틀
        community[0].text = DataManager.Instance.Settings.menuMyPage.community[Idx];

        //instagram
        community[1].text = DataManager.Instance.Settings.community.instagram[Idx];
        //discord
        community[2].text = DataManager.Instance.Settings.community.discord[Idx];
        //discord tip
        community[3].text = DataManager.Instance.Settings.community.discordTip[Idx];
        //x
        community[4].text = DataManager.Instance.Settings.community.x[Idx];

        //타이틀
        credit[0].text = DataManager.Instance.Settings.menuMyPage.credit[Idx];


        //watching
        progress[0].text = DataManager.Instance.Settings.checklist.phase1[Idx];
        progress[1].text = DataManager.Instance.Settings.checklist.phase2[Idx];
        progress[2].text = DataManager.Instance.Settings.checklist.phase3[Idx];
        progress[3].text = DataManager.Instance.Settings.checklist.phase4[Idx];


        //for (int i = 0; i < menu.Length; i++)
        //{
        //    menu[i].font = font;
        //}

        //for (int i = 0; i < mypage.Length; i++)
        //{
        //    mypage[i].font = font;
        //}

        //for(int i=0;i<community.Length; i++)
        //{
        //    community[i].font = font;
        //}

        //for(int i=0;i<credit.Length; i++)
        //{
        //    credit[i].font = font;
        //}

        //for(int i=0;i<progress.Length; i++)
        //{
        //    progress[i].font = font;
        //}
    }

    public void onMenu()
    {
        InputGuard.WorldInputLocked = true;
        phase = PlayerController.GetCurrentPhase();
        MenuBut.GetComponent<Button>().enabled = false;
        isOpening = !isOpening;
        MenuButAnim.SetFloat("speed", 1f);
        if (isOpening)
        {
            TimeUI.SetActive(false);
            checkList.SetActive(false);
            MenuDefault.SetActive(true);
            MenuButAnim.SetBool("isDowning", true);
            ExitBut.GetComponent<Button>().enabled = true;
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.MenuOn, this.transform.position);
            OnMenuOpened?.Invoke(); // 튜토 트리거
        }
        else
        {
            MenuDefault.transform.GetChild(1).gameObject.SetActive(false);
            MenuButAnim.SetBool("isDowning", false);
        }
    }

    private void OnDisable()
    {
        // 휘발성 상태는 끊어준다 (튜토/트리거 계열)
        isOpening = false;
        isprogress = false;
    }
    public void offMenu()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        if(ExitBut != null)
        {
            ExitBut.GetComponent<Button>().enabled = false;
            onMenu();
        }
    }

    public void MenuoffExit()
    {
        //if (!SkipController.is_end)
        //    TimeUI.SetActive(true);
        MenuAniExit();
    }
    public void MenuAniExit()
    {
        InputGuard.WorldInputLocked = false;
        MenuBut.GetComponent<Button>().enabled = true;
        if (isOpening)
        {
            MenuDefault.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            if (phase == 1 || phase == 3 || phase == 5)
            {
                checkList.SetActive(true);
                MenuDefault.SetActive(false);
                TimeUI.SetActive(false);
            }
            else
            {
                TimeUI.SetActive(true);
                checkList.SetActive(true);
                MenuDefault.SetActive(false);
            }
        }
        ApplyMoldGate();
        ApplyEndingOverride();
    }

    private void ApplyMoldGate()
    {
        var gm = FindObjectOfType<GameManager>(true); // 최소: 필요할 때만 찾기
        if (gm == null || PlayerController == null) return;

        bool shouldMold =
            gm.Chapter == 1 &&
            gm.Pattern == GamePatternState.Thinking &&
            PlayerController.GetSubseq() == 1;

        if (shouldMold) moldOn();
    }

    public void onDayProgressUI()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.iconClick, this.transform.position);
        //DayProgressUI on,.,
        DayProgressUI.SetActive(true);
        DayProgressUI.GetComponent<ProgressUIController>().RefreshProgressUI();
        isprogress = true;
    }

    public void tutoProgressUI()
    {
        if (tutoalert.activeSelf == false)
        {
            tutoalert.SetActive(true);
            StartCoroutine(CloseAlter(tutoalert));
        }
        else
            tutoalert.SetActive(false);
    }

    public void onClickHelper()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.iconClick, this.transform.position);
        Helper.SetActive(true);
    }

    public void onClickMypage()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.iconClick, this.transform.position);
        MyPageUI.SetActive(true);
    }

    IEnumerator CloseAlter(GameObject checkList)
    {
        yield return new WaitForSeconds(2f);
        checkList.SetActive(false);
    }
    public void onClickCheckListIcon()
    {
        if (checkList.activeSelf == false)
        {
            checkList.SetActive(true);
            StartCoroutine(CloseAlter(checkList));
        }
        else
            checkList.SetActive(false);
    }

    public void onlyskipoff()
    {
        Default.SetActive(true);
        TimeUI.SetActive(false);
    }
    public void skipoff()
    {
        Default.SetActive(false);
        TimeUI.SetActive(false);
    }

    public void skipon()
    {
        Default.SetActive(true);
        TimeUI.SetActive(true);
    }
    public void replayON()
    {
        TimeUI.SetActive(false);
        MenuBut.GetComponent<Button>().enabled = true;
        if (checkList) checkList.SetActive(false);
        if (Default) Default.SetActive(true);
        Replay.SetActive(true);
    }
    public void tuto()
    {
        if (!MenuButAnim)
            MenuButAnim = GetComponent<Animator>();

        if (!MenuButAnim)
        {
            Debug.LogError("[MenuController] Animator(MenuButAnim) missing on this GameObject. tuto() skipped.");
            return;
        }
        MenuButAnim.SetFloat("tuto", 1f);
    }
    public void LaterON()
    {
        StartCoroutine(later());
    }

    public void alloff()
    {
        TimeUI.SetActive(false);
        Default.SetActive(false);
        checkList.SetActive(false);
        ApplyEndingOverride();
    }

    public void allon()
    {
        TimeUI.SetActive(true);
        Default.SetActive(true);
        checkList.SetActive(true);
        tuto();
        ApplyEndingOverride();
    }

    public void moldOn()
    {
        Debug.Log("스킵버튼");
        TimeUI.SetActive(false);
        Default.SetActive(true);
        checkList.SetActive(true);
        tuto();
    }
    
    public void nextandoff()
    {
        UItutoEnd = true;
        DayProgressUI.GetComponent<ProgressUIController>().exit();
        DayProgressUI.GetComponent<ProgressUIController>().exit();
        offMenu();
        //PlayerController.NextPhase();
    }
    IEnumerator later()
    {
        yield return new WaitForSeconds(2.5f);
        skipon();
        GameObject.FindWithTag("GameController").GetComponent<SubTuto>().skiptouchGuide();
    }
    IEnumerator later2()
    {
        ScrollManager camera = GameObject.FindWithTag("MainCamera").GetComponent<ScrollManager>();
        camera.stopscroll();
        camera.MoveCamera(new UnityEngine.Vector3(-5.5f, 0, -10), 2.5f);
        tuto();
        yield return new WaitForSeconds(2.5f);
        skipon();
        GameObject.FindWithTag("GameController").GetComponent<SubTuto>().skiptouchGuide();
    }

    // 보이게 하는 단계 (종이 날아갈 때)
    public void SetMenuButtonVisible(bool visible)
    {
        if (MenuBut != null)
            MenuBut.SetActive(visible);
    }

    // 클릭 가능 여부 전용 (tuto-watching 진입 이후)
    public void SetMenuButtonInteractable(bool canClick)
    {
        if (MenuBut != null)
            MenuBut.GetComponent<Button>().interactable = canClick;
    }

    public void tutonum2laterON()
    {
        StartCoroutine(later2());
    }

    public void testskipoff()
    {
        TimeUI.SetActive(false);
    }

    public void ApplyEndingOverride()
    {
        if (!GameManager.isend) return;

        bool post = DeathNoteClick.readDeathnote;

        // 공통: 엔딩에서는 필요없는 UI는 항상 off
        if (TimeUI) TimeUI.SetActive(false);
        if (checkList) checkList.SetActive(false);

        if (!post)
        {
            // Pre: 유서 전 → 아무 버튼도 안 보이게
            if (MenuBut) MenuBut.SetActive(false);
            if (Replay) Replay.SetActive(false);
            if (Default) Default.SetActive(false);
            isOpening = false;
            return;
        }

        // Post: 유서 후 → 메뉴/리플레이 유지
        if (Default) Default.SetActive(true);
        if (MenuBut) MenuBut.SetActive(true);
        if (Replay) Replay.SetActive(true);
    }







}
