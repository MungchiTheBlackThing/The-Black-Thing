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

    private Coroutine subDialogCoroutine;
    private bool isSkipping = false;
    private Coroutine phaseTimerCoroutine;
    private string currentPhaseTimerKey;

    public ObjectManager ObjectManager
    {
        get { return objectManager; }
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

    Dictionary<GamePatternState, List<int>> phaseToSubseqs = new Dictionary<GamePatternState, List<int>>()
    {
    { GamePatternState.Thinking, new List<int>{1, 2} },
    { GamePatternState.Writing,  new List<int>{3} },
    { GamePatternState.Sleeping, new List<int>{4} }
    };

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
        pc.nextPhaseDelegate += ChangeGameState;
        if (mainDialoguePanel)
        {
            mainDialoguePanel.GetComponent<MainPanel>().InitializePanels();
        }
        objectManager = GameObject.FindWithTag("ObjectManager").gameObject.GetComponent<ObjectManager>();
        scrollManager = GameObject.FindWithTag("MainCamera").gameObject.GetComponent<ScrollManager>();
        cameraZoom = GameObject.FindWithTag("MainCamera").gameObject.GetComponent<CameraZoom>();
        subDialogue = subDialoguePanel.GetComponent<SubDialogue>();
        
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
    public void SetPhase(GamePatternState newPhase)
    {
        currentPattern = newPhase;
        AudioManager.Instance.UpdateBGMByChapter(Chapter, currentPattern);
        Debug.Log($"[SetPhase] Phase 설정됨: {newPhase}");

        if (phaseToSubseqs.TryGetValue(currentPattern, out var subs) && subs.Count > 0)
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
        Debug.Log("NextPhase 호출됨");
        pc.NextPhase();
    }


    //페이즈 변경
    public void ChangeGameState(GamePatternState patternState)
    {
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

        if (phaseTimerCoroutine != null)
        {
            StopCoroutine(phaseTimerCoroutine);
        }
        if (!string.IsNullOrEmpty(currentPhaseTimerKey))
        {
            PlayerPrefs.DeleteKey(currentPhaseTimerKey);
            PlayerPrefs.Save();
        }

        if (phaseToSubseqs.TryGetValue(currentPattern, out var subs) && subs.Count > 0)
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
        Debug.Log("[디버깅]스테이트 변경: " + patternState);
        activeState.Enter(this, dot);

        phaseTimerCoroutine = StartCoroutine(PhaseTimer());

        // 1일차 MainA 종료 후 Thinking 페이즈에 진입하면 UI 튜토리얼을 켭니다.
        if (patternState == GamePatternState.Thinking && Chapter == 1 && pc.GetSubseq() == 1)
        {
            var uiTuto = FindObjectOfType<UITutorial>(true);
            if (uiTuto) uiTuto.gameObject.SetActive(true);
        }

        // [수정] 대화 페이즈가 아닐 때 TimeSkipUI가 꺼져있다면 켜줍니다. (1일차 튜토리얼 직후 등 대비)
        if (timeSkipUIController != null)
        {
            bool shouldShowTimeSkip = (patternState != GamePatternState.MainA && 
                                       patternState != GamePatternState.MainB && 
                                       patternState != GamePatternState.NextChapter &&
                                       patternState != GamePatternState.End);
            
            if (timeSkipUIController.gameObject.activeSelf != shouldShowTimeSkip)
                timeSkipUIController.gameObject.SetActive(shouldShowTimeSkip);
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
        ILoadingInterface loadingInterface = activeState as ILoadingInterface;

        //skip video 재생
        if (loadingInterface != null)
        {
            SkipVideoIdx videoIdx = (currentPattern == GamePatternState.NextChapter) ? SkipVideoIdx.SkipSleeping : SkipVideoIdx.SkipPhase;

            // SkipSleeping 비디오는 항상 재생, SkipPhase 비디오는 버튼 클릭 시에만 재생
            if (videoIdx == SkipVideoIdx.SkipSleeping || 
               (videoIdx == SkipVideoIdx.SkipPhase && timeSkipUIController.IsSkipButtonClicked))
            {
                bool isLooping = (videoIdx == SkipVideoIdx.SkipSleeping);
                videoController.ShowSkipVideo(videoIdx, isLooping);
                timeSkipUIController.IsSkipButtonClicked = false;
            }
        }                   
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
        //AudioManager.instance.EnsureAMB(FMODEvents.instance.ambRoom, sltime.ToString());
    }
    IEnumerator LoadDataAsync()
    {
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
        dot.alertOff(); // 초기화 시 알림이 켜져있으면 타이머가 돌지 않으므로, 상태 진입 전에 강제로 끈다
        currentPattern = patternState;
        Debug.Log($"[디버깅]초기 스테이트 설정: {patternState}");
        activeState = states[patternState];
        activeState.Enter(this, dot);
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
        if (!phaseToSubseqs.ContainsKey(phase))
            return false;

        if (!phaseToSubseqs[phase].Contains(subseq))
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
        dot.TriggerSub(false);
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

        ScriptList nxscript = null;
        Debug.Log($"SubDialog 진입 - Phase: {Pattern}, Subseq: {pc.GetSubseq()}");

        // --- NEW UNIFIED TIMER LOGIC ---
        float waitTime = 0f;
        // 각 이벤트에 대한 고유 키를 사용하여 타이머가 겹치지 않도록 합니다.
        string timestampKey = "PendingEventTimestamp_" + Chapter + "_" + Pattern.ToString() + "_" + pc.GetSubseq();
        string timestampStr = PlayerPrefs.GetString(timestampKey, "");

        if (!string.IsNullOrEmpty(timestampStr))
        {
            try
            {
                long temp = Convert.ToInt64(timestampStr);
                DateTime triggerTime = DateTime.FromBinary(temp);

                if (triggerTime > DateTime.Now)
                {
                    waitTime = (float)(triggerTime - DateTime.Now).TotalSeconds;
                }
                else
                {
                    waitTime = 0; // 이미 시간이 지났으면 바로 실행
                }
                Debug.Log($"[로드] 이벤트 트리거 시간 로드: {triggerTime}, 남은 시간: {waitTime}초");
            }
            catch (Exception e)
            {
                Debug.LogError($"이벤트 트리거 시간 변환 오류: {e.Message}. 스크립트 Delay 값을 사용합니다.");
                waitTime = script.Delay * 0.5f; // 문제가 생기면 스크립트의 Delay 값(분)으로 복구 [디버그] script.Delay * 60f -> script.Delay * 0.5f
                DateTime newTriggerTime = DateTime.Now.AddSeconds(waitTime);
                PlayerPrefs.SetString(timestampKey, newTriggerTime.ToBinary().ToString());
                PlayerPrefs.Save();
            }
        }
        else
        {
            waitTime = script.Delay * 0.5f;  //[디버그] script.Delay * 60f -> script.Delay * 0.5f
            if (waitTime > 0) // Delay가 0초 이상일 때만 타이머 저장
            {
                DateTime triggerTime = DateTime.Now.AddSeconds(waitTime);
                PlayerPrefs.SetString(timestampKey, triggerTime.ToBinary().ToString());
                PlayerPrefs.Save();
                Debug.Log($"[저장] 이벤트 트리거 시간 저장됨: {triggerTime}, 대기 시간: {waitTime}초");
            }
        }

        if (waitTime > 0f)
        {
            float elapsed = 0f;
            while (elapsed < waitTime)
            {
                if (isSkipping) {
                    PlayerPrefs.DeleteKey(timestampKey); // 스킵 시 타이머 정보 삭제
                    PlayerPrefs.Save();
                    yield break;
                }
                yield return null;
                elapsed += UnityEngine.Time.deltaTime;
            }
        }

        if (timeSkipUIController != null) timeSkipUIController.SetTime(0);

        PlayerPrefs.DeleteKey(timestampKey); // 타이머 만료 후 정보 삭제
        PlayerPrefs.Save();

        if (!isSkipping)
        {
            Debug.Log("시간 경과! 현재 스크립트 키: " + script.ScriptKey);
            dot.TriggerSub(true, script.DotAnim, script.DotPosition);
            pc.ProgressSubDial(script.ScriptKey);
        }
    }

    IEnumerator PhaseTimer()
    {
        float duration = GetPhaseDuration(currentPattern);
        if (duration <= 0)
        {
            if (timeSkipUIController != null) timeSkipUIController.SetTime(0);
            yield break;
        }

        currentPhaseTimerKey = $"PhaseTimer_{Chapter}_{currentPattern}";
        float remainingTime = duration;

        if (PlayerPrefs.HasKey(currentPhaseTimerKey))
        {
            long binaryTime = Convert.ToInt64(PlayerPrefs.GetString(currentPhaseTimerKey));
            DateTime endTime = DateTime.FromBinary(binaryTime);
            remainingTime = (float)(endTime - DateTime.Now).TotalSeconds;
        }
        else
        {
            DateTime endTime = DateTime.Now.AddSeconds(duration);
            PlayerPrefs.SetString(currentPhaseTimerKey, endTime.ToBinary().ToString());
            PlayerPrefs.Save();
        }

        while (remainingTime > 0)
        {
            if (timeSkipUIController != null) timeSkipUIController.SetTime(remainingTime);
            yield return null;
            remainingTime -= UnityEngine.Time.deltaTime;
        }

        if (timeSkipUIController != null) timeSkipUIController.SetTime(0);
        
        NextPhase();
    }


// [디버그] 각 페이즈 당 시간
    private float GetPhaseDuration(GamePatternState phase)
    {
        switch (phase)
        {
            case GamePatternState.Watching: return 2 * 3600f; // 2시간
            case GamePatternState.Thinking: return 3 * 3600f; // 3시간
            case GamePatternState.Writing: return 1 * 3600f; // 1시간
            case GamePatternState.Sleeping: return 1 * 3600f; // 1시간
            default: return 0f;
        }
    }
    public void PlayAllSubDialogs()
    {
        List<int> phaseSubs = phaseToSubseqs.ContainsKey(Pattern) ? phaseToSubseqs[Pattern] : null;
        if (phaseSubs != null && phaseSubs.Count > 0)
        {
            int currentIdx = phaseSubs.IndexOf(pc.GetSubseq());
            if (currentIdx + 1 < phaseSubs.Count)
            {
                pc.SetSubseq(phaseSubs[currentIdx + 1]); // 다음 서브
            }
        }
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
        isend = true;
        string resourcePath = "Ending/end_animation";
        GameObject endani = Instantiate(Resources.Load<GameObject>(resourcePath), canvas.transform);
        endani.transform.SetAsLastSibling();
        dot.gameObject.SetActive(false);
        GameObject deathnote = Instantiate(Resources.Load<GameObject>((SITime)GetSITime + "/deathnote"));
    }
}