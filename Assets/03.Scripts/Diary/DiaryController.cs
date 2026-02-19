using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;

public class DiaryController : BaseObject, ISleepingInterface
{
    
    bool isClicked = true;
    bool isDiaryUpdated = false;
    [SerializeField]
    GameObject light;

    [SerializeField]
    GameObject alert;
    [SerializeField]
    DiaryUIController diaryUI;
    [SerializeField]
    TMP_Text text;

    TranslateManager translator;
    PlayerController playerController;
    DotController dotController;

    void OnEnable()
    {
        DotController.DiaryGateChanged += OnDiaryGateChanged;
    }

    void OnDisable()
    {
        DotController.DiaryGateChanged -= OnDiaryGateChanged;
    }

    void OnDiaryGateChanged()
    {
        if (!gameObject.activeInHierarchy) return;
        if (!enabled) return;
        // 애니메이션/패널 토글 프레임 꼬임 방지
        StopCoroutine(nameof(CoUpdateLightNextFrame));
        StartCoroutine(CoUpdateLightNextFrame());
    }


    void Start()
    {
        Init();
        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();
        translator.translatorDel += Translate;
        
        // 초기 로컬라이제이션 적용 (동적으로 생성되는 오브젝트는 Translate가 자동 호출되지 않을 수 있음)
        if (playerController != null)
        {
            LANGUAGE currentLang = playerController.GetLanguage();
            Translate(currentLang, null);
        }
        
        // 로컬라이제이션 설정 변경 감지 (언어 변경 시 자동 업데이트)
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
    
    void OnLocaleChanged(Locale locale)
    {
        // 언어 변경 시 텍스트 업데이트
        if (playerController != null)
        {
            LANGUAGE currentLang = playerController.GetLanguage();
            Translate(currentLang, null);
        }
    }

    void Translate(LANGUAGE language, TMP_FontAsset font)
    {
        if(alert != null && text != null)
        {
            // 로컬라이제이션 패키지 사용
            StringTable table = LocalizationSettings.StringDatabase.GetTable("SystemUIText");
            if (table != null)
            {
                var entry = table.GetEntry("mapalert_diary");
                if (entry != null)
                {
                    text.text = entry.GetLocalizedString();
                    return;
                }
            }
            
            int Idx = (int)language;
            text.text = DataManager.Instance.Settings.alert.diary[Idx];
        }
    }
    public void Init()
    {
        if(playerController == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            playerController = playerObj.GetComponent<PlayerController>();
            
        }
        if(dotController == null)
        {
            dotController = GameObject.FindWithTag("DotController")?.GetComponent<DotController>();
        }

        isClicked = playerController.GetIsDiaryCheck();
        isDiaryUpdated = playerController.GetIsUpdatedDiary();
        UpdateDiaryLight();

        /**
        일기 불빛 로직 수정 - 피로함 개선

        isDiaryUpdated == true (새로 업데이트됨)

        isClicked == false (아직 안 읽음)

        canOpen == true (지금 훔쳐보기 가능: 외출 Watching OR anim_sleep)

        => 이 3개가 모두 true일 때만 light ON, 나머지는 전부 OFF.
        **/
    }
    public void OpenSleeping()
    {
        isDiaryUpdated = true;
        if (playerController) playerController.SetIsUpdatedDiary(true);

        StopCoroutine(nameof(CoUpdateLightNextFrame));
        StartCoroutine(CoUpdateLightNextFrame());
    }

    IEnumerator CoUpdateLightNextFrame()
    {
        yield return new WaitForEndOfFrame();
        UpdateDiaryLight();
    }

    public void OnMouseUp()
    {
        if (InputGuard.BlockWorldInput()) return;
        if (GameManager.isend) // static 접근
        {
            if (diaryUI == null)
                diaryUI = GameObject.Find("Diary").GetComponent<DiaryUIController>();

            if (light != null) UpdateDiaryLight();

            if (playerController == null)
                playerController = GameObject.Find("PlayerController")?.GetComponent<PlayerController>();

            if (playerController != null)
                playerController.SetIsDiaryCheck(true);
            
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.diary, this.transform.position);

            diaryUI.SetActiveCloseDiary();
            return;
        }
        RecentData data = RecentManager.Load();
        if (!data.tutoend)
        {
            return;
        }
        if (diaryUI == null)
        {
            diaryUI = GameObject.Find("Diary").GetComponent<DiaryUIController>();
        }

        // canOpen = (외출 Watching) OR (anim_sleep)
        bool canOpen = CanOpenDiaryNow();
        if (!canOpen)
        {
            OpenAlert();
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.lockClick, transform.position);
            return;
        }

        isClicked = true;
        //플레이어 정보도 업데이트 한다.
        if (light != null) UpdateDiaryLight();
        playerController.SetIsDiaryCheck(true);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.diary, this.transform.position);
        diaryUI.SetActiveCloseDiary();
    }

    public void OpenAlert()
    {
        if (alert.activeSelf == false)
        {
            alert.SetActive(true);
            StartCoroutine(CloseAlter(alert));
        }
    }

    IEnumerator CloseAlter(GameObject alert)
    {
        yield return new WaitForSeconds(1.5f);
        alert.SetActive(false);
    }

    bool CanOpenDiaryNow()
    {
        // 0) 서브 패널 켜져 있으면 무조건 불가 + 라이트도 꺼져야 함
        if (dotController == null)
            dotController = GameObject.FindWithTag("DotController")?.GetComponent<DotController>();

        if (dotController != null && dotController.subDialogue != null && dotController.subDialogue.activeSelf)
            return false;
        // 1) 외출 Watching이면 OK
        var phase = (GamePatternState)playerController.GetCurrentPhase();

        if (phase == GamePatternState.Watching)
        {
            string watchStateStr = DataManager.Instance.Watchinginfo.pattern[playerController.GetChapter()];
            if (Enum.TryParse(watchStateStr, true, out EWatching watch))
                return watch != EWatching.StayAtHome;
        }

        // 2) anim_sleep이면 페이즈 무관 OK
        if (dotController == null)
            dotController = GameObject.FindWithTag("DotController")?.GetComponent<DotController>();

        return dotController != null && 
            (dotController.AnimKey == "anim_sleep" || dotController.AnimKey == "anim_sleep_mare");
    }

    void UpdateDiaryLight()
    {
        bool shouldLight =
            isDiaryUpdated &&
            !isClicked &&
            CanOpenDiaryNow();

        if (light != null)
            light.SetActive(shouldLight);
    }
}
