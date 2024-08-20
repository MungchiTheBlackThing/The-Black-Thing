using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    /*Menu ����*/
    #region Chapter ������ ���� ����,���� �Ѱ��� �����ؾ��ϰ�, �����ؼ� �ȵȴ�.
    public static Chapters chapterList;
    #endregion

    [SerializeField]
    GameObject MenuBut;
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
    GameObject Replay;
    #region é�� ����
    [SerializeField]
    GameObject checkList;

    [SerializeField]
    GameObject dragIcon;
    public static Dictionary<int, GameObject> prograssUI; //prograss UI�� ��ü ������ ���� 

    [SerializeField]
    GameObject dragScroller;
    float dragScrollWidth = 0.0f;
    #endregion
    void Start()
    {
        prograssUI = new Dictionary<int, GameObject>();
        //������ �ε� 
        var loadedJson = Resources.Load<TextAsset>("Json/Chapters");

        if (loadedJson)
        {
            chapterList = JsonUtility.FromJson<Chapters>(loadedJson.ToString());
        }
    }

    public void onMenu()
    {
        if (!Icon.activeSelf)
        {
            TimeUI.SetActive(false);
            //checklist�� �θ�
            checkList.transform.parent.gameObject.SetActive(false);
            Icon.transform.parent.gameObject.SetActive(true);
            this.gameObject.GetComponent<Animator>().SetBool("isDowning", false);
        }
        else
        {
            Icon.SetActive(false);
            this.gameObject.GetComponent<Animator>().SetBool("isDowning", true);
        }
    }

    public void offMenu()
    {
        if (Icon.activeSelf)
        {
            this.gameObject.GetComponent<Animator>().SetBool("isDowning", true);
            Icon.SetActive(false);
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
        Icon.SetActive(true);
    }
    public void onDayProgressUI()
    {
        //DayProgressUI on,.,
        DayProgressUI.SetActive(true);
        MenuDefault.SetActive(false);

    }

    public void onClickHelper()
    {
        Helper.SetActive(true);
        MenuDefault.SetActive(false);
    }

    public void onClickMypage()
    {
        MyPageUI.SetActive(true);
        MenuDefault.SetActive(false);
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
        Debug.Log("��");
        Default.SetActive(false);
        TimeUI.SetActive(false);
    }

    public void skipon()
    {
        Debug.Log("��");
        Default.SetActive(true);
        TimeUI.SetActive(true);
    }
    public void replayON()
    {
        TimeUI.SetActive(false);
        Replay.SetActive(true);
    }

    public void OnUpdatedProgress(int chapter)
    {
        dragScrollWidth = dragScroller.GetComponent<RectTransform>().rect.width; //������ġ?
        //chapter�� �´� Dictionary ������ ���⼭ �ϰ�, ProgressUIController�� �װ� SetActive�ϴ� �뵵�� �������.
        //1~4���� =>�ʼ� 
        for (int i = 1; i <= 3; i++)
        {
            if (prograssUI.ContainsKey(i) == false)
            {
                GameObject icon = Instantiate(dragIcon, dragScroller.transform.GetChild(0));
                icon.name = chapterList.chapters[i].chapter;
                DragIcon curIconScript = icon.GetComponent<DragIcon>();
                curIconScript.Settings(chapterList.chapters[i].id, chapterList.chapters[i].title, chapterList.chapters[i].mainFilePath, "���� Ÿ��Ʋ �ּ���");
                prograssUI[i] = icon;
                icon.GetComponent<Button>().onClick.AddListener(DayProgressUI.GetComponent<ProgressUIController>().onClickdragIcon);

                //ProgressBar�� ���� ������ ����.
                dragScroller.GetComponent<RectTransform>().sizeDelta = new Vector2(dragScroller.GetComponent<RectTransform>().rect.width, dragScroller.GetComponent<RectTransform>().rect.height);
            }
        }

        //5����~14�ϱ���
        for (int i = 4; i <= chapter + 1; i++)
        {
            if (i >= 15) continue;
            if (prograssUI.ContainsKey(i) == false)
            {
                GameObject icon = Instantiate(dragIcon, dragScroller.transform.GetChild(0));
                icon.name = chapterList.chapters[i].chapter;
                DragIcon curIconScript = icon.GetComponent<DragIcon>();
                curIconScript.Settings(chapterList.chapters[i].id, chapterList.chapters[i].title, chapterList.chapters[i].mainFilePath, "���� Ÿ��Ʋ �ּ���");
                prograssUI[i] = icon;
                icon.GetComponent<Button>().onClick.AddListener(DayProgressUI.GetComponent<ProgressUIController>().onClickdragIcon);

                //ProgressBar�� ���� ������ ����.

                dragScroller.GetComponent<RectTransform>().sizeDelta = new Vector2(dragScroller.GetComponent<RectTransform>().rect.width + dragIcon.GetComponent<RectTransform>().rect.width, dragScroller.GetComponent<RectTransform>().rect.height);
            }
        }

        foreach (var progress in prograssUI)
        {
            if (progress.Key <= chapter)
            {
                progress.Value.GetComponent<DragIcon>().DestoryLock();
            }
        }
        float val = (chapter * dragIcon.GetComponent<RectTransform>().rect.width) / (dragScroller.GetComponent<ScrollRect>().content.rect.width - dragScrollWidth);
        //�߾� ��ġ ���
        dragScroller.GetComponent<ScrollRect>().horizontalNormalizedPosition = (val / val) - 0.2f; //�ΰ��� 1�� �������, 0.2f�� �ؼ� �˸��ߴ� ������ �ذ�
    }
}
