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


    public void SetActiveGuide()
    {
        guideUI.SetActive(true);
        SetActiveCloseDiary();
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
    }
}
