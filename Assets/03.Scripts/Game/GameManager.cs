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
        pc = GameObject.FindWithTag("Player").gameObject.GetComponent<PlayerController>();
        pc.nextPhaseDelegate += ChangeGameState;
        objectManager = GameObject.FindWithTag("ObjectManager").gameObject.GetComponent<ObjectManager>();
        scrollManager = GameObject.FindWithTag("MainCamera").gameObject.GetComponent<ScrollManager>();
        cameraZoom = GameObject.FindWithTag("MainCamera").gameObject.GetComponent<CameraZoom>();
        subDialogue = subDialoguePanel.GetComponent<SubDialogue>();
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
        if (pc.GetChapter() == 1)
        {
            Debug.Log("Tuto �ٽ� ����");
            subDialoguePanel.SetActive(true);
            subDialogue.Tuto_start(118);
            return;
        }
        dot.GoSleep();
    }
    public void SetPhase(GamePatternState newPhase)
    {
        currentPattern = newPhase;
        Debug.Log($"[SetPhase] Phase ������: {newPhase}");

        if (phaseToSubseqs.TryGetValue(currentPattern, out var subs) && subs.Count > 0)
        {
            Debug.Log($"[SetPhase] Subseq �ʱ�ȭ��: {subs[0]}");
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
    public void ChangeGameState(GamePatternState patternState)
    {
        Debug.Log($"[Test] ChangeGameState ����: {patternState}");
        Debug.Log("������Ʈ ����");
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
        //����� ���ε��Ѵ�.
        Int32 hh = Int32.Parse(DateTime.Now.ToString(("HH"))); //���� �ð��� �����´�
        if (hh >= (int)STime.T_DAWN && hh < (int)STime.T_MORNING) //����ð� >= 3 && ����ð� <7
        {
            sltime = SITime.Dawn;
        } //����ð� >= 7&& ����ð� <4
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
    }
    IEnumerator LoadDataAsync()
    {
        float totalProgress = 0f;
        float backgroundLoadWeight = 0.5f;  // ��� �ε尡 ��ü �۾��� 50% ����
        float objectLoadWeight = 0.5f;      // ������Ʈ �ε尡 ������ 50% ����
        // �񵿱������� ��� ���ҽ��� �ε�
        loadingProgressBar.value = 0;
        ResourceRequest loadOperation = Resources.LoadAsync<GameObject>("Background/" + sltime.ToString());
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
        Coroutine objectLoadCoroutine = StartCoroutine(TrackObjectLoadProgress(sltime.ToString(), pc.GetChapter(), objectLoadWeight));
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
        subDialogue.gameObject.SetActive(false);
    }
    IEnumerator TrackObjectLoadProgress(string path, int chapter, float weight)
    {
        float progress = 0f;
        float previousProgress = 0f;
        // objectManager�� �񵿱� �۾� ���� ��Ȳ�� ����
        Coroutine loadObjectCoroutine = StartCoroutine(objectManager.LoadObjectAsync(sltime.ToString(), pc.GetChapter()));
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
        Debug.Log("ShowSubDial �����");

        int currentSubseq = pc.GetSubseq();
        if (!ShouldShowSub(Pattern, currentSubseq))
        {
            Debug.Log("�̹� �� ����ų� �ش� phase�� ���� ����");
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

        return !pc.IsSubWatched(subseq); // ����� subseq ����Ʈ�� ������ Ȯ��
    }

    public void StopSubDial()
    {
        Debug.Log("StopSubDial �����");

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
        Debug.Log($"SubDialog ���� - Phase: {Pattern}, Subseq: {pc.GetSubseq()}");
        if (pc.GetSubseq() == 1)
        {
            isready = false;
            nxscript = dot.GetnxSubScriptList(Pattern);

            Debug.Log("���� ��ũ��Ʈ �ð�: " + nxscript.Delay);
            float startTime = UnityEngine.Time.time;
            targetTime = startTime + nxscript.Delay;

            Debug.Log("���� ��ũ��Ʈ �ð�: " + script.Delay);

            float elapsed = 0f;
            while (elapsed < script.Delay)
            {
                if (isSkipping) yield break;
                yield return null;
                elapsed += UnityEngine.Time.deltaTime;
            }

            if (!isSkipping)
            {
                Debug.Log("���� ��ũ��Ʈ Ű: " + script.ScriptKey);
                dot.TriggerSub(true);
                pc.ProgressSubDial(script.ScriptKey);
            }
        }
        else if (pc.GetSubseq() == 2)
        {
            Debug.Log("���� ��ũ��Ʈ Ű: " + script.ScriptKey);
            float waitTime = targetTime - UnityEngine.Time.time;
            Debug.Log("��ٷ��� �ϴ� �ð�: " + waitTime);

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
                Debug.Log("���� 2 ����");
                dot.TriggerSub(true);
                pc.ProgressSubDial(script.ScriptKey);
            }
        }
        else
        {
            Debug.Log("���� ��ũ��Ʈ �ð�: " + script.Delay);

            float elapsed = 0f;
            while (elapsed < script.Delay)
            {
                if (isSkipping) yield break;
                yield return null;
                elapsed += UnityEngine.Time.deltaTime;
            }

            if (!isSkipping)
            {
                Debug.Log("���� ��ũ��Ʈ Ű: " + script.ScriptKey);
                dot.TriggerSub(true);
                pc.ProgressSubDial(script.ScriptKey);
            }
        }
        //pc.MarkSubWatched(pc.GetSubseq()); // �� �� ���
    }
    public void PlayAllSubDialogs()
    {
        List<int> phaseSubs = phaseToSubseqs.ContainsKey(Pattern) ? phaseToSubseqs[Pattern] : null;
        if (phaseSubs != null && phaseSubs.Count > 0)
        {
            int currentIdx = phaseSubs.IndexOf(pc.GetSubseq());
            if (currentIdx + 1 < phaseSubs.Count)
            {
                pc.SetSubseq(phaseSubs[currentIdx + 1]); // ���� ����
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
        subDialoguePanel.GetComponent<SubDialogue>().SubContinue();
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