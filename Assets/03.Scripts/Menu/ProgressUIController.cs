using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class ProgressUIController : MonoBehaviour
{ 
    [SerializeField]
    GameObject dragScroller; //스크롤러의 크기를 조절 예정

    //현재 생성된 개수를 알아야함 
    PlayerController player;
    //임시 타이틀 배열
    #region 상세 팝업을 위한 변수
    [SerializeField]
    GameObject day_progress;
    #endregion


    [SerializeField]
    GameObject alter;
    [SerializeField]
    GameObject detailed_popup;

    [SerializeField]
    Alertmanager alertmanager;
    [SerializeField]
    GameObject dragIconPrefab;
    [SerializeField]
    Dictionary<int,GameObject> dragIconList;

    [SerializeField]
    DotController dotController;
    int curChapter = 1;
    float iconWidth = 0;
    public bool tutorial = false;
    public bool guide1 = false;
    public bool guide2 = false;

    [Tooltip("Init Rect Size(width,height)")]
    Vector2 InitScrollSize;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        dragIconList = new Dictionary<int, GameObject>();
        iconWidth = dragIconPrefab.GetComponent<RectTransform>().rect.width;
        InitScrollSize = new Vector2(dragScroller.GetComponent<RectTransform>().rect.width, dragScroller.GetComponent<RectTransform>().rect.height);
        dragScroller.GetComponent<ScrollRect>().onValueChanged.AddListener(Scroll);
        //dotController = GameObject.FindWithTag("DotController").GetComponent<DotController>();
    }

    private void OnEnable()
    {
        dragScroller.GetComponent<ScrollRect>().horizontalNormalizedPosition = 0f;
    }
    void Start()
    {
        /*Icon 14개를 모두 생성 및 초기화.*/
        InstantiateDragIcon();
    }

    /*Progress 챕터에 의해 활성화된다.*/
    void SetActiveDragIcon(int chapter)
    {
        if(dragIconList.ContainsKey(chapter) == false) { return; }
        //Chapter Update
        curChapter = chapter;

        ChapterInfo info = DataManager.Instance.ChapterList.chapters[chapter];
        dragIconList[chapter].SetActive(true);

        if (alertmanager != null && alertmanager.isAlert)
        {
            Debug.Log("되냐?");
            alertmanager.ChapterAlert = dragIconList[chapter].GetComponent<DragIcon>().RedAlert;
            alertmanager.openChapter();
        }
        //Lock을 해제한다.
        foreach (var progress in dragIconList)
        {
            if (progress.Key <= chapter && !dotController.tutorial)
            {
                progress.Value.GetComponent<DragIcon>().DestoryLock();
            }
        }

        if(curChapter<3)
        {
            for (int i = 1; i <= 3; i++)
            {
                dragIconList[curChapter + i].SetActive(true);
            }
        }
        else if(curChapter<14)
        {
            dragIconList[curChapter + 1].SetActive(true);
        }

        //기본 3개는 미리 보여주는 영역이기 때문에, 플레이어 챕터로부터 -3을 빼게 된다.
        int sizeChapter = Mathf.Clamp(curChapter - 3, 0, 15);
        dragScroller.GetComponent<RectTransform>().sizeDelta = new Vector2(InitScrollSize.x + sizeChapter * iconWidth, InitScrollSize.y);
    }

    void InstantiateDragIcon()
    {
        for (int i=1;i<=14;i++)
        {
            ChapterInfo info = DataManager.Instance.ChapterList.chapters[i];
            GameObject icon = Instantiate(dragIconPrefab,dragScroller.transform.GetChild(0));
            icon.name = info.chapter;
            dragIconList.Add(i,icon);

            DragIcon curIconScript = icon.GetComponent<DragIcon>();
            /*모든 상태를 업데이트 한다.*/
            curIconScript.Settings(i,info,player.GetLanguage());
            icon.GetComponent<Button>().onClick.AddListener(onClickdragIcon);

            if (i > player.GetChapter())
            {
                icon.SetActive(false);
            }
        }

        SetActiveDragIcon(player.GetChapter()); //재사용
    }

    public void onClickdragIcon()
    {
        guide1 = true;
        GameObject day = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (day.GetComponent<DragIcon>().isLocking())
        {
            alter.SetActive(true);
        }
        else if (!tutorial)
        {
            dragScroller.transform.parent.gameObject.SetActive(false); //메인 다이얼로그 진행
            detailed_popup.SetActive(true); //서브 다이얼로그 설정(진행바)
            //정규식을 사용해서 문자열 내에 있는 숫자 찾기
            int findChapter = int.Parse(Regex.Replace(day.name, @"\D", ""));

            ChapterInfo info = DataManager.Instance.ChapterList.chapters[findChapter];
            detailed_popup.GetComponent<ChapterProgressManager>().PassData(info, player);
            guide2 = true;
        }
    }

    public void Scroll(Vector2 value)
    {
        if(value.x>=1f)
        {
            alter.SetActive(true);
        }
    }

    public void canceled(){
        alter.SetActive(false);
    }

    public void exit()
    {
        //현재 게임 오브젝트가 DayProgress_Default이면, DayProgressUI SetActive한다.

        if (detailed_popup.activeSelf)
        {
            detailed_popup.SetActive(false);
            dragScroller.transform.parent.gameObject.SetActive(true);
        }
        else
        {
           //menu_default.gameObject.SetActive(true);
            this.gameObject.SetActive(false);
        }

    }
}
