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
    public void OpenWatching(int Chapter)
    {
        alert.SetActive(true);
        chapterIdx = Chapter;
    }

    private void OnMouseDown()
    {
        if (InputGuard.BlockWorldInput()) return;
        if (pc == null) return;

        GamePatternState curPhase = (GamePatternState)pc.GetCurrentPhase();

        if (curPhase != GamePatternState.Watching || !IsValidBinoDay(chapterIdx))
        
            return;

        if (phase != null)
            return;

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
        
    }

    public void CloseWatching()
    {
        alert.SetActive(false);
        DoorController door = FindObjectOfType<DoorController>();
        if (door != null)
        {
            door.SetDoorForDialogue(true);
        }
    }

    private void OnDisable()
    {
        DoorController door = FindObjectOfType<DoorController>();
        if (door != null)
        {
            door.SetDoorForDialogue(true);
        }
    }

}