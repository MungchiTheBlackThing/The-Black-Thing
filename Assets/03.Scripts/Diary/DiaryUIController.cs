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
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.DiaryButton, this.transform.position);
        popupUI.SetActive(true);
    }

    public void OnClickDiary()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        popupUI.SetActive(false);
        openDiaryUI.SetActive(true);
        ScreenShield.Off();
        //���̵� UI Ȱ��ȭ ����
        //�ϱ��� �������� 2�� �̻��� ��
        //ù 1ȸ�� Ȱ��ȭ
        float eligible = PlayerPrefs.GetFloat("DiaryGuideEligible", 0f);
        float shown    = PlayerPrefs.GetFloat("DiaryGuideShown", 0f);
        if (eligible >= 1f && shown < 1f)
        {
            SetActiveGuide();
            PlayerPrefs.SetFloat("DiaryGuideShown", 1);
            PlayerPrefs.Save();
        }
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
