using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiaryUIController : MonoBehaviour
{
    // Start is called before the first frame update


    [SerializeField]
    GameObject guideUI;
    [SerializeField]
    GameObject popupUI;
    [SerializeField]
    GameObject openDiaryUI;
    [SerializeField]
    GameObject closeDiaryUI;
    [SerializeField]
    GameManager gameManger;


    public void SetActiveGuide()
    {
        guideUI.SetActive(true);
    }

    public void SetActiveCloseDiary()
    {
        closeDiaryUI.SetActive(true);
    }

    public void OnClickGuide()
    {
        if(guideUI.activeSelf)
        {
            guideUI.SetActive(false);
        }
    }
    public void OnClickPopupUIOpen()
    {
        popupUI.SetActive(true);
    }

    public void OnClickDiary()
    {
        popupUI.SetActive(false);
        openDiaryUI.SetActive(true);
        ScreenShield.Off();
        //가이드 UI 활성화 조건
        //일기장 페이지가 2장 이상일 때
        //첫 1회만 활성화
        float isShowGuide = PlayerPrefs.GetFloat("DiaryGuideShown");
        if (gameManger.Chapter >= 2 && isShowGuide == 0)
        {
            SetActiveGuide();
            PlayerPrefs.SetFloat("DiaryGuideShown", 1);
        }
    }

    public void Exit()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;
        GameObject gameObject = button.transform.parent.gameObject;
        if(gameObject)
        {
            gameObject.SetActive(false);
        }

        if(openDiaryUI.activeSelf)
        {
            openDiaryUI.SetActive(false);
        }
        ScreenShield.Off();
    }
}
