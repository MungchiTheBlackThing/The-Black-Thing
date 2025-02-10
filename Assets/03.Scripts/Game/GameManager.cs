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
//���⼭ ���� ���� ���� 
//�ϳ��� ū ���� ���� �ӽ� ���� ����
public enum GamePatternState
{
    Watching = 0, //Watching �ܰ�
    MainA,        // Main ���̾�α� A �ܰ�
    Thinking,     // Thinking �ܰ�
    MainB,        // Main ���̾�α� B �ܰ�
    Writing,      // Writing �ܰ�
    Play,         //Play �ܰ�
    Sleeping,     //Sleeping �ܰ�
    NextChapter,  //Sleeping �ܰ谡 ������ ��ٸ��簡, �ƴ� Skip�� ������ Watching���� �Ѿ �� ����. 
    End,          //�� �ܰ�� �Ѿ�� ����, �����ܰ� 0���� �̵��ؾ���.
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
        //Player �ܰ踦 �����´�.
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
            Debug.Log("���� ���� �Դϴ�.");
            return;
        }
        if (activeState != null)
        {
            activeState.Exit(this); //�̸� �����Ѵ�.
        }
        currentPattern = patternState;
        activeState = states[patternState];
        activeState.Enter(this, dot);

        if(dot.GetSubScriptListCount(patternState) != 0)
        {
            Debug.Log("changeGameState:Sub");
            //���� ����
            ShowSubDial();
        }
        //C#���� ����� ����ȯ�� ����, as ���������� ����.. ��, ���� ������ �˰� �ʹٸ�, as�� ����Ѵ�.
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
        //����� ���ε��Ѵ�.
        Int32 hh = Int32.Parse(DateTime.Now.ToString(("HH"))); //���� �ð��� �����´�
        if (hh >= (int)STime.T_DAWN && hh < (int)STime.T_MORNING) //����ð� >= 3 && ����ð� <7
        {
            time = SITime.Dawn;
        } //����ð� >= 7&& ����ð� <4
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
        float backgroundLoadWeight = 0.5f;  // ��� �ε尡 ��ü �۾��� 50% ����
        float objectLoadWeight = 0.5f;      // ������Ʈ �ε尡 ������ 50% ����
        // �񵿱������� ��� ���ҽ��� �ε�
        loadingProgressBar.value = 0;
        ResourceRequest loadOperation = Resources.LoadAsync<GameObject>("Background/" + time.ToString());
        while (!loadOperation.isDone)
        {
            totalProgress = loadOperation.progress * backgroundLoadWeight;
            loadingProgressBar.value = totalProgress;
            yield return null;
        }
        // �ε��� �Ϸ�Ǹ� ���ҽ��� �����ͼ� Instantiate
        if (loadOperation.asset != null)
        {
            GameObject background = (GameObject)loadOperation.asset;
            Instantiate<GameObject>(background, objectManager.transform);
        }
        else
        {
            Debug.LogError("Background not found!");
        }
        // Ǯ�� ä��� �� ������ �۾��� ����
        Coroutine objectLoadCoroutine = StartCoroutine(TrackObjectLoadProgress(time.ToString(), pc.GetChapter(), objectLoadWeight));
        foreach (var state in states)
        {
            state.Value.Init();
        }
        //�ڷ�ƾ�� ���������� ���
        yield return objectLoadCoroutine;
        loadingProgressBar.value = 1; //��� �۾��� ������.
        GamePatternState patternState = (GamePatternState)pc.GetAlreadyEndedPhase();
        currentPattern = patternState;
        activeState = states[patternState];
        activeState.Enter(this, dot);
    }
    IEnumerator TrackObjectLoadProgress(string path, int chapter, float weight)
    {
        float progress = 0f;
        float previousProgress = 0f;
        // objectManager�� �񵿱� �۾� ���� ��Ȳ�� ����
        Coroutine loadObjectCoroutine = StartCoroutine(objectManager.LoadObjectAsync(time.ToString(), pc.GetChapter()));
        // objectManager.LoadObjectAsync �ڷ�ƾ�� ���� ��Ȳ�� ���� (����: objectManager���� ���� ��Ȳ�� ������ �� �ִ� �޼��带 �����Ѵٰ� ����)
        while (!objectManager.IsLoadObjectComplete())
        {
            progress = objectManager.GetLoadProgress();  // ���� ��Ȳ�� ������
            float totalProgress = (previousProgress + progress) * weight + loadingProgressBar.value;
            loadingProgressBar.value = totalProgress;
            yield return null;
        }
        // �ڷ�ƾ�� �Ϸ�Ǿ��� �� 100%�� ����
        loadingProgressBar.value += weight;
    }
    public void SceneTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }
    //������ ���� ������ ���� showSubDial �� �ٷ� ȣ���ϸ�, �˾Ƽ� n�� ��� �� �� �����Ұž� 
    //���갡 �ֵ� ���� ȣ�� ���� ������, Interface�� �ɷ��� ���� �ߴ� �ൿ �ϰ� ������
    public void ShowSubDial()
    {
        Debug.Log("showsubdial");
        StartCoroutine(SubDialog(dot));
    }

    IEnumerator SubDialog(DotController dot = null)
    {
        ScriptList script = dot.GetSubScriptList(Pattern); //���� ���° ���� ���������� üũ

        if (script == null)
        {
            //sub�� ������ Sleeping�� ���� ������ �����ϰ���...
            //���� ���Ͽ� ���� ���̻� ������... 
            IResetStateInterface resetState = CurrentState as IResetStateInterface;
            if (resetState != null)
            {
                resetState.ResetState(this, dot);
            }
            yield break;
        }

        //playercontroller SetSubPhase ȣ��
        // Task.Delay�� ����Ͽ� 10�� ��� (600,000 �и��� = 10��)
        //await Task.Delay(TimeSpan.FromMinutes(10));
        // 10�� �Ŀ� ȣ��Ǵ� �۾�
        
        dot.TriggerSub(true);
        pc.ProgressSubDial(script.ScriptKey);

        preScriptList = script;

        //�ð� ���߿� ���� ���� - ���� �ؾ��ϴ� ��
        yield return new WaitForSeconds(100f);

        dot.EndSubScriptList(Pattern);
        script = dot.GetSubScriptList(Pattern); //���� ���° ���� ���������� üũ

        if (script != null && script != preScriptList)
        {
            //�ð��� ������ ���� ���갡 �����ؾ� ��.
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