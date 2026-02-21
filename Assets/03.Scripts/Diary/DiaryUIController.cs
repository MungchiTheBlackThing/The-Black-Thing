using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DiaryUIController : MonoBehaviour
{
    // Start is called before the first frame update


    [SerializeField]
    GameObject popupUI;
    [SerializeField]
    GameObject openDiaryUI;
    [SerializeField]
    GameObject closeDiaryUI;
    [SerializeField]
    GameManager gameManger;


    public void SetActiveCloseDiary()
    {
        closeDiaryUI.SetActive(true);
    }

    public void OnClickPopupUIOpen()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.DiaryButton, this.transform.position);
        popupUI.SetActive(true);
    }

    public void OnClickDiary()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        popupUI.SetActive(false);
        openDiaryUI.SetActive(true);
        ScreenShield.Off();
    }

    public void Exit()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
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
