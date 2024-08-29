using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiaryController : BaseObject, ISleepingInterface
{

    
    bool isClicked = true;
    bool isDiaryUpdated = false;
    [SerializeField]
    GameObject light;
    [SerializeField]
    GameObject alert;
    PlayerController playerController;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        if(playerController == null)
        {
            playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        }
        isClicked = playerController.GetIsDiaryCheck(); //���̾�� �о����� �����´�.
        isDiaryUpdated = playerController.GetIsUpdatedDiary();
        //���̾�� ������Ʈ �Ǿ�������, Ŭ������ �ʾ��� ��� ���̾�� ���������� �Һ��� ���´�.
        if (isDiaryUpdated)
        {
            if(isClicked == false)
            {
                OpenSleeping();
                return;
            }
        }

        //Ŭ���߰ų�, ������Ʈ�� �ȵ����� �ƹ� �ǹ̾���
    }
    public void OpenSleeping()
    {
        //Play���� ���̾�� ������Ʈ
        //���̾�� ������Ʈ �Ǿ��� ������ Sleeping���� ���ö� �׻� ���̾ �Һ��� ���´�.
        //���̾ �Һ��� ���´�.    
        if(light.activeSelf == false)
        {
            light.SetActive(true);
            isDiaryUpdated = true;

            if(playerController)
            {
                Debug.Log("�÷��̾� ��Ʈ�ѷ� ������� �ȿ�");
                //�÷��̾� ������ ������Ʈ �Ѵ�.
                playerController.SetIsUpdatedDiary(isDiaryUpdated);
            }
        }
    }

    public void OnMouseDown()
    {
        //Ŭ������ �� ���� ��ġ�� ���� ���ΰ�, Sleeping�ΰ��� ���� ���콺 Ŭ���� ���ƾ��Ѵ�.
        GamePatternState CurrentPhase = (GamePatternState)playerController.GetAlreadyEndedPhase();

        if(CurrentPhase != GamePatternState.Watching && CurrentPhase != GamePatternState.Sleeping)
        {
            OpenAlert();
            return;
        }

        if(CurrentPhase == GamePatternState.Watching)
        {
            //AtHome�� �� return;
            string WatchState = DataManager.Instance.Settings.watching.pattern[playerController.GetChapter()];

            EWatching watch;
            if (Enum.TryParse(WatchState,true, out watch))
            {
                if(watch == EWatching.StayAtHome)
                {
                    return;
                }
            }
        }

        isClicked = true;
        //�÷��̾� ������ ������Ʈ �Ѵ�.
        light.SetActive(false);
        playerController.SetIsDiaryCheck(isClicked);
    }

    public void OpenAlert()
    {
        if (alert.activeSelf == false)
        {
            alert.SetActive(true);
            StartCoroutine(CloseAlter(alert));
        }
    }

    IEnumerator CloseAlter(GameObject alert)
    {
        yield return new WaitForSeconds(1.5f);
        alert.SetActive(false);
    }
}
