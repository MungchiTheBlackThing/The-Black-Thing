using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Device;
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

    public static event Action OnMenuOpened;
    private void Start()
    {
        MenuButAnim = GetComponent<Animator>();
        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();

        translator.translatorDel += Translate;


        objectManager.activeSystemUIDelegate += CallbackActiveSystemUI;
    }
    public void CallbackActiveSystemUI(bool InActive)
    {
        this.gameObject.SetActive(InActive);
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


        for (int i = 0; i < menu.Length; i++)
        {
            menu[i].font = font;
        }

        for (int i = 0; i < mypage.Length; i++)
        {
            mypage[i].font = font;
        }

        for(int i=0;i<community.Length; i++)
        {
            community[i].font = font;
        }

        for(int i=0;i<credit.Length; i++)
        {
            credit[i].font = font;
        }

        for(int i=0;i<progress.Length; i++)
        {
            progress[i].font = font;
        }
    }

    public void onMenu()
    {
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
        }
        else
        {
            MenuDefault.transform.GetChild(1).gameObject.SetActive(false);
            MenuButAnim.SetBool("isDowning", false);
        }
    }

    public void offMenu()
    {
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
        checkList.transform.parent.gameObject.SetActive(true);
        Icon.transform.parent.gameObject.SetActive(false);
    }
    public void MenuAniExit()
    {
        MenuBut.GetComponent<Button>().enabled = true;
        if (isOpening)
        {
            MenuDefault.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            TimeUI.SetActive(true);
            checkList.SetActive(true);
            MenuDefault.SetActive(false);
        }
    }
    public void onDayProgressUI()
    {
        //DayProgressUI on,.,
        DayProgressUI.SetActive(true);
        isprogress = true;
    }

    public void onClickHelper()
    {
        Helper.SetActive(true);
    }

    public void onClickMypage()
    {
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
    }
    public void tuto()
    {
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
    }

    public void allon()
    {
        TimeUI.SetActive(true);
        Default.SetActive(true);
        checkList.SetActive(true);
        tuto();
    }
    IEnumerator later()
    {
        yield return new WaitForSeconds(2.5f);
        skipon();
        GameObject.FindWithTag("GameController").GetComponent<SubTuto>().skiptouchGuide();
    }
}
