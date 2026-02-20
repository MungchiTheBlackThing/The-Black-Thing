using Assets.Script.DialClass;
using Assets.Script.TimeEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization;
using Unity.VisualScripting;
//여기서 게임 상태 정의 
//하나의 큰 유한 상태 머신 만들 예정
public enum GamePatternState
{
    Watching = 0, //Watching 단계
    MainA,        // Main 다이얼로그 A 단계
    Thinking,     // Thinking 단계
    MainB,        // Main 다이얼로그 B 단계
    Writing,      // Writing 단계
    Play,         //Play 단계
    Sleeping,     //Sleeping 단계
    NextChapter,  //Sleeping 단계가 끝나면 기다리든가, 아님 Skip을 눌러서 Watching으로 넘어갈 수 있음. 
    End,          //이 단계로 넘어가면 오류, 다음단계 0으로 이동해야함.
};
public class GameManager : MonoBehaviour
{
    
    [SerializeField]
    protected DotController dot;
    protected GameState activeState;
    protected SITime sltime;
    protected ObjectManager objectManager;
    protected ScrollManager scrollManager;
    protected CameraZoom cameraZoom;
    [SerializeField]
    protected GamePatternState currentPattern;
    public int TutoNum = 0;
    public SITime CurrentSITime => sltime;

    private DateTime _phaseEndTime = DateTime.MinValue; // 외부에서 remainging time 받아오도록 접근 가능한 프로퍼티

    bool isready = false;
    public float targetTime;
    public SubDialogue subDialogue;
    ScriptList preScriptList;
    public static bool isend = false;
    [SerializeField]
    protected Canvas canvas;
    [SerializeField]
    protected MenuController menu;
    [SerializeField]
    public MainVideo mainVideo;
    [SerializeField]
    public TimeSkipUIController timeSkipUIController;

    private string NextChapterAnchorKey => $"NextChapterAnchor_{Chapter}";
    private string NextChapterSkipKey   => $"NextChapterEnteredBySkip_{Chapter}";


    [SerializeField] private Day8SleepEventController day8;


    private Coroutine subDialogCoroutine;
    private bool isSkipping = false;
    protected Coroutine phaseTimerCoroutine;
    private string currentPhaseTimerKey;

    public ObjectManager ObjectManager
    {
        get { return objectManager; }
    }
    public MenuController Menu
    {
        get { return menu; }
    }
    public ScrollManager ScrollManager
    {
        get { return scrollManager; }
    }
    public CameraZoom CameraZoom
    {
        get { return cameraZoom; }
    }
    public GameState CurrentState
    {
        get { return activeState; }
    }
    public string Time
    {
        get { return sltime.ToString(); }
    }
    public int GetSITime
    {
        get { return (int)sltime; }
    }
    public DotController Dot
    {
        get { return dot; }
    }
    public GamePatternState Pattern
    {
        get { return currentPattern; }
    }
    public GameObject test;
    private Dictionary<GamePatternState, GameState> states;
    public PlayerController pc;
    [SerializeField]
    public GameObject mainDialoguePanel;
    [SerializeField]
    public VideoPlayerController videoController;
    [SerializeField]
    public GameObject subDialoguePanel;
    [SerializeField]
    public Slider loadingProgressBar;
    public int Chapter
    {
        get { return pc.GetChapter(); }
    }
    private static bool endingInitialized = false;

    // [DEBUG] 하루 시작 시각 설정 
    public int dayStartHour = 11;
    public int dayStartMinute = 0;
    private bool _nextPhaseRequested = false;

    protected GameManager()
    {
        states = new Dictionary<GamePatternState, GameState>();
        states[GamePatternState.Watching] = new Watching();
        states[GamePatternState.MainA] = new MainA();
        states[GamePatternState.Thinking] = new Thinking();
        states[GamePatternState.MainB] = new MainB();
        states[GamePatternState.Writing] = new Writing();
        states[GamePatternState.Play] = new Play();
        states[GamePatternState.Sleeping] = new Sleeping();
        states[GamePatternState.NextChapter] = new NextChapter();
        preScriptList = null;
    }
    private void Awake()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
        
    }
    private void Start()
    {
        //Player 단계를 가져온다.
        StartCoroutine(InitMain());
        loadingProgressBar.onValueChanged.AddListener(OnValueChanged);
    }

    private IEnumerator InitMain()
    {
        yield return new WaitForSeconds(0.5f);

        pc = GameObject.FindWithTag("Player").gameObject.GetComponent<PlayerController>();
        pc.nextPhaseDelegate -= ChangeGameState; // 혹시 이미 있으면 제거
        pc.nextPhaseDelegate += ChangeGameState; // 정확히 한 번만 다시 추가
        if (mainDialoguePanel)
        {
            mainDialoguePanel.GetComponent<MainPanel>().InitializePanels();
        }
        objectManager = GameObject.FindWithTag("ObjectManager").gameObject.GetComponent<ObjectManager>();
        scrollManager = GameObject.FindWithTag("MainCamera").gameObject.GetComponent<ScrollManager>();
        cameraZoom = GameObject.FindWithTag("MainCamera").gameObject.GetComponent<CameraZoom>();
        subDialogue = subDialoguePanel.GetComponent<SubDialogue>();

        LoadSceneManager.Instance.OnLoadingUIShown -= HandleLoadingUIShown;
        LoadSceneManager.Instance.OnLoadingUIShown += HandleLoadingUIShown;
        
        if (timeSkipUIController == null)
        {
            timeSkipUIController = GameObject.FindObjectOfType<TimeSkipUIController>();
        }

        InitGame();
    }

    public void OnValueChanged(float value)
    {
        if (value >= 1f)
        {
            Invoke("CloseLoading", 1f);
        }
    }
    void CloseLoading()
    {
        if (loadingProgressBar != null)
        {
            loadingProgressBar.transform.parent.gameObject.SetActive(false);
        }
    }
    public void GoSleep()
    {
        if (pc.GetChapter() == 1)
        {
            Debug.Log("Tuto 다시 시작");
            subDialoguePanel.SetActive(true);
            subDialogue.Tuto_start(118, 1.5f);
            return;
        }
        dot.GoSleep();
    }

    public void BeginSkipPhaseTransition()
    {
        if (GameManager.isend) return;
        if (videoController == null) return;

        // 스킵 시작 순간: 타이머/서브 끊기
        PausePhaseTimer();
        if (!string.IsNullOrEmpty(currentPhaseTimerKey))
        {
            PlayerPrefs.DeleteKey(currentPhaseTimerKey);
            PlayerPrefs.Save();
            currentPhaseTimerKey = null;
        }
        StopSubDial();

        var current = (GamePatternState)pc.GetCurrentPhase();
        var next = (GamePatternState)((int)current + 1);

        // Sleeping -> NextChapter : SkipSleeping(루프) + 페이드인 끝나자마자 NextPhase 커밋
        if (next == GamePatternState.NextChapter)
        {
            videoController.PlaySkipTransition(
                SkipVideoIdx.SkipSleeping,
                looping: true,
                commitAfterFadeIn: () => NextPhase(),
                autoCloseOnEnd: false
            );
            return;
        }

        // 일반 스킵: SkipPhase(논루프) + 페이드인 끝나자마자 NextPhase 커밋 + 영상 끝나면 자동 Close
        videoController.PlaySkipTransition(
            SkipVideoIdx.SkipPhase,
            looping: false,
            commitAfterFadeIn: () => NextPhase(),
            autoCloseOnEnd: true
        );
    }


    public void SetPhase(GamePatternState newPhase)
    {
        currentPattern = newPhase;
        AudioManager.Instance.UpdateBGMByChapter(Chapter, currentPattern);
        Debug.Log($"[SetPhase] Phase 설정됨: {newPhase}");

        var subs = GetSubseqsForPhase(currentPattern);
        if (subs != null && subs.Count > 0)
        {
            Debug.Log($"[SetPhase] Subseq 초기화됨: {subs[0]}");
            pc.SetSubseq(subs[0]);
        }
        else
        {
            pc.SetSubseq(1);
        }
    }
    public void NextPhase()
    {
        dot.ForceStopAfterScript();
        if (_nextPhaseRequested) return;
        _nextPhaseRequested = true;
        pc.NextPhase();
        StartCoroutine(ReleaseNextPhaseRequested());
    }

    private IEnumerator ReleaseNextPhaseRequested()
    {
        yield return null;
        _nextPhaseRequested = false;
    }


    //페이즈 변경
    public void ChangeGameState(GamePatternState patternState)
    {
        EnsureVideoController(); 
        Debug.Log($"[Test] ChangeGameState 실행: {patternState}");
        Debug.Log("스테이트 변경");
        if (states == null) return;
        if (states.ContainsKey(patternState) == false)
        {
            Debug.Log("없는 패턴 입니다.");
            return;
        }
        if (activeState != null)
        {
            dot.StopSubDialogueAnimation();
            activeState.Exit(this); //미리 정리한다.
        }

        // 페이즈 변경 시 이전 페이즈의 서브 다이얼로그 타이머가 돌고 있다면 정지
        if (subDialogCoroutine != null)
        {
            StopCoroutine(subDialogCoroutine);
            subDialogCoroutine = null;
        }

        currentPattern = patternState;

        // 새 페이즈 진입 시 이전 페이즈 알림 정리
        switch (patternState)
        {
            case GamePatternState.Watching:
                NotificationService.Cancel(PushIdType.Watching, Chapter);
                break;
            case GamePatternState.MainA:
                // Watching이 끝나서 MainA 진입 = A 알림 필요 없음
                NotificationService.Cancel(PushIdType.A, Chapter);
                NotificationService.Cancel(PushIdType.A6, Chapter);
                break;

            case GamePatternState.MainB:
                // Thinking이 끝나서 MainB 진입 = B 알림 필요 없음
                NotificationService.Cancel(PushIdType.B, Chapter);
                NotificationService.Cancel(PushIdType.B6, Chapter);
                break;

            case GamePatternState.Play:
                // Writing이 끝나서 Sleeping 진입 = Night 알림 필요 없음
                NotificationService.CancelGlobal(PushIdType.Night);
                break;
        }
        _phaseEndTime = DateTime.MinValue;

        if (phaseTimerCoroutine != null)
        {
            StopCoroutine(phaseTimerCoroutine);
        }
        if (!string.IsNullOrEmpty(currentPhaseTimerKey))
        {
            PlayerPrefs.DeleteKey(currentPhaseTimerKey);
            PlayerPrefs.Save();
        }

        var subs = GetSubseqsForPhase(currentPattern);
        if (subs != null && subs.Count > 0)
        {
            pc.SetSubseq(subs[0]);
            
            // 새로운 페이즈 진입 시 이전 타이머가 남아있다면 제거하여 즉시 실행되는 문제 방지
            string timestampKey = "PendingEventTimestamp_" + Chapter + "_" + currentPattern.ToString() + "_" + subs[0];
            if (PlayerPrefs.HasKey(timestampKey))
            {
                PlayerPrefs.DeleteKey(timestampKey);
                PlayerPrefs.Save();
            }
        }

        activeState = states[patternState];
        Debug.Log("스테이트 변경: " + patternState);
        activeState.Enter(this, dot);
        ApplyPhaseUI(patternState);
        ApplyMoldGateIfNeeded();
        objectManager?.SettingChapter(Chapter, patternState);

        // 1일차 시 페이즈로 처음 진입할 때 다이어리 활성화
        if (patternState == GamePatternState.Play && Chapter == 1 && !pc.IsDiaryUnlockedForChapter1())
        {
            Debug.Log($"[GameManager.ChangeGameState] 1일차 Writing 페이즈 진입 - 다이어리 잠금 해제 및 활성화");
            pc.UnlockDiaryForChapter1();
            if (ObjectManager != null)
            {
                ObjectManager.SettingChapter(Chapter, patternState);
            }
        }

        // 2일차 sleeping 시작 시 다이어리 가이드 활성화 플래그
        if (patternState == GamePatternState.Sleeping && Chapter == 2)
        {
            PlayerPrefs.SetFloat("DiaryGuideEligible", 1f);
            PlayerPrefs.Save();
        }

        // Day8: Sleeping 진입에서만 이벤트 실행
        if (patternState == GamePatternState.Sleeping
            && Chapter == 8
            && day8 != null
            && day8.ShouldRun())
        {
            Debug.Log("DAY8 TRIGGERED");
            day8.TryStart();   // 내부에서 PausePhaseTimer() 처리
            return;            // 아래 PhaseTimer/ShowSubDial이 동시에 돌지 않게 차단
        }


        phaseTimerCoroutine = StartCoroutine(PhaseTimer());

        // 1일차 MainA 종료 후 Thinking 페이즈에 진입하면 UI 튜토리얼 켜기
        if (patternState == GamePatternState.Thinking && Chapter == 1 && pc.GetSubseq() == 1)
        {
            var uiTuto = FindObjectOfType<UITutorial>(true);
            if (uiTuto) uiTuto.gameObject.SetActive(true);
        }


        if (dot.GetSubScriptListCount(patternState) != 0)
        {
            Debug.Log("changeGameState:Sub");
            //서브 실행
            ShowSubDial();
        }
        //C#에서 명시적 형변환은 강제, as 할지말지를 결정.. 즉, 실패 유무를 알고 싶다면, as를 사용한다.
        //activeState가 ILoadingInterface를 구현하고 있는지 확인, 맞다면 loadingInterface에 할당하고 아니면 null
        //현재 상태가 비디오를 재생해야하는 로딩 관련 상태인지 확인하는 역할

        //skip video 재생
        if (currentPattern == GamePatternState.NextChapter)
        {
            videoController.ShowSkipVideo(SkipVideoIdx.SkipSleeping, looping: true);
            
        }
           
    }


    private void ApplyPhaseUI(GamePatternState patternState)
    {
            //대화 페이즈가 아닐 때 TimeSkipUI가 꺼져있다면 켜주기
        //TimeSkipUI 꺼줘야 하는 페이즈 여기서 설정
        if (timeSkipUIController != null)
        {
            bool shouldShowTimeSkip = 
                (patternState != GamePatternState.MainA && 
                patternState != GamePatternState.MainB && 
                patternState != GamePatternState.NextChapter &&
                patternState != GamePatternState.End &&
                patternState != GamePatternState.Play);
            
            if (timeSkipUIController.gameObject.activeSelf != shouldShowTimeSkip)
                timeSkipUIController.gameObject.SetActive(shouldShowTimeSkip);
        }
    }

    private void HandleLoadingUIShown()
    {
        // NextChapter 루프 비디오가 떠있으면 로딩 UI가 덮은 뒤에 닫아버리기
        EnsureVideoController();
        if (videoController != null && videoController.CurrentIdx == SkipVideoIdx.SkipSleeping)
            videoController.Close();
    }

    public void StartMain()
    {

        MainDialogue mainState = (MainDialogue)activeState;
        subDialogue.gameObject.SetActive(false);
        string fileName = "main_ch" + Chapter;
        if (mainState != null)
        {
            if (mainDialoguePanel != null)
            {
                mainDialoguePanel.SetActive(true); // 메인 패널 켜지면
                InputGuard.WorldInputLocked = true;
                DoorController door = FindObjectOfType<DoorController>(); // 문 렌더링 끄기
                if (door != null)
                {
                    door.SetDoorForDialogue(false);
                }
            }
            mainState.StartMain(this, fileName);
        }
    }
    private void InitGame()
    {
        NotificationService.Init();

        var perm = NotificationService.GetPermissionState(forceSync: false);
        if (perm != PushPermissionState.Granted)
        {
            PlayerPrefs.SetInt("PushEnabled", 0);
            PlayerPrefs.Save();
        }
        // 새 게임 시 이전 타이머 기록이 남아있다면 삭제
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            string key = $"PhaseTimer_1_{GamePatternState.Watching}";
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }

        //배경을 업로드한다.
        Int32 hh = Int32.Parse(DateTime.Now.ToString(("HH"))); //현재 시간을 가져온다
        if (hh >= (int)STime.T_DAWN && hh < (int)STime.T_MORNING) //현재시간 >= 3 && 현재시간 <7
        {
            sltime = SITime.Dawn;
        } //현재시간 >= 7&& 현재시간 <4
        else if (hh >= (int)STime.T_MORNING && hh < (int)STime.T_EVENING)
        {
            sltime = SITime.Morning;
        }
        else if (hh >= (int)STime.T_EVENING && hh < (int)STime.T_NIGHT)
        {
            sltime = SITime.Evening;
        }
        else
        {
            sltime = SITime.Night;
        }
        StartCoroutine(LoadDataAsync());
        Debug.Log("시간: " + sltime.ToString());

        // 하루 시작 시각 로드
        if (PlayerPrefs.HasKey("DayStartHour"))
        {
            dayStartHour = PlayerPrefs.GetInt("DayStartHour");
        }
        if (PlayerPrefs.HasKey("DayStartMinute"))
        {
            dayStartMinute = PlayerPrefs.GetInt("DayStartMinute");
        }
        //AudioManager.instance.EnsureAMB(FMODEvents.instance.ambRoom, sltime.ToString());
    }
    IEnumerator LoadDataAsync()
    {
        Debug.Log("[LoadDataAsync] CALLED");
        float totalProgress = 0f;
        float backgroundLoadWeight = 0.5f;  // 배경 로드가 전체 작업의 50% 차지
        float objectLoadWeight = 0.5f;      // 오브젝트 로드가 나머지 50% 차지
        // 비동기적으로 배경 리소스를 로드
        loadingProgressBar.value = 0;
        ResourceRequest loadOperation = Resources.LoadAsync<GameObject>("Background/" + sltime.ToString());
        while (!loadOperation.isDone)
        {
            totalProgress = loadOperation.progress * backgroundLoadWeight;
            loadingProgressBar.value = totalProgress;
            yield return null;
        }
        // 로딩이 완료되면 리소스를 가져와서 Instantiate
        if (loadOperation.asset != null)
        {
            GameObject background = (GameObject)loadOperation.asset;
            Instantiate<GameObject>(background, objectManager.transform);
        }
        else
        {
            Debug.LogError("Background not found!");
        }
        // 풀을 채우는 등 나머지 작업을 수행
        Coroutine objectLoadCoroutine = StartCoroutine(TrackObjectLoadProgress(sltime.ToString(), pc.GetChapter(), objectLoadWeight));
        foreach (var state in states)
        {
            state.Value.Init();
        }
        //코루틴이 끝날때까지 대기
        yield return objectLoadCoroutine;
        loadingProgressBar.value = 1; //모든 작업이 끝났음.
        //GamePatternState patternState = (GamePatternState)pc.GetAlreadyEndedPhase();
        GamePatternState patternState = (GamePatternState)pc.GetCurrentPhase();
        currentPattern = patternState;

        var info = pc.GetPlayerInfo();
        if (info != null && info.endingReached)
        {
            RestoreEndingFromSave(info);
            yield break; // 엔딩이면 여기서 끝. 아래 Enter/Timer/Sub 절대 돌리면 안 됨!!
        }
        Debug.Log($"초기 스테이트 설정: {patternState}");
        
        activeState = states[patternState];
        activeState.Enter(this, dot);
        ApplyPhaseUI(patternState);

        subDialogue.gameObject.SetActive(false);

        if (phaseTimerCoroutine != null) StopCoroutine(phaseTimerCoroutine);
        phaseTimerCoroutine = StartCoroutine(PhaseTimer());


        if (dot.GetSubScriptListCount(patternState) != 0)
        {
            Debug.Log("LoadDataAsync: Starting SubDial");
            ShowSubDial();
        }

        if (patternState == GamePatternState.Thinking)
        {
            dot.UpdateIdleAnimation();
        }
        DoorController door = FindObjectOfType<DoorController>();
        if (door != null)
        {
            door.SetDoorForDialogue(true);
        }
        ApplyMoldGateIfNeeded();

        EnsureVideoController();

        if (currentPattern == GamePatternState.NextChapter)
        {
            videoController.ShowSkipVideo(SkipVideoIdx.SkipSleeping, looping: true);
        }

        if (patternState == GamePatternState.Sleeping
            && Chapter == 8
            && day8 != null
            && day8.ShouldRun())
        {
            Debug.Log("[Day8] Resume trigger on LoadDataAsync");
            day8.TryStart();   // 내부에서 PausePhaseTimer() 처리하도록 유지
            yield break;       
        }
    }

    private void EnsureVideoController()
    {
        if (videoController == null)
            videoController = FindObjectOfType<VideoPlayerController>(true); // inactive 포함
    }

    private void RestoreEndingFromSave(PlayerInfo info)
    {
        // 1) 플래그 동기화
        GameManager.isend = true;
        DeathNoteClick.readDeathnote = (info.deathnoteState == 1);

        // 2) 엔딩에선 뭉치 숨김
        if (dot != null) dot.gameObject.SetActive(false);

        // 3) UI 오버라이드
        var menu = GameObject.Find("Menu")?.GetComponent<MenuController>();
        if (menu != null)
            menu.ApplyEndingOverride();

        
        if (GameObject.Find("deathnote") == null) 
            {
                GameObject deathnote = Instantiate(Resources.Load<GameObject>(((SITime)GetSITime) + "/deathnote"));
                deathnote.name = "deathnote"; 
            }
        var moonRadio = FindObjectOfType<MoonRadio>(true);
        if (moonRadio != null) moonRadio.ApplyEndingMoonRadioLock();
        
    }
    IEnumerator TrackObjectLoadProgress(string path, int chapter, float weight)
    {
        float progress = 0f;
        float previousProgress = 0f;
        // objectManager의 비동기 작업 진행 상황을 추적
        Coroutine loadObjectCoroutine = StartCoroutine(objectManager.LoadObjectAsync(sltime.ToString(), pc.GetChapter()));
        // objectManager.LoadObjectAsync 코루틴의 진행 상황을 추적 (가정: objectManager에서 진행 상황을 제공할 수 있는 메서드를 제공한다고 가정)
        while (!objectManager.IsLoadObjectComplete())
        {
            progress = objectManager.GetLoadProgress();  // 진행 상황을 가져옴
            float totalProgress = (previousProgress + progress) * weight + loadingProgressBar.value;
            loadingProgressBar.value = totalProgress;
            yield return null;
        }
        // 코루틴이 완료되었을 때 100%로 설정
        loadingProgressBar.value += weight;
    }
    public void SceneTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }
    //준현아 서브 끝나고 나서 showSubDial 을 바로 호출하면, 알아서 n분 대기 후 또 등장할거야 
    //서브가 있든 없든 호출 ㄱㄱ 없으면, Interface상에 걸려서 이전 했던 행동 하고 끝낼겨
    public void ShowSubDial()
    {
        Debug.Log("ShowSubDial 실행됨");

        int currentSubseq = pc.GetSubseq();
        if (!ShouldShowSub(Pattern, currentSubseq))
        {
            Debug.Log("이미 본 서브거나 해당 phase에 서브 없음");
            return;
        }

        if (subDialogCoroutine != null)
            StopCoroutine(subDialogCoroutine);

        isSkipping = false;
        subDialogCoroutine = StartCoroutine(SubDialog(dot));
    }
    bool ShouldShowSub(GamePatternState phase, int subseq)
    {
        var subs = GetSubseqsForPhase(phase);
        if (subs == null || !subs.Contains(subseq))
            return false;

        return !pc.IsSubWatched(subseq); // 저장된 subseq 리스트에 없는지 확인
    }

    public void StopSubDial()
    {
        Debug.Log("StopSubDial 실행됨");

        if (subDialogCoroutine != null)
        {
            StopCoroutine(subDialogCoroutine);
            subDialogCoroutine = null;
        }

        isSkipping = true;
    }

    IEnumerator SubDialog(DotController dot = null)
    {
        yield return null;
        subDialogue.gameObject.SetActive(true);
        subDialogue.gameObject.SetActive(false);

        ScriptList script = dot.GetSubScriptList(Pattern);
        if (script == null)
        {
            IResetStateInterface resetState = CurrentState as IResetStateInterface;
            if (resetState != null)
            {
                resetState.ResetState(this, dot);
            }
            yield break;
        }

        Debug.Log($"SubDialog 진입 - Phase: {Pattern}, Subseq: {pc.GetSubseq()}");

        string timestampKey = "PendingEventTimestamp_" + Chapter + "_" + Pattern.ToString() + "_" + pc.GetSubseq();
        string timestampStr = PlayerPrefs.GetString(timestampKey, "");
        DateTime triggerTime;

        if (string.IsNullOrEmpty(timestampStr))
        {
            dot.TriggerSub(false);
            //[DEBUG] 서브 타이머 단축 script.Delay * 60f -> script.Delay * 0.5f
            float delay = script.Delay * 60f;
            triggerTime = DateTime.Now.AddSeconds(delay);
            PlayerPrefs.SetString(timestampKey, triggerTime.ToBinary().ToString());
            PlayerPrefs.Save();
            Debug.Log($"[저장] 새 이벤트 타이머 저장됨: {triggerTime}, 대기 시간: {delay}초");
        }
        else
        {
            triggerTime = DateTime.FromBinary(Convert.ToInt64(timestampStr));
            Debug.Log($"[로드] 이벤트 트리거 시간 로드: {triggerTime}");
        }

        while (true)
        {
            if (isSkipping)
            {
                PlayerPrefs.DeleteKey(timestampKey);
                PlayerPrefs.Save();
                yield break;
            }

            bool timeDone = DateTime.Now >= triggerTime;
            bool tutoDone = SubDialogue.isSubmoldtutoend;

            if (timeDone && tutoDone)
            {
                break;
            }

            yield return null;
        }

        if (timeSkipUIController != null) timeSkipUIController.SetTime(0);

        if (!isSkipping)
        {
            Debug.Log("시간 경과! 현재 스크립트 키: " + script.ScriptKey);
            dot.TriggerSub(true, script.DotAnim, script.DotPosition);
            pc.ProgressSubDial(script.ScriptKey);
        }
    }

    public IEnumerator DelayedAction(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    private void ApplyMoldGateIfNeeded()
    {
        if (currentPattern == GamePatternState.Thinking
            && Chapter == 1
            && pc != null
            && pc.GetSubseq() == 1)
        {
            menu?.moldOn(); // 스킵/타임 끄는 기존 로직
        }
    }

    public void SkipSubDialWaitAndShowNow()
    {
        int currentSubseq = pc.GetSubseq();
        if (!ShouldShowSub(Pattern, currentSubseq))
        {
            return;
        }

        // 대기 시간 스킵: 트리거 시간을 지금으로 당김
        string timestampKey = "PendingEventTimestamp_" + Chapter + "_" + Pattern.ToString() + "_" + currentSubseq;
        PlayerPrefs.SetString(timestampKey, DateTime.Now.ToBinary().ToString());
        PlayerPrefs.Save();

        // 이미 서브 대기 코루틴이 돌고 있으면 재시작
        if (subDialogCoroutine != null)
        {
            StopCoroutine(subDialogCoroutine);
            subDialogCoroutine = null;
        }

        isSkipping = false;
        subDialogCoroutine = StartCoroutine(SubDialog(dot));
    }


    protected IEnumerator PhaseTimer()
    {
        float duration = GetPhaseDuration(currentPattern);

        if (duration <= 0)
        {
            if (timeSkipUIController != null) timeSkipUIController.SetTime(0);
            yield break;
        }

        currentPhaseTimerKey = $"PhaseTimer_{Chapter}_{currentPattern}";
        DateTime endTime;

        if (PlayerPrefs.HasKey(currentPhaseTimerKey))
        {
            long binaryTime = Convert.ToInt64(PlayerPrefs.GetString(currentPhaseTimerKey));
            endTime = DateTime.FromBinary(binaryTime);

            if (endTime <= DateTime.Now)
            {
                // 이미 시간이 지난 타이머는 리셋하지 않는다: 즉시 캐치업(다음 페이즈)
                PlayerPrefs.DeleteKey(currentPhaseTimerKey);
                PlayerPrefs.Save();

                if (timeSkipUIController != null) timeSkipUIController.SetTime(0);

                Debug.Log($"[PhaseTimer] endTime already passed ({currentPattern}). Immediate catch-up transition.");

                // Sleeping은 NextChapter 오버레이로(기존 흐름 유지)
                if (currentPattern == GamePatternState.Sleeping)
                {
                    // 자연 종료로 NextChapter 오버레이 진입: anchor = Sleeping endTime(=dayStart), skip=false
                    PlayerPrefs.SetString(NextChapterAnchorKey, endTime.ToBinary().ToString());
                    PlayerPrefs.SetInt(NextChapterSkipKey, 0);
                    PlayerPrefs.Save();

                    BeginSkipPhaseTransition();
                    yield break;
                }
                else
                {
                    NextPhase();
                }

                yield break; // 중요: 아래 while로 내려가지 않게 차단
            }
            _phaseEndTime = endTime;
        }
        else
        {
            endTime = DateTime.Now.AddSeconds(duration);
            PlayerPrefs.SetString(currentPhaseTimerKey, endTime.ToBinary().ToString());
            PlayerPrefs.Save();
            _phaseEndTime = endTime;

            if (PlayerPrefs.GetInt("PushEnabled", 0) == 1)
                PushScheduler.ScheduleForCurrentPhase(this); // 푸시 알럿 예약 - 푸시 안 오면 여기 걸 바꿔야 함
        }

        while (DateTime.Now < endTime)
        {
            float remainingTime = (float)(endTime - DateTime.Now).TotalSeconds;
            if (remainingTime < 0) remainingTime = 0;

            if (timeSkipUIController != null) timeSkipUIController.SetTime(remainingTime);
            yield return null;
        }

        if (timeSkipUIController != null) timeSkipUIController.SetTime(0);
        Debug.Log("시간떔에 다음 페이즈 넘어감");
        if (currentPattern == GamePatternState.Sleeping)
        {
            BeginSkipPhaseTransition(); // 이 함수가 다음이 NextChapter면 SkipSleeping 루프로 처리
        }
        else
        {
            NextPhase();
        }
    }

        private void SetPhaseTimerEnd(GamePatternState phase, DateTime endTime)
    {
        string key = $"PhaseTimer_{Chapter}_{phase}";
        PlayerPrefs.SetString(key, endTime.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    public void ForceSetPhase(GamePatternState phase)
    {

        PausePhaseTimer(); //안전하게...
        // 현재 NextChapter에서 빠져나가므로, 영상 루프 닫기
        EnsureVideoController();

        // pc 상태 강제 세팅 + subseq 초기화는 기존 SetPhase 사용
        pc.SetCurrentPhase(phase);
        SetPhase(phase);

        // 실제 씬 상태 진입
        ChangeGameState(phase);
    }

    public void CommitNextChapterLanding()
    {
        // 0) 현재(=NextChapter 들어오기 전) 챕터를 고정해 둔다.
        int chBefore = Chapter;
        string anchorKey = $"NextChapterAnchor_{chBefore}";
        string skipKey   = $"NextChapterEnteredBySkip_{chBefore}";

        if (!PlayerPrefs.HasKey(anchorKey))
        {
            Debug.LogWarning("[NextChapterLanding] Anchor missing. Fallback to NextPhase().");
            NextPhase(); // 그래도 챕터는 넘어가게
            return;
        }

        // 1) 앵커/플래그를 먼저 로컬로 확보 (NextPhase가 뭔가 지워도 괜찮게)
        DateTime now = DateTime.Now;

        long anchorBin = Convert.ToInt64(PlayerPrefs.GetString(anchorKey));
        DateTime anchor = DateTime.FromBinary(anchorBin);
        bool enteredBySkip = PlayerPrefs.GetInt(skipKey, 0) == 1;

        if (enteredBySkip) anchor = now;
        DateTime watchingEnd = anchor.AddHours(2);

        // 2) 먼저 "진짜" NextPhase를 실행해서 챕터+1 커밋(기존 안전 로직 유지)
        NextPhase(); // NextChapter -> (phase overflow) -> Chapter++ & Watching 세팅이 여기서 일어남

        // 3) 커밋 직후, 안착을 시간 기반으로 오버라이드
        if (now < watchingEnd)
        {
            // 1) 일단 안착(이 과정에서 PhaseTimer가 now+2h로 잠깐 생성될 수 있음)
            ForceSetPhase(GamePatternState.Watching);

            // 2) 바로 PhaseTimer를 끊고(레이스 종료)
            PausePhaseTimer();

            // 3) 원하는 endTime으로 덮어쓴 뒤
            SetPhaseTimerEnd(GamePatternState.Watching, watchingEnd);

            // 4) 그 endTime 기준으로 PhaseTimer를 다시 시작
            ResumePhaseTimer();

            if (timeSkipUIController != null)
            {
                float remaining = (float)(watchingEnd - now).TotalSeconds;
                if (remaining < 0) remaining = 0;
                timeSkipUIController.SetTime(remaining);
            }
        }
        else
        {
            ForceSetPhase(GamePatternState.MainA);

            // 새 챕터의 Watching 타이머 혹시 생겼으면 제거(안전)
            string wKey = $"PhaseTimer_{Chapter}_{GamePatternState.Watching}";
            if (PlayerPrefs.HasKey(wKey)) PlayerPrefs.DeleteKey(wKey);
            PlayerPrefs.Save();
        }

        // 4) 스탬프 청소(이전 챕터 키를 지워야 함)
        PlayerPrefs.DeleteKey(anchorKey);
        PlayerPrefs.DeleteKey(skipKey);
        PlayerPrefs.Save();
    }



    public void PausePhaseTimer()
    {
        if (phaseTimerCoroutine != null)
        {
            StopCoroutine(phaseTimerCoroutine);
            phaseTimerCoroutine = null;
        }
    }

    public void ResumePhaseTimer()
    {
        if (phaseTimerCoroutine == null)
            phaseTimerCoroutine = StartCoroutine(PhaseTimer());
    }


    // [DEBUG] 각 페이즈 당 시간
    private float GetPhaseDuration(GamePatternState phase)
    {
        switch (phase)
        {
            case GamePatternState.Watching: return 2 * 3600f; // 2시간
            case GamePatternState.Thinking: return 3 * 3600f; // 3시간
            case GamePatternState.Writing: return 1 * 3600f; // 1시간
            case GamePatternState.Sleeping:
                //하루 시작 시각 설정에 따른 Sleeping 시간 계산
                DateTime now = DateTime.Now;
                DateTime target = new DateTime(now.Year, now.Month, now.Day, dayStartHour, dayStartMinute, 0);

                //현재 시각이 설정된 시작 시각보다 지났다면, 다음 날 시작 시각을 목표로
                if (now >= target)
                {
                    target = target.AddDays(1);
                }
                return (float)(target - now).TotalSeconds;

            // MainA, MainB, EventPoem 등 타이머 멈춰야할 때 (수정해야 함)
            case GamePatternState.MainA:
            case GamePatternState.MainB:
            case GamePatternState.Play:
            case GamePatternState.NextChapter:
            case GamePatternState.End:
                return 0f;

            default: return 0f;
        }
    }

    //챕터별/페이즈별 Subseq 리스트 반환
    public List<int> GetSubseqsForPhase(GamePatternState phase)
    {
        //예외: 14챕터
        if (Chapter == 14)
        {
            if (phase == GamePatternState.Watching) return new List<int> { 1, 2 };
            if (phase == GamePatternState.Thinking) return new List<int> { 3, 4 };
            return new List<int>();
        }

        // 기본 패턴
        switch (phase)
        {
            case GamePatternState.Thinking: return new List<int> { 1, 2 };
            case GamePatternState.Writing: return new List<int> { 3 };
            case GamePatternState.Sleeping: return new List<int> { 4 };
            default: return new List<int>();
        }
    }

    // 하루 시작 시각 설정 및 저장 (UI에서 호출)
    public void SetDayStartTime(int hour, int minute)
    {
        dayStartHour = Mathf.Clamp(hour, 0, 23);
        dayStartMinute = Mathf.Clamp(minute, 0, 59);
        PlayerPrefs.SetInt("DayStartHour", dayStartHour);
        PlayerPrefs.SetInt("DayStartMinute", dayStartMinute);
        PlayerPrefs.Save();

        // Sleeping 페이즈일 때 실시간으로 타이머 재계산
        if (currentPattern == GamePatternState.Sleeping)
        {
            RestartSleepingTimer();
        }
    }

    public void PlayAllSubDialogs()
    {
        List<int> phaseSubs = GetSubseqsForPhase(Pattern);
        if (phaseSubs != null && phaseSubs.Count > 0)
        {
            int currentIdx = phaseSubs.IndexOf(pc.GetSubseq());
            if (currentIdx + 1 < phaseSubs.Count)
            {
                pc.SetSubseq(phaseSubs[currentIdx + 1]); // 다음 서브
            }
        }
    }

    public void RestartSleepingTimer()
    {
        // Sleeping 타이머 키 강제 리셋
        string key = $"PhaseTimer_{Chapter}_{GamePatternState.Sleeping}";
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        // 기존 코루틴 정리 후 다시 시작
        if (phaseTimerCoroutine != null)
        {
            StopCoroutine(phaseTimerCoroutine);
            phaseTimerCoroutine = null;
        }

        currentPattern = GamePatternState.Sleeping; // 안전
        phaseTimerCoroutine = StartCoroutine(PhaseTimer());
    }



    public void StartTutoMain()
    {
        MainDialogue mainState = (MainDialogue)activeState;
        string fileName = "tutorial_main";
        if (mainState != null)
        {
            if (mainDialoguePanel != null)
            {
                mainDialoguePanel.SetActive(true);
            }
            mainState.StartMain(this, fileName);
        }
    }

    public void SubContinue()
    {
        subDialoguePanel.GetComponent<SubDialogue>().Tuto_start(79, 2.0f);
    }
    public void Delay(string function, float delay)
    {
        Invoke(function, delay);
    }
    public void Ending()
    {
        GameManager.isend = true;
        if (pc != null)
        {
            var info = pc.GetPlayerInfo();
            info.endingReached = true;
            pc.SavePlayerInfo();
        }
        if (!endingInitialized)
        {
            DeathNoteClick.readDeathnote = false;
            endingInitialized = true;
        }
        var menu = GameObject.Find("Menu")?.GetComponent<MenuController>();
        if (menu != null)
            menu.ApplyEndingOverride();
        dot.gameObject.SetActive(false);
        GameObject deathnote = Instantiate(Resources.Load<GameObject>((SITime)GetSITime + "/deathnote"));
        foreach (var door in FindObjectsOfType<DoorController>()) //문 꺼지는 오류 가끔 있어서 확실히 켜 주기
        {
            door.SetDoorForDialogue(true);
        }
        var moonRadio = FindObjectOfType<MoonRadio>(true);
        if (moonRadio != null)
            moonRadio.ApplyEndingMoonRadioLock();
    }
    private void OnDestroy()
    {
        if (LoadSceneManager.Instance != null)
            LoadSceneManager.Instance.OnLoadingUIShown -= HandleLoadingUIShown;
        if (pc != null) pc.nextPhaseDelegate -= ChangeGameState;
    }

    public double GetPhaseRemainingSeconds()
    {
        if (_phaseEndTime == DateTime.MinValue) return 0;
        double r = (_phaseEndTime - DateTime.Now).TotalSeconds;
        return r > 0 ? r : 0;
    }
}