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
//���⼭ ���� ���� ���� 
//�ϳ��� ū ���� ���� �ӽ� ���� ����
public enum GamePatternState
{
    Watching = 0, //Watching �ܰ�
    MainA, // Main ���̾�α� A �ܰ�
    Thinking, // Thinking �ܰ�
    MainB, // Main ���̾�α� B �ܰ�
    Writing, // Writing �ܰ�
    Play, //Play �ܰ�
    Sleeping, //Sleeping �ܰ�
    NextChapter, //Sleeping �ܰ谡 ������ ��ٸ��簡, �ƴ� Skip�� ������ Watching���� �Ѿ �� ����. 
    End,//�� �ܰ�� �Ѿ�� ����, �����ܰ� 0���� �̵��ؾ���.
};

public class GameManager : MonoBehaviour
{
    [SerializeField]
    protected DotController dot;

    protected GameState activeState;

    protected SITime time;
    protected ObjectManager objectManager;
    protected ScrollManager scrollManager;
    protected GamePatternState currentPattern;

    public ObjectManager ObjectManager
    {
        get { return objectManager; }
    }

    public ScrollManager ScrollManager
    {
        get { return scrollManager; }
    }

    public GameState CurrentState
    {
        get { return activeState; }
    }

    public string Time
    {
        get { return time.ToString(); }
    }
    public GamePatternState Pattern
    {
        get { return currentPattern; }
    }
    private Dictionary<GamePatternState, GameState> states;
    public PlayerController pc;


    public delegate void OnVideoEndedDelegate();

    OnVideoEndedDelegate onVideoEnded;
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
        onVideoEnded += OnVideoCompleted;
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

    public void OnVideoCompleted()
    {
        //����
        if (currentPattern == GamePatternState.Play)
        {
            videoController.CloseVideo(EVideoIdx.SkipSleeping, true);
        }
        else
        {
            videoController.CloseVideo(EVideoIdx.SkipPhase, false);
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

        //C#���� ����� ����ȯ�� ����, as ���������� ����.. ��, ���� ������ �˰� �ʹٸ�, as�� ����Ѵ�.
        ILoadingInterface loadingInterface = activeState as ILoadingInterface;

        if (loadingInterface != null)
        {
            videoController.ShowVideo();
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

        OnVideoCompleted(); //�̸� ������ �ε�
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
        if (dot.GetSubScriptListCount(Pattern) == 0)
        {
            //sub�� ������ Sleeping�� ���� ������ �����ϰ���...
            //���� ���Ͽ� ���� ���̻� ������... 
            IResetStateInterface resetState = CurrentState as IResetStateInterface;

            if (resetState != null)
            {
                resetState.ResetState(this, dot);
            }
            yield return null;
        }

        yield return new WaitForSeconds(5f);

        //playercontroller SetSubPhase ȣ��
        // Task.Delay�� ����Ͽ� 10�� ��� (600,000 �и��� = 10��)
        //await Task.Delay(TimeSpan.FromMinutes(10));

        // 10�� �Ŀ� ȣ��Ǵ� �۾�
        ScriptList script = dot.GetSubScriptList(Pattern); //���� ���° ���� ���������� üũ

        DotPatternState dotPattern;

        Debug.Log(script.AnimState + " Sub ������");
        if (Enum.TryParse(script.AnimState, true, out dotPattern))
        {

            dot.ChangeState(dotPattern, script.DotAnim, script.DotPosition);
            dot.TriggerSub(true);
        }

    }
}

