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

    void Start()
    {
        //Debug.Log($"[DiaryController.Start] 시작 - GameObject: {gameObject.name}, 현재 활성화 상태: {gameObject.activeSelf}");
        Init();

        int currentChapter = playerController.GetChapter();
        bool isUnlocked = playerController.IsDiaryUnlockedForChapter1();
        //Debug.Log($"[DiaryController.Start] Chapter: {currentChapter}, IsDiaryUnlockedForChapter1: {isUnlocked}, 현재 활성화 상태: {gameObject.activeSelf}");

        // 1챕터이고, 다이어리가 아직 잠금 해제되지 않았을 때만 비활성화
        // 이미 잠금 해제된 경우에는 활성화 상태 유지 (게임 재시작 시에도 유지)
        if (currentChapter == 1)
        {
            if (!isUnlocked)
            {
                //Debug.Log($"[DiaryController.Start] 1일차 다이어리 잠금 상태 - 비활성화");
                gameObject.SetActive(false);
                //Debug.Log($"[DiaryController.Start] 비활성화 후 상태: {gameObject.activeSelf}");
                return;
            }
            else
            {
                //Debug.Log($"[DiaryController.Start] 1일차 다이어리 이미 잠금 해제됨 - 활성화 확인");
                if (!gameObject.activeSelf)
                {
                    //Debug.Log($"[DiaryController.Start] 비활성화 상태 감지 - 활성화 시도");
                    gameObject.SetActive(true);
                    //Debug.Log($"[DiaryController.Start] 활성화 후 상태: {gameObject.activeSelf}");
                }
                else
                {
                    //Debug.Log($"[DiaryController.Start] 이미 활성화 상태 유지");
                }
            }
        }
        else
        {
            //Debug.Log($"[DiaryController.Start] 1일차가 아님 (Chapter: {currentChapter}) - 정상 처리 계속");
        }

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
            if (playerObj != null)
            {
                playerController = playerObj.GetComponent<PlayerController>();



            }
        }
        if(dotController == null)
        {
            dotController = GameObject.FindWithTag("DotController")?.GetComponent<DotController>();
        }
        isClicked = playerController.GetIsDiaryCheck(); //다이어리를 읽었는지 가져온다.
        isDiaryUpdated = playerController.GetIsUpdatedDiary();
        //다이어리가 업데이트 되어있지만, 클릭하지 않았을 경우 다이어리는 지속적으로 불빛이 들어온다.
        if (isDiaryUpdated)
        {
            if(isClicked == false)
            {
                OpenSleeping();
                return;
            }
        }

        //클릭했거나, 업데이트가 안됐으면 아무 의미없음
    }
    public void OpenSleeping()
    {
        //Play에서 다이어리가 업데이트
        //다이어리가 업데이트 되었기 때문에 Sleeping으로 들어올땐 항상 다이어리 불빛이 들어온다.
        //다이어리 불빛이 들어온다.    
        if(light.activeSelf == false)
        {
            light.SetActive(true);
            isDiaryUpdated = true;

            if(playerController)
            {
                //플레이어 정보도 업데이트 한다.
                playerController.SetIsUpdatedDiary(isDiaryUpdated);
            }
        }
    }

    public void OnMouseUp()
    {
        if (InputGuard.BlockWorldInput()) return;
        if (GameManager.isend) // static 접근
        {
            if (diaryUI == null)
                diaryUI = GameObject.Find("Diary").GetComponent<DiaryUIController>();

            if (light != null) light.SetActive(false);

            if (playerController == null)
                playerController = GameObject.Find("PlayerController")?.GetComponent<PlayerController>();

            if (playerController != null)
                playerController.SetIsDiaryCheck(true);

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

        //클릭했을 때 현재 뭉치가 외출 중인가, Sleeping인가에 따라서 마우스 클릭을 막아야한다.
        GamePatternState CurrentPhase = (GamePatternState)playerController.GetCurrentPhase();

        //인터페이스로 빼자
        if (CurrentPhase != GamePatternState.Watching && CurrentPhase != GamePatternState.Sleeping)
        {
            OpenAlert();
            return;
        }

        // Sleeping 페이즈에서 subseq 진입 애니메이션이나 AfterScript 애니메이션이 재생 중이면 diary 열람 막기
        if (CurrentPhase == GamePatternState.Sleeping)
        {
            if (dotController == null)
            {
                dotController = GameObject.FindWithTag("DotController")?.GetComponent<DotController>();
            }
            
            GameObject subDialogueObj = GameObject.Find("SubDialogue");
            bool isSubDialogueActive = subDialogueObj != null && subDialogueObj.activeSelf;
            
            if (dotController != null)
            {
                if (dotController.IsSubDialogueAnimPlaying || dotController.IsAfterScriptPlaying || isSubDialogueActive)
                {
                    return;
                }
            }
            else if (isSubDialogueActive)
            {
                return;
            }
        }

        if (CurrentPhase == GamePatternState.Watching)
        {
            //AtHome일 때 return;
            string WatchState = DataManager.Instance.Watchinginfo.pattern[playerController.GetChapter()];

            EWatching watch;
            if (Enum.TryParse(WatchState, true, out watch))
            {
                if (watch == EWatching.StayAtHome)
                {
                    return;
                }
            }
        }

        isClicked = true;
        //플레이어 정보도 업데이트 한다.
        light.SetActive(false);
        playerController.SetIsDiaryCheck(isClicked);
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
}
