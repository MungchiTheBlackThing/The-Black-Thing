using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonRadioMoonController : MonoBehaviour
{

    [SerializeField]
    GameObject[] popupUI;

    [SerializeField]
    GameObject exit;

    [SerializeField]
    MoonChatClickController chatController;

    int popupIdx = 0;

    IPlayerInterface player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<IPlayerInterface>();
        popupIdx = player.GetMoonRadioIdx() - 1;
    }
    private void OnEnable()
    {
        // 매번 재진입할 때 UI/인덱스 초기화
        for (int i = 0; i < popupUI.Length; i++)
            popupUI[i].SetActive(false);

        int saved = (player != null) ? player.GetMoonRadioIdx() : 1; // 기본 1
        // saved가 1이면 첫 팝업(0)을 띄우고 싶다는 뜻으로 처리
        popupIdx = Mathf.Clamp(saved - 1, 0, popupUI.Length); 
    }
    public void OnPopup()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonoff, transform.position);
        if(popupIdx>=popupUI.Length)
        {
            return;
        }

        popupUI[popupIdx].SetActive(true);
        popupIdx++;
    }

    public void Exit()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonoff, transform.position);
        for (int i = 0; i < popupUI.Length; i++)
        {
            popupUI[i].SetActive(false);
        }

        this.gameObject.SetActive(false);
        exit.gameObject.SetActive(false);
        //플레이어가 현재 본 Idx 저장 (즉, popupIdx 를 저장하면 된다.)
        player.SetMoonRadioIdx(popupIdx + 1);
    }

    public void SetDial()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonclick, transform.position);
        exit.gameObject.SetActive(false);
        popupUI[0].SetActive(false);
        chatController.Reset(popupIdx + 1);
    }
}
