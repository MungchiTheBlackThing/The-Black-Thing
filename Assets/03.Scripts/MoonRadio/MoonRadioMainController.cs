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


    private void OnEnable()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.GetComponent<IPlayerInterface>();

        AudioManager.Instance.PushBGMOverride(FMODEvents.Instance.bgm_moonradio);
        moonRadioOff.SetActive(false);
        moonRadioOn.SetActive(true);
        ScreenShield.On();
        systemUI = GameObject.Find("SystemUI");
        if(systemUI)
        {
            systemUI.SetActive(false);
        }
    }

    private void OnDisable()
    {
        AudioManager.Instance.PopBGMOverride();
        ScreenShield.Off();
        if (systemUI)
        {
            systemUI.SetActive(true);
        }
        if (menu)
        {
            menu.SetActive(true);
            menu.GetComponent<MenuController>().tuto();
        }
    }
    private void OnDestroy()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PopBGMOverride();
    }

    public void RadioOff()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonoff, transform.position);
        if (moonRadioOff.activeSelf == false)
        {
            screen.SetActive(true);
            moonRadioOn.SetActive(false);
            moonRadioOff.SetActive(true);
        }
    }

    public void GoMoon()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonclick, transform.position);
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
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonclick, transform.position);
        popupUI.SetActive(false);
    }

    public void GoEarth()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonclick, transform.position);
        moonRadioEarth.SetActive(true);
    }

    public void GoMain()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonoff, transform.position);
        this.gameObject.SetActive(false);
    }

    public void BackMoonRadio()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonclick, transform.position);
        moonRadioOn.SetActive(true);
        moonRadioOff.SetActive(false);
    }
}
