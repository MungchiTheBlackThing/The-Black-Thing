using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Localization.Editor;
using UnityEditor.Localization.Plugins.XLIFF.V20;
#endif

using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;


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

    private string _stringTableName = "WatchingNoteUI";

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        dragIconList = new Dictionary<int, GameObject>();
        iconWidth = dragIconPrefab.GetComponent<RectTransform>().rect.width;
        InitScrollSize = new Vector2(dragScroller.GetComponent<RectTransform>().rect.width, dragScroller.GetComponent<RectTransform>().rect.height);
        //dragScroller.GetComponent<ScrollRect>().onValueChanged.AddListener(Scroll);
        //dotController = GameObject.FindWithTag("DotController").GetComponent<DotController>();
    }

    private void OnEnable()
    {
        Debug.Log("[ProgressUIController] OnEnable 호출됨");
        dragScroller.GetComponent<ScrollRect>().horizontalNormalizedPosition = 0f;

        // 아이콘이 이미 생성된 경우(재활성화 시) UI를 갱신합니다.
        if (dragIconList != null && dragIconList.Count > 0)
        {
            if (player == null)
                player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

            int currentChapter = player.GetChapter();
            Debug.Log($"[ProgressUIController] UI 활성화됨. 현재 챕터: {currentChapter}, UI 갱신");

            // 언어 설정 등에 따른 텍스트/이미지 갱신
            RefreshProgressUI();
            
            // 챕터 진행 상황에 따른 아이콘 활성화 및 잠금 해제 처리
            SetActiveDragIcon(currentChapter);
        }
        else
        {
            Debug.Log("[ProgressUIController] 아이콘 리스트가 비어있음");
        }
    }
    void Start()
    {
        /*Icon 14개를 모두 생성 및 초기화.*/
        InstantiateDragIcon();
    }

    /*Progress 챕터에 의해 활성화된다.*/
    void SetActiveDragIcon(int chapter)
    {

        if (dragIconList.ContainsKey(chapter) == false) 
        { 
            Debug.LogError($"[ProgressUIController] 챕터 {chapter}에 해당하는 아이콘 없음");
            return; 
        }
        //Chapter Update
        curChapter = chapter;

        ChapterInfo info = DataManager.Instance.ChapterList.chapters[chapter];
        dragIconList[chapter].SetActive(true);

        if (alertmanager != null && alertmanager.isAlert)
        {
            Debug.Log("[ProgressUIController] AlertManager 활성화 상태, 챕터 알림 설정");
            alertmanager.ChapterAlert = dragIconList[chapter].GetComponent<DragIcon>().RedAlert;
            alertmanager.openChapter();
        }
        //Lock을 해제한다.
        int unlockedCount = 0;
        foreach (var progress in dragIconList)
        {
            if (progress.Key <= chapter)
            {
                progress.Value.GetComponent<DragIcon>().DestoryLock();
                unlockedCount++;
            }
        }
        Debug.Log($"[ProgressUIController] {unlockedCount}개의 챕터 아이콘 잠금 해제 완료");

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
        Debug.Log("[ProgressUIController] InstantiateDragIcon: 아이콘 생성");
        for (int i = 1; i <= 14; i++)
        {
            ChapterInfo info = DataManager.Instance.ChapterList.chapters[i];
            GameObject icon = Instantiate(dragIconPrefab, dragScroller.transform.GetChild(0));
            icon.name = info.chapter;
            dragIconList.Add(i, icon);

            DragIcon curIconScript = icon.GetComponent<DragIcon>();
            /*모든 상태를 업데이트 한다.*/

            curIconScript.Settings(i, info, player.GetLanguage());

            icon.GetComponent<Button>().onClick.AddListener(onClickdragIcon);

            if (i > player.GetChapter())
            {
                icon.SetActive(false);
            }
        }

        SetActiveDragIcon(player.GetChapter()); //재사용
    }

    public void RefreshProgressUI()
    {
        Debug.Log("[ProgressUIController] RefreshProgressUI: 모든 아이콘 설정 갱신");
        foreach (var icon in dragIconList)
        {
            DragIcon curIconScript = icon.Value.GetComponent<DragIcon>();
            ChapterInfo info = DataManager.Instance.ChapterList.chapters[icon.Key];
            curIconScript.Settings(icon.Key, info, player.GetLanguage());
        }
    }

    public void onClickdragIcon()
    {
        guide1 = true;
        GameObject day = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (day.GetComponent<DragIcon>().isLocking())
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.lockClick, this.transform.position);
            alter.SetActive(true);
        }
        else if (!tutorial)
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.iconClick, this.transform.position);
            dragScroller.transform.parent.gameObject.SetActive(false); //메인 다이얼로그 진행
            detailed_popup.SetActive(true); //서브 다이얼로그 설정(진행바)
            //정규식을 사용해서 문자열 내에 있는 숫자 찾기
            int findChapter = int.Parse(Regex.Replace(day.name, @"\D", ""));

            ChapterInfo info = DataManager.Instance.ChapterList.chapters[findChapter];
            detailed_popup.GetComponent<ChapterProgressManager>().PassData(info, player);
            guide2 = true;
        }
    }

    //public void Scroll(Vector2 value)
    //{
    //    if(value.x>=1f)
    //    {
    //        alter.SetActive(true);
    //    }
    //}

    public void canceled(){
        alter.SetActive(false);
    }

    public void exit()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
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
