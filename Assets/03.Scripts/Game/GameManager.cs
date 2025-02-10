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
    protected SITime time;
    protected ObjectManager objectManager;
    protected ScrollManager scrollManager;
    protected CameraZoom cameraZoom;
    [SerializeField]
    protected GamePatternState currentPattern;
    public int TutoNum = 0;


    ScriptList preScriptList;

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
        get { return time.ToString(); }
    }
    public DotController Dot
    {
        get { return dot; }
    }
    public GamePatternState Pattern
    {
        get { return currentPattern; }
    }
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
        pc = GameObject.FindWithTag("Player").gameObject.GetComponent<PlayerController>();
        pc.nextPhaseDelegate += ChangeGameState;
        objectManager = GameObject.FindWithTag("ObjectManager").gameObject.GetComponent<ObjectManager>();
        scrollManager = GameObject.FindWithTag("MainCamera").gameObject.GetComponent<ScrollManager>();
        cameraZoom = GameObject.FindWithTag("MainCamera").gameObject.GetComponent<CameraZoom>();

    }
    private void Start()
    {
        //Player 단계를 가져온다.
        if (mainDialoguePanel)
        {
            mainDialoguePanel.GetComponent<MainPanel>().InitializePanels();
        }
        InitGame();
        loadingProgressBar.onValueChanged.AddListener(OnValueChanged);
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
        dot.GoSleep();
    }
    public void NextPhase()
    {
        pc.NextPhase();
    }
    public void ChangeGameState(GamePatternState patternState)
    {
       
        if (states == null) return;
        if (states.ContainsKey(patternState) == false)
        {
            Debug.Log("없는 패턴 입니다.");
            return;
        }
        if (activeState != null)
        {
            activeState.Exit(this); //미리 정리한다.
        }
        currentPattern = patternState;
        activeState = states[patternState];
        activeState.Enter(this, dot);

        if(dot.GetSubScriptListCount(patternState) != 0)
        {
            Debug.Log("changeGameState:Sub");
            //서브 실행
            ShowSubDial();
        }
        //C#에서 명시적 형변환은 강제, as 할지말지를 결정.. 즉, 실패 유무를 알고 싶다면, as를 사용한다.
        ILoadingInterface loadingInterface = activeState as ILoadingInterface;
        if (loadingInterface != null)
        {
            EVideoIdx VideoIdx = currentPattern == GamePatternState.NextChapter ? EVideoIdx.SkipSleeping : EVideoIdx.SkipPhase;
            bool IsLooping = VideoIdx == EVideoIdx.SkipSleeping ? true : false;
            videoController.ShowVideo(VideoIdx, IsLooping);
        }
    }
    public void StartMain()
    {
        MainDialogue mainState = (MainDialogue)activeState;
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
            time = SITime.Dawn;
        } //현재시간 >= 7&& 현재시간 <4
        else if (hh >= (int)STime.T_MORNING && hh < (int)STime.T_EVENING)
        {
            time = SITime.Morning;
        }
        else if (hh >= (int)STime.T_EVENING && hh < (int)STime.T_NIGHT)
        {
            time = SITime.Evening;
        }
        else
        {
            time = SITime.Night;
        }
        StartCoroutine(LoadDataAsync());
    }
    IEnumerator LoadDataAsync()
    {
        float totalProgress = 0f;
        float backgroundLoadWeight = 0.5f;  // 배경 로드가 전체 작업의 50% 차지
        float objectLoadWeight = 0.5f;      // 오브젝트 로드가 나머지 50% 차지
        // 비동기적으로 배경 리소스를 로드
        loadingProgressBar.value = 0;
        ResourceRequest loadOperation = Resources.LoadAsync<GameObject>("Background/" + time.ToString());
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
        Coroutine objectLoadCoroutine = StartCoroutine(TrackObjectLoadProgress(time.ToString(), pc.GetChapter(), objectLoadWeight));
        foreach (var state in states)
        {
            state.Value.Init();
        }
        //코루틴이 끝날때까지 대기
        yield return objectLoadCoroutine;
        loadingProgressBar.value = 1; //모든 작업이 끝났음.
        GamePatternState patternState = (GamePatternState)pc.GetAlreadyEndedPhase();
        currentPattern = patternState;
        activeState = states[patternState];
        activeState.Enter(this, dot);
    }
    IEnumerator TrackObjectLoadProgress(string path, int chapter, float weight)
    {
        float progress = 0f;
        float previousProgress = 0f;
        // objectManager의 비동기 작업 진행 상황을 추적
        Coroutine loadObjectCoroutine = StartCoroutine(objectManager.LoadObjectAsync(time.ToString(), pc.GetChapter()));
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
        Debug.Log("showsubdial");
        StartCoroutine(SubDialog(dot));
    }

    IEnumerator SubDialog(DotController dot = null)
    {
        ScriptList script = dot.GetSubScriptList(Pattern); //현재 몇번째 서브 진행중인지 체크

        if (script == null)
        {
            //sub가 끝나면 Sleeping에 대한 동작을 수행하겠지...
            //현재 패턴에 대해 더이상 없으면... 
            IResetStateInterface resetState = CurrentState as IResetStateInterface;
            if (resetState != null)
            {
                resetState.ResetState(this, dot);
            }
            yield break;
        }

        //playercontroller SetSubPhase 호출
        // Task.Delay를 사용하여 10분 대기 (600,000 밀리초 = 10분)
        //await Task.Delay(TimeSpan.FromMinutes(10));
        // 10분 후에 호출되는 작업
        
        dot.TriggerSub(true);
        pc.ProgressSubDial(script.ScriptKey);

        preScriptList = script;

        //시간 나중에 설정 예정 - 추후 해야하는 일
        yield return new WaitForSeconds(100f);

        dot.EndSubScriptList(Pattern);
        script = dot.GetSubScriptList(Pattern); //현재 몇번째 서브 진행중인지 체크

        if (script != null && script != preScriptList)
        {
            //시간이 지나서 다음 서브가 등장해야 함.
            CurrentState.RunSubScript(dot, this);
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
        subDialoguePanel.GetComponent<SubDialogue>().SubContinue();
    }
    public void Delay(string function, float delay)
    {
        Invoke(function, delay);
    }
}