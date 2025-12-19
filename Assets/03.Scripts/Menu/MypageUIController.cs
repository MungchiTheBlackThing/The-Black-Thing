using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class MypageUIController : MonoBehaviour
{
    [SerializeField] GameObject menu;

    [SerializeField] GameObject editNamePopup;
    [SerializeField] GameObject LangPopup;
    [SerializeField] GameObject closePopup;
    [SerializeField] GameObject alterPopup;
    [SerializeField] GameObject nameSetting;

    [SerializeField] List<Button> pushAlert;
    [SerializeField] List<Button> languageAlert;

    [SerializeField] Slider seSlider;
    [SerializeField] Slider musicSlider;

    [SerializeField] string navStringTable = "SystemUIText";
    [SerializeField] List<string> popupPageLocalizationKeys;

    [SerializeField] GameObject TimeSetPopup;
    [SerializeField] private TMP_Text timeLabel;





    string userID = "";
    string userName = "";
    float musicVolume = 0.5f;
    float seVolume = 0.5f;
    int pageIdx = 0;
    bool isEnableAlert = true;
    bool isKorean = true;

    PlayerController player;

    #region Nickname Section
    [SerializeField]
    TMP_Text nicknameTxt;
    [SerializeField]
    TMP_Text closeTxt;

    private int _uiHour24 = 11;
    private int _uiMinute = 0;
    #endregion


    [SerializeField] List<GameObject> popupPage;

    [SerializeField] GameObject prevBut;
    [SerializeField] GameObject nextBut;
    [SerializeField] List<Color> colors;

    List<List<string>> popupPageName;

    TMP_Text _nextButText;
    TMP_Text _prevButText;
    TMP_Text _nameSettingText;
    bool _inited = false;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        _nextButText = nextBut.GetComponent<TMP_Text>();
        _prevButText = prevBut.GetComponent<TMP_Text>();
        _nameSettingText = nameSetting.GetComponent<TMP_Text>();
        popupPageName = new List<List<string>>();

        Init();
    }

    private void OnEnable()
    {
        //켜질때마다
        pageIdx = 0;

        for (int i = 0; i < popupPage.Count; i++)
        {
            //모두 정리
            popupPage[i].SetActive(false);
        }
        prevBut.SetActive(false);
        popupPage[pageIdx].SetActive(true);
        RefreshTimeUI();

        UpdateNavButtonsVisibility();
        UpdateNavButtonText();
    }

    void Init()
    {
        if (_inited) return;
        _inited = true;

        if (player != null)
        {
            isEnableAlert = player.GetisPushNotificationEnabled();
            isKorean = player.GetLanguage() == LANGUAGE.KOREAN;
            userName = player.GetNickName();
            musicVolume = player.GetMusicVolume();
            seVolume = player.GetAcousticVolume();
        }
        _nameSettingText.text = userName;

        if (seSlider != null)
        {
            seSlider.value = seVolume;
            seSlider.onValueChanged.RemoveListener(OnValueChangeSE);
            seSlider.onValueChanged.RemoveAllListeners();
            //delegate 연결,실제 음악 델리게이트 연결
            seSlider.onValueChanged.AddListener(OnValueChangeSE);
            if (player != null) seSlider.onValueChanged.AddListener(player.SetSEVolume);
            // seSlider.onValueChanged.AddListener( MusicManager.Instance.AdjustSEVolume);
        }

        if (musicSlider != null)
        {
            musicSlider.value = musicVolume;
            musicSlider.onValueChanged.RemoveListener(OnValueChangedBGM);
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(OnValueChangedBGM);
            if (player != null) musicSlider.onValueChanged.AddListener(player.SetBGMVolume);
            // musicSlider.onValueChanged.AddListener(MusicManager.Instance.AdjustBGMVolume);  
        }


        List<string> setting = new List<string>
        {
            DataManager.Instance.Settings.menuMyPage.settings[0],
            DataManager.Instance.Settings.menuMyPage.settings[1]
        };

        List<string> community = new List<string>
        {
            DataManager.Instance.Settings.menuMyPage.community[0],
            DataManager.Instance.Settings.menuMyPage.community[1]
        };

        List<string> credit = new List<string>
        {
            DataManager.Instance.Settings.menuMyPage.credit[0],
            DataManager.Instance.Settings.menuMyPage.credit[1]
        };

        popupPageName.Add(setting);
        popupPageName.Add(community);
        popupPageName.Add(credit);

        UpdateNavButtonText();
        EnablePushAlertColor();
    }
    public void OnValueChangedBGM(float value)
    {
        musicVolume = value;
        AudioManager.Instance.SetBGMVolume(value);
        //델리게이트 float 값 주어서 값이 변경될 때 음악도 변경
    }

    public void OnValueChangeSE(float value)
    {
        seVolume = value;
        AudioManager.Instance.SetSFXVolume(value);
        //델리게이트 float 값을 주어서 값이 변경될 때 음악도 변경
    }

    public void StoreName()
    {
        //1. EditPopup으로부터 실제 저장할 Nickname을 가져온다.popup_namesetting - NameInput - TextBoxInput으로부터 text를 가져온다.
        userName = nicknameTxt.text;

        //2. 셋팅
        player.SetNickName(userName);

        //3. closePopup <nickname>을 찾아서 Nickname으로 수정한다.
        var lse = closeTxt.GetComponent<LocalizeStringEvent>();
        if (lse != null)
        {
            lse.StringReference.Arguments = new object[] { new { nickname = userName } };
            lse.RefreshString();
        }
        else
        {
            closeTxt.text = closeTxt.text.Replace("<nickname>", userName);
            Debug.Log("LocalizeStringEvent 없음");
        }

        //4. 1.5초 뒤 꺼진다.
        editNamePopup.SetActive(false);
        closePopup.SetActive(true);
        _nameSettingText.text = userName;

        Invoke("CloseAlter", 1.5f);
    }

    /*Edit과 CancelEdit이 같이 사용*/
    public void ToggleEditName()
    {
        if (editNamePopup.activeSelf)
        {
            editNamePopup.gameObject.SetActive(false);
            return;
        }
        editNamePopup.gameObject.SetActive(true);
    }

    public void OpenLangPopup()
    {
        if (LangPopup.activeSelf)
        {
            LangPopup.gameObject.SetActive(false);
            return;
        }
        LangPopup.gameObject.SetActive(true);
    }

    void CloseAlter()
    {
        closePopup.SetActive(false);
    }

    public void NextPage()
    {
        int nextIndex = pageIdx + 1;

        //한계 설정, 페이지 개수에 따라서 페이지를 넘을 경우 조절
        if (popupPage == null || popupPage.Count == 0) return;

        if (nextIndex >= popupPage.Count)
        {
            pageIdx = popupPageName != null && popupPageName.Count > 0 ? popupPageName.Count - 1 : popupPage.Count - 1;
            UpdateNavButtonsVisibility();
            UpdateNavButtonText();
            return;
        }

        //현재 페이지를 보여주고, 이전 페이지를 없앰
        SetActivePage(nextIndex, pageIdx);
        pageIdx = nextIndex;

        UpdateNavButtonsVisibility();
        UpdateNavButtonText();
    }

    public void PrePage()
    {
        int prevIndex = pageIdx - 1;

        if (pageIdx < 0)
        {
            pageIdx = 0;
            UpdateNavButtonsVisibility();
            UpdateNavButtonText();
            return;
        }
        SetActivePage(prevIndex, pageIdx);
        pageIdx = prevIndex;

        UpdateNavButtonsVisibility();
        UpdateNavButtonText();
    }

    public void OnPushAlert()
    {
        isEnableAlert = true;
        EnablePushAlertColor();
        player.SetisPushNotificationEnabled(isEnableAlert);
    }

    public void OffPushAlert()
    {
        alterPopup.SetActive(true);
    }

    public void Off()
    {
        isEnableAlert = false;
        EnablePushAlertColor();
        player.SetisPushNotificationEnabled(isEnableAlert);
        alterPopup.SetActive(false);
    }

    public void On()
    {
        isEnableAlert = true;
        EnablePushAlertColor();
        player.SetisPushNotificationEnabled(isEnableAlert);
        alterPopup.SetActive(false);
    }

    public void SetKorean()
    {
        isKorean = true;
        player.SetLanguage(LANGUAGE.KOREAN);
        //EnableLanguageColor();
        UpdateNavButtonText();
    }

    public void SetEnglish()
    {
        isKorean = false;
        player.SetLanguage(LANGUAGE.ENGLISH);
        //EnableLanguageColor();
        UpdateNavButtonText();
    }
    public void Exit()
    {
        this.gameObject.SetActive(false);
    }

    void EnablePushAlertColor()
    {
        if (pushAlert == null || pushAlert.Count < 2) return;
        if (colors == null || colors.Count < 2) return;

        int Idx = Convert.ToInt32(isEnableAlert) % 2;
        var onBtn = pushAlert[Idx];
        var offBtn = pushAlert[(Idx + 1) % 2];

        if (onBtn != null)
        {
            var img = onBtn.GetComponent<Image>();
            if (img != null) img.color = colors[0]; //0번이 활성화
        }

        if (offBtn != null)
        {
            var img = offBtn.GetComponent<Image>();
            if (img != null) img.color = colors[1]; //1번 비활성화
        }
    }

    // void EnableLanguageColor()
    // {
    //     int Idx = Convert.ToInt32(isKorean) % 2;
    //     //languageAlert[Idx].GetComponent<TMP_Text>().color = colors[0]; //0번이 활성화
    //     //languageAlert[(Idx + 1) % 2].GetComponent<TMP_Text>().color = colors[1]; //1번 비활성화

    //     if (prevBut.activeSelf)
    //     {
    //         if (popupPage.Count > 0)
    //         {
    //             int LangIdx = (int)player.GetLanguage();
    //             prevBut.GetComponent<TMP_Text>().text = popupPageName[pageIdx - 1][LangIdx];
    //         }
    //     }
    //     if (nextBut.activeSelf)
    //     {
    //         if (popupPage.Count > 0)
    //         {
    //             int LangIdx = (int)player.GetLanguage();
    //             nextBut.GetComponent<TMP_Text>().text = popupPageName[pageIdx + 1][LangIdx];
    //         }
    //     }
    // }

    void UpdateNavButtonsVisibility()
    {
        // prev
        if (prevBut != null)
        {
            bool showPrev = pageIdx - 1 >= 0;
            prevBut.SetActive(showPrev);
        }

        // next
        if (nextBut != null)
        {
            bool showNext = popupPageName != null && pageIdx + 1 < popupPageName.Count;
            if (popupPage != null && pageIdx + 1 < popupPage.Count)
                showNext = showNext && true;
            nextBut.SetActive(showNext);
        }
    }

    void UpdateNavButtonText()
    {
        if (nextBut.activeSelf)
        {
            int nextLabelIdx = pageIdx + 1;
            _nextButText.text = GetPageNameLocalized(nextLabelIdx);

        }
        if (prevBut.activeSelf)
        {
            int prevLabelIdx = pageIdx - 1;
            _prevButText.text = GetPageNameLocalized(prevLabelIdx);
        }
    }

    void SetActivePage(int newIdx, int oldIdx)
    {
        if (popupPage == null) return;

        if (oldIdx >= 0 && oldIdx < popupPage.Count && popupPage[oldIdx] != null)
            popupPage[oldIdx].SetActive(false);

        if (newIdx >= 0 && newIdx < popupPage.Count && popupPage[newIdx] != null)
            popupPage[newIdx].SetActive(true);
    }

    string GetPageNameLocalized(int idx)
    {
        if (idx >= 0 && idx < popupPageLocalizationKeys.Count &&
            !string.IsNullOrEmpty(popupPageLocalizationKeys[idx]))
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(navStringTable, popupPageLocalizationKeys[idx]);
        }

        return string.Empty;
    }

    public void OpenTimeSettingPopup()
    {
        if (TimeSetPopup == null) return;

        var popup = TimeSetPopup.GetComponent<TimeSettingPopupController>();
        if (popup == null) return;

        popup.Open(_uiHour24, _uiMinute, ApplyTime);
    }

    private void ApplyTime(int hour24, int minute)
    {
        _uiHour24 = hour24;
        _uiMinute = minute;

        RefreshTimeUI();
    }

    private void RefreshTimeUI()
    {
        bool isPM = _uiHour24 >= 12;

        int hour12 = _uiHour24 % 12;
        if (hour12 == 0) hour12 = 12;

        if (timeLabel != null)
            timeLabel.text = $"{(isPM ? "PM" : "AM")} {hour12:00}:{_uiMinute:00}";
    }


}
