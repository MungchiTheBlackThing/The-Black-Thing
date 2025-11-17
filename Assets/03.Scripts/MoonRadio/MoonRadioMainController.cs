using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

public class MoonRadioMainController : MonoBehaviour
{

    #region Main Moon Radio
    [SerializeField]
    GameObject moonRadioOff;
    [SerializeField]
    GameObject moonRadioOn;
    #endregion

    [SerializeField]
    GameObject moonRadioMoon;
    [SerializeField]
    GameObject moonRadioEarth;

    [SerializeField]
    GameObject systemUI; //이거 델리게이트로 대체할 예정
    [SerializeField]
    GameObject screen;
    IPlayerInterface player;

    [SerializeField]
    GameObject popupUI;

    [SerializeField]
    GameObject menu;

    [SerializeField]
    GameObject screenshield;
    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<IPlayerInterface>();
    }

    private void OnEnable()
    {
        moonRadioOff.SetActive(false);
        moonRadioOn.SetActive(true);
        screenshield.SetActive(true);
        //systemUI를 꺼준다.
        systemUI = GameObject.Find("SystemUI");
        if(systemUI)
        {
            systemUI.SetActive(false);
        }
    }

    private void OnDisable()
    {
        screenshield.SetActive(false);
        if(systemUI)
        {
            systemUI.SetActive(true);
        }
        if (menu)
        {
            menu.SetActive(true);
            menu.GetComponent<MenuController>().tuto();
        }
    }

    public void RadioOff()
    {
        if (moonRadioOff.activeSelf == false)
        {
            screen.SetActive(true);
            moonRadioOn.SetActive(false);
            moonRadioOff.SetActive(true);
        }
    }

    public void GoMoon()
    {
        if(player.GetMoonRadioIdx() <= 2)
        {
            moonRadioMoon.SetActive(true);
        }
        else
        {
            popupUI.SetActive(true);
        }
    }

    public void ClosePopup()
    {
        popupUI.SetActive(false);
    }

    public void GoEarth()
    {
        moonRadioEarth.SetActive(true);
    }

    public void GoMain()
    {
        this.gameObject.SetActive(false);
    }

    public void BackMoonRadio()
    {
        moonRadioOn.SetActive(true);
        moonRadioOff.SetActive(false);
    }
}
