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

    Dictionary<int,int> Idx = new Dictionary<int,int>();
    public bool IsCurrentPattern(EWatching curPattern)
    {
        return curPattern == type;
    }

    private void Start()
    {
        watchingBackground = GameObject.Find("Phase").gameObject;
        Init();
    }

    void Init()
    {
        if (Idx.Count <= 0)
        {
            int i = 0, j = 0;
            foreach (string strVal in DataManager.Instance.Settings.watching.pattern)
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

        if (alert.activeSelf)
        {
            alert.SetActive(false);
            //Ŭ����~
            phase = Instantiate(watching[Idx[chapterIdx]], watchingBackground.transform);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.binocular, this.transform.position);
        }
    }

    public void CloseWatching()
    {
        alert.SetActive(false);
    }
}
