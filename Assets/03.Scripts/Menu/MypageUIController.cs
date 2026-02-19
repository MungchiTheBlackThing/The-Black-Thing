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

    [SerializeField] PushNudgePopup unknownPopup;
    [SerializeField] PushNudgePopup deniedPopup;
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

    [SerializeField] GameManager gameManager;

    [SerializeField] List<Button> skipModeButtons; // [0]=ON, [1]=OFF
    [SerializeField] List<Button> subSkipButtons;  // [0]=ON, [1]=OFF

    [SerializeField] GameObject popupSkipMode;  // popup_mode_skiptime (YES/NO)
    [SerializeField] GameObject popupSubSkipOn;   // popup_mode_subskip_on (YES/NO)
    [SerializeField] GameObject popupSubSkipOff;  // popup_mode_subskip_off (YES/NO)







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
        gameManager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        _nextButText = nextBut.GetComponent<TMP_Text>();
        _prevButText = prevBut.GetComponent<TMP_Text>();
        _nameSettingText = nameSetting.GetComponent<TMP_Text>();
        popupPageName = new List<List<string>>();
        PushNudgeController.RegisterPopups(unknownPopup, deniedPopup);

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

        // UI가 켜질 때 GameManager에 저장된 현재 시작 시간 설정값을 가져와 동기화
        if (gameManager != null)
        {
            _uiHour24 = gameManager.dayStartHour;
            _uiMinute = gameManager.dayStartMinute;
        }
        RefreshTimeUI();

        UpdateNavButtonsVisibility();
        UpdateNavButtonText();

        EnableSkipModeColor();
        EnableSubSkipModeColor();
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
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
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
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.checklistOn, this.transform.position);
        _nameSettingText.text = userName;

        Invoke("CloseAlter", 1.5f);
    }

    /*Edit과 CancelEdit이 같이 사용*/
    public void ToggleEditName()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        if (editNamePopup.activeSelf)
        {
            editNamePopup.gameObject.SetActive(false);
            return;
        }
        editNamePopup.gameObject.SetActive(true);
    }

    public void OpenLangPopup()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        if (LangPopup.activeSelf)
        {
            LangPopup.gameObject.SetActive(false);
            return;
        }
        LangPopup.gameObject.SetActive(true);
    }

    void CloseAlter()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        closePopup.SetActive(false);
    }

    public void NextPage()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
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
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
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
        if (GuardAlready(isEnableAlert, true)) return;

        var perm = NotificationService.GetPermissionState(forceSync: true);

        if (perm == PushPermissionState.Granted)
        {
            ApplyPushOn();
        }
        else
        {
            PushNudgeController.TryShowFromSettings(gameManager, onGranted: ApplyPushOn);
        }
    }

    void ApplyPushOn()
    {
        isEnableAlert = true;
        EnablePushAlertColor();
        player.SetisPushNotificationEnabled(true);
        PlayerPrefs.SetInt("PushEnabled", 1);
        PlayerPrefs.Save();
        PushScheduler.ScheduleForCurrentPhase(gameManager);
    }

    public void OffPushAlert()
    {
        if (GuardAlready(isEnableAlert, false)) return;
        alterPopup.SetActive(true);
    }

    public void Off()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        isEnableAlert = false;
        EnablePushAlertColor();
        player.SetisPushNotificationEnabled(false);
        PlayerPrefs.SetInt("PushEnabled", 0);
        PlayerPrefs.Save();
        NotificationService.CancelAll();
        alterPopup.SetActive(false);
    }

    public void On()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
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
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
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

    void EnableSkipModeColor()
    {
        if (skipModeButtons == null || skipModeButtons.Count < 2) return;
        if (colors == null || colors.Count < 2) return;

        bool enabled = player != null && player.GetSkipModeEnabled();
        int idx = Convert.ToInt32(enabled) % 2;

        var onBtn  = skipModeButtons[idx];
        var offBtn = skipModeButtons[(idx + 1) % 2];

        if (onBtn != null)
        {
            var img = onBtn.GetComponent<Image>();
            if (img != null) img.color = colors[0];
        }

        if (offBtn != null)
        {
            var img = offBtn.GetComponent<Image>();
            if (img != null) img.color = colors[1];
        }
    }

    void EnableSubSkipModeColor()
    {
        if (subSkipButtons == null || subSkipButtons.Count < 2) return;
        if (colors == null || colors.Count < 2) return;

        bool enabled = player != null && player.GetSubSkipModeEnabled();
        int idx = Convert.ToInt32(enabled) % 2;

        var onBtn  = subSkipButtons[idx];
        var offBtn = subSkipButtons[(idx + 1) % 2];

        if (onBtn != null)
        {
            var img = onBtn.GetComponent<Image>();
            if (img != null) img.color = colors[0];
        }

        if (offBtn != null)
        {
            var img = offBtn.GetComponent<Image>();
            if (img != null) img.color = colors[1];
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
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        if (TimeSetPopup == null) return;

        var popup = TimeSetPopup.GetComponent<TimeSettingPopupController>();
        if (popup == null) return;

        popup.Open(_uiHour24, _uiMinute, ApplyTime);
    }

    private void ApplyTime(int hour24, int minute)
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        _uiHour24 = hour24;
        _uiMinute = minute;

        gameManager.SetDayStartTime(_uiHour24, _uiMinute);
        
        RefreshTimeUI();
    }

    private void RefreshTimeUI()
    {
        bool isPM = _uiHour24 >= 12;

        int hour12 = _uiHour24 % 12;
        if (hour12 == 0) hour12 = 12;

        if (timeLabel != null)
            timeLabel.text = $"{hour12:00}:{_uiMinute:00} {(isPM ? "PM" : "AM")}";
    }

    public void OnSkipModeOnClicked()
    {
        bool cur = player != null && player.GetSkipModeEnabled();
        if (GuardAlready(cur, true)) return;
        if (popupSkipMode != null) popupSkipMode.SetActive(true); // 팝업 뜸
    }

    public void OnSkipModeOffClicked()
    {

        bool cur = player != null && player.GetSkipModeEnabled();
        if (GuardAlready(cur, false)) return;
        ApplySkipMode(false); // 바로 OFF

    }

    public void OnSkipModeConfirmYes()
    {
        ApplySkipMode(true); // 스킵 모드 켬
        ClosePopupSkips(popupSkipMode); 

    }

    public void OnSubSkipOnClicked()
    {

        bool cur = player != null && player.GetSubSkipModeEnabled();
        if (GuardAlready(cur, true)) return;
        // 서브 스킵 켤까요? 팝업
        if (popupSubSkipOn != null) popupSubSkipOn.SetActive(true);
    }

    public void OnSubSkipOffClicked()
    {
        bool cur = player != null && player.GetSubSkipModeEnabled();
        if (GuardAlready(cur, false)) return;
        // 서브 스킵 끌까요? 팝업
        if (popupSubSkipOff != null) popupSubSkipOff.SetActive(true);
    }

    // popupSubSkipOn - YES
    public void OnSubSkipOnConfirmYes()
    {
        // 켤게요 - yes
        ApplySubSkip(true);
        ClosePopupSkips(popupSubSkipOn);

    }

    // popupSubSkipOff - YES
    public void OnSubSkipOffConfirmYes()
    {
        // 끌게요 - yes
        ApplySubSkip(false);
        ClosePopupSkips(popupSubSkipOff);
    }

    public void ClosePopupSkips(GameObject popup) // 스킵들 팝업 닫는 함수
    {
        // 팝업만 끔
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, transform.position);
        if (popup != null) popup.SetActive(false);
    }

    void ApplySkipMode(bool enabled)
    {
        if (player != null) player.SetSkipModeEnabled(enabled);
        EnableSkipModeColor();
    }

    void ApplySubSkip(bool enabled)
    {
        if (player != null) player.SetSubSkipModeEnabled(enabled);
        EnableSubSkipModeColor();
    }

    bool GuardAlready(bool currentEnabled, bool targetEnabled)
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, transform.position);
        return currentEnabled == targetEnabled; // 같으면 이미 그 상태
    }

}
