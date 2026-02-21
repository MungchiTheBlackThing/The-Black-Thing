using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BinocularController : BaseObject , IWatchingInterface
{
    [SerializeField]
    EWatching type;

    [SerializeField]
    GameObject alert;
   
    [SerializeField]
    List<GameObject> watching;
    
    int chapterIdx; //-1�� �ٲ������.

    GameObject watchingBackground;
    GameObject phase;
    PlayerController pc;
    DoorController door;
    Dictionary<int,int> Idx = new Dictionary<int,int>();

    private Collider2D col2D;
    private int lastChapterChecked = int.MinValue;
    public bool IsCurrentPattern(EWatching curPattern)
    {
        return curPattern == type;
    }

    private bool IsValidBinoDay(int chapter)
    {
        return chapter == 2 || chapter == 5 || chapter == 8 || chapter == 10 || chapter == 13;
    }

    private void Start()
    {
        door = FindObjectOfType<DoorController>();
        var phaseObj = GameObject.Find("Phase");
        if (phaseObj != null) watchingBackground = phaseObj;

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) pc = playerObj.GetComponent<PlayerController>();

        StartCoroutine(InitWhenReady());
        col2D = GetComponent<Collider2D>();
        RefreshBinocularInteractable(force: true);
    }

    private IEnumerator InitWhenReady()
    {
        // DataManager 준비될 때까지 대기 (최대 2초 정도)
        float timeout = 2f;
        while (timeout > 0f &&
            (DataManager.Instance == null ||
                DataManager.Instance.Watchinginfo == null ||
                DataManager.Instance.Watchinginfo.pattern == null))
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (DataManager.Instance == null ||
            DataManager.Instance.Watchinginfo?.pattern == null)
        {
            Debug.LogError("BinocularController: Watchinginfo/pattern not ready.", this);
            yield break;
        }

        Init();
    }

    void Init()
    {
        if (Idx.Count <= 0)
        {
            int i = 0, j = 0;
            foreach (string strVal in DataManager.Instance.Watchinginfo.pattern)
            {
                EWatching enumVal;
                if (Enum.TryParse(strVal, true, out enumVal))
                {
                    if (enumVal == EWatching.Binocular)
                    {
                        Idx.Add(i, j++);
                    }
                }
                i++;
            }
        }
    }

    private int GetCurrentChapterNow()
    {
        if (pc != null) return pc.GetChapter();
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) pc = playerObj.GetComponent<PlayerController>();
        return (pc != null) ? pc.GetChapter() : 0;
    }

    private bool IsTodayBinocularByData(int chapter)
    {
        var list = DataManager.Instance?.Watchinginfo?.pattern;
        if (list == null || list.Count == 0) return false;

        // 중요: 지금 프로젝트는 원복 상태(기존에 잘 됐음)니까 "chapter를 그대로 인덱스로" 본다.
        // (만약 여기서 밀림이 보이면 그때만 -1로 조정)
        int idx = chapter;
        if (idx < 0 || idx >= list.Count) return false;

        return Enum.TryParse(list[idx], true, out EWatching today) && today == EWatching.Binocular;
    }

    private void SetColliderEnabled(bool enabled)
    {
        if (col2D != null) col2D.enabled = enabled;
    }

    private void RefreshBinocularInteractable(bool force)
    {
        if (pc == null) return;
        if (DataManager.Instance?.Watchinginfo?.pattern == null) return;

        int ch = GetCurrentChapterNow();
        if (!force && ch == lastChapterChecked) return;
        lastChapterChecked = ch;

        bool inWatchingPhase = (GamePatternState)pc.GetCurrentPhase() == GamePatternState.Watching;
        bool canClick = inWatchingPhase && IsTodayBinocularByData(ch);

        // 오늘 아니면 클릭 자체 차단
        SetColliderEnabled(canClick);

        // 안전: 클릭 불가면 알럿도 꺼 버림
        if (!canClick && alert != null) alert.SetActive(false);
    }

    private void Update()
    {
        RefreshBinocularInteractable(force: false);
    }

    public void OpenWatching(int Chapter)
    {
        alert.SetActive(true);
        chapterIdx = Chapter;
    }

    private void OnMouseDown()
    {
        if (InputGuard.BlockWorldInput()) return;
        if (pc == null) return;

        // chapterIdx/하드코딩 대신 현재 값으로 판정
        int ch = GetCurrentChapterNow();

        if ((GamePatternState)pc.GetCurrentPhase() != GamePatternState.Watching) return;
        if (!IsTodayBinocularByData(ch)) return;

        if (phase != null) return;
        alert.SetActive(false);
            //Ŭ����~
            // 문 렌더링 끄기
        DoorController door = FindObjectOfType<DoorController>(); // 문 렌더링 끄기
        if (door != null)
        {
            door.SetDoorForDialogue(false);
        }

        phase = Instantiate(watching[Idx[chapterIdx]], watchingBackground.transform);       
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.binocular, this.transform.position);

        InputGuard.WorldInputLocked = true;
        
    }

    public void CloseWatching()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.iconClick, transform.position);
        alert.SetActive(false);
        DoorController door = FindObjectOfType<DoorController>();
        if (door != null)
        {
            door.SetDoorForDialogue(true);
        }
        InputGuard.WorldInputLocked = false;
    }

    private void OnDisable()
    {
        DoorController door = FindObjectOfType<DoorController>();
        if (door != null)
        {
            door.SetDoorForDialogue(true);
        }
        InputGuard.WorldInputLocked = false;
    }

}