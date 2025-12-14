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
        Debug.Log("1");
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
        currentPattern = patternState;
        activeState = states[patternState];
        Debug.Log("[디버깅]스테이트 변경: " + patternState);
        activeState.Enter(this, dot);

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
                mainDialoguePanel.SetActive(true);
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
        GamePatternState patternState = (GamePatternState)pc.GetAlreadyEndedPhase();
        currentPattern = patternState;
        Debug.Log($"[디버깅]초기 스테이트 설정: {patternState}");
        activeState = states[patternState];
        activeState.Enter(this, dot);
        subDialogue.gameObject.SetActive(false);
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

        if (Pattern == GamePatternState.Writing)
        {
            pc.SetSubseq(3);
        }
        if (Pattern == GamePatternState.Sleeping)
        {
            pc.SetSubseq(4);
        }
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
        if (pc.GetSubseq() == 1)
        {
            isready = false;
            nxscript = dot.GetnxSubScriptList(Pattern);

            Debug.Log("다음 스크립트 시간: " + nxscript.Delay);
            float startTime = UnityEngine.Time.time;
            targetTime = startTime + nxscript.Delay*60;

            Debug.Log("현재 스크립트 시간: " + script.Delay);

            float elapsed = 0f;
            while (elapsed < script.Delay*60)
            {
                if (isSkipping) yield break;
                yield return null;
                elapsed += UnityEngine.Time.deltaTime;
            }

            if (!isSkipping)
            {
                Debug.Log("현재 스크립트 키: " + script.ScriptKey);
                dot.TriggerSub(true);
                pc.ProgressSubDial(script.ScriptKey);
            }
        }
        else if (pc.GetSubseq() == 2)
        {
            Debug.Log("현재 스크립트 키: " + script.ScriptKey);
            float waitTime = targetTime - UnityEngine.Time.time;
            Debug.Log("기다려야 하는 시간: " + waitTime);

            if (waitTime > 0f)
            {
                float elapsed = 0f;
                while (elapsed < waitTime)
                {
                    if (isSkipping) yield break;
                    yield return null;
                    elapsed += UnityEngine.Time.deltaTime;
                }
            }

            if (!isSkipping)
            {
                Debug.Log("서브 2 실행");
                dot.TriggerSub(true);
                pc.ProgressSubDial(script.ScriptKey);
            }
        }
        else
        {
            Debug.Log("현재 스크립트 시간: " + script.Delay);

            float elapsed = 0f;
            while (elapsed < script.Delay*60)
            {
                if (isSkipping) yield break;
                yield return null;
                elapsed += UnityEngine.Time.deltaTime;
            }

            if (!isSkipping)
            {
                Debug.Log("현재 스크립트 키: " + script.ScriptKey);
                dot.TriggerSub(true);
                pc.ProgressSubDial(script.ScriptKey);
            }
        }
        //pc.MarkSubWatched(pc.GetSubseq()); // 본 걸 기록
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