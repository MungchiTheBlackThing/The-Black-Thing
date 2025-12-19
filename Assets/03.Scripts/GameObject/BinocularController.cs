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

    private void Start()
    {
        door = FindObjectOfType<DoorController>();
        watchingBackground = GameObject.Find("Phase").gameObject;
            GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            pc = playerObj.GetComponent<PlayerController>();
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
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // ���콺�� UI ���� ���� ���� �� �Լ��� �������� �ʵ��� ��
            return;
        }

        if (pc == null) return;

        GamePatternState curPhase = (GamePatternState)pc.GetCurrentPhase();

        if (curPhase != GamePatternState.Watching)
        
            return;

        if (phase != null)
            return;

        alert.SetActive(false);
            //Ŭ����~
            // 문 렌더링 끄기
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
    }
}