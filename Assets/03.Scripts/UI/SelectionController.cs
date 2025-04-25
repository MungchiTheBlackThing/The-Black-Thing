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
            AudioManager.instance.PlayOneShot(FMODEvents.instance.dialouguecheckbox, this.transform.position);
        }
        else
        {
            option.transform.GetChild(0).gameObject.SetActive(true);
            selectedCount++;
            AudioManager.instance.PlayOneShot(FMODEvents.instance.dialougeSelect, this.transform.position);
        }

        actionButton.SetActive(selectedCount > 0);
    }
}