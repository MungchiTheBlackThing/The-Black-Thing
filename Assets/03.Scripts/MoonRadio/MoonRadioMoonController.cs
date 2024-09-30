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
    public void OnPopup()
    {
        if(popupIdx>=popupUI.Length)
        {
            return;
        }

        popupUI[popupIdx].SetActive(true);
        popupIdx++;
    }

    public void Exit()
    {
        for (int i = 0; i < popupUI.Length; i++)
        {
            popupUI[i].SetActive(false);
        }

        this.gameObject.SetActive(false);
        exit.gameObject.SetActive(false);
        //�÷��̾ ���� �� Idx ���� (��, popupIdx �� �����ϸ� �ȴ�.)
        player.SetMoonRadioIdx(popupIdx + 1);
    }

    public void SetDial()
    {
        exit.gameObject.SetActive(false);
        popupUI[0].SetActive(false);
        chatController.Reset(popupIdx + 1);
    }
}
