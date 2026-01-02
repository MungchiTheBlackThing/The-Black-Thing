using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionController : MonoBehaviour
{
    //isFour 변수에 따라 선택지 갯수가 달라진다.
    [SerializeField]
    bool isFour;

    [SerializeField]
    GameObject actionButton;

    private int selectedCount = 0;
    //선택지는 나중에 저장한다.
    public void Choose()
    {
        GameObject option = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        Debug.Log(option);
        if (option.transform.GetChild(0).gameObject.activeSelf)
        {
            option.transform.GetChild(0).gameObject.SetActive(false);
            selectedCount--;
            //AudioManager.Instance.PlayOneShot(FMODEvents.Instance.dialouguecheckbox, this.transform.position);    //엠비언트 효과 빠짐 -> 추후에 추가
        }
        else
        {
            option.transform.GetChild(0).gameObject.SetActive(true);
            selectedCount++;
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.dialougeSelect, this.transform.position);
        }

        actionButton.SetActive(selectedCount > 0);
    }

    public void OnDisable()
    {
        // 자식 옵션들 전부 체크 끄기
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform option = transform.GetChild(i);
            if (option.childCount > 0)
            {
                GameObject mark = option.GetChild(0).gameObject;
                if (mark.activeSelf)
                    mark.SetActive(false);
            }
        }

        selectedCount = 0;
        actionButton.SetActive(false);
    }
}
