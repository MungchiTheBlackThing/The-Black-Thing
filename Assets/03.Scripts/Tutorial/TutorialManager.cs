using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script.TimeEnum;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Android;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum TutorialState
{
    Sub,
    Main,
    End, // 이 단계로 넘어가면 오류, 다음 단계 0으로 이동해야 함.
};

public class TutorialManager : GameManager
{
    private Dictionary<TutorialState, GameState> states;
    //private Dictionary<TutorialState, DotState> dots;
    private TutorialState tutostate;

    public TutorialState TutoPattern => tutostate;
    private static TutorialManager instance;

    private bool _ready = false;
    private TutorialState? _pendingState = null;

    public static TutorialManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TutorialManager>();
                if (instance == null)
                {
                    Debug.LogError("TutorialManager 인스턴스를 찾을 수 없습니다.");
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Debug.Log("위치 확인: " + Application.persistentDataPath);
        states = new Dictionary<TutorialState, GameState>();
        //dots = new Dictionary<TutorialState, DotState>();
        states[TutorialState.Sub] = new Tutorial.Sub();
        states[TutorialState.Main] = new Tutorial.Main();

        
    }

    private void Start()
    {
        mainDialoguePanel?.GetComponent<MainPanel>()?.InitializePanels();
        StartCoroutine(InitTutorial());
    }

    private IEnumerator InitTutorial()
    {
        yield return new WaitForSeconds(0.5f);

        pc = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        if (pc == null) Debug.LogError("PlayerController를 찾을 수 없습니다!");

        objectManager = GameObject.FindWithTag("ObjectManager")?.GetComponent<ObjectManager>();
        if (objectManager == null) Debug.LogError("ObjectManager를 찾을 수 없습니다!");

        scrollManager = Camera.main?.GetComponent<ScrollManager>();
        if (scrollManager == null) Debug.LogError("ScrollManager를 찾을 수 없습니다!");

        cameraZoom = Camera.main?.GetComponent<CameraZoom>();
        if (cameraZoom == null) Debug.LogError("CameraZoom를 찾을 수 없습니다!");

        dot = GameObject.FindWithTag("DotController")?.GetComponent<DotController>();
        if (dot == null) Debug.LogError("DotController를 찾을 수 없습니다!");

        timeSkipUIController = FindObjectOfType<TimeSkipUIController>(true);
        if (timeSkipUIController == null) Debug.LogError("TimeSkipUIController를 찾을 수 없습니다!");
        
        InitGame();
    }

    public void ChangeGameState(TutorialState patternState)
    {

        if (!_ready)
        {
            _pendingState = patternState;
            Debug.LogWarning($"[TutorialManager] queued state={patternState} (not ready yet)");
            return;
        }

        if (dot == null)
        {
            _pendingState = patternState;
            Debug.LogWarning($"[TutorialManager] queued state={patternState} (dot null)");
            return;
        }
        if (states == null)
        {
            Debug.LogError("states가 초기화되지 않았습니다.");
            return;
        }

        if (!states.ContainsKey(patternState))
        {
            Debug.LogError($"'{patternState}'는 유효하지 않은 상태입니다.");
            return;
        }

        if (activeState != null)
        {
            activeState.Exit(this, this); // 'this'는 올바른 TutorialManager를 전달해야 함.
        }

        tutostate = patternState;
        activeState = states[patternState];
        activeState.Enter(this, dot, this);

        // 튜토리얼 상태 변경 시 타이머 재설정 (GameManager의 PhaseTimer 활용)
        if (phaseTimerCoroutine != null) StopCoroutine(phaseTimerCoroutine);
        currentPattern = (GamePatternState)patternState; // PhaseTimer가 currentPattern을 참조하므로 동기화
        phaseTimerCoroutine = StartCoroutine(PhaseTimer());
        
    }

    private void InitGame()
    {
        //튜토리얼 시작 시(새 게임) Watching 단계의 타이머 기록 초기화
        string key = $"PhaseTimer_1_{GamePatternState.Watching}";
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        int hh = DateTime.Now.Hour; // 현재 시간 가져오기

        if (hh >= (int)STime.T_DAWN && hh < (int)STime.T_MORNING)
        {
            sltime = SITime.Dawn;
        }
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

    private IEnumerator LoadDataAsync()
    {
        float backgroundLoadWeight = 0.5f; // 배경 로드 비중
        float objectLoadWeight = 0.5f;     // 오브젝트 로드 비중

        loadingProgressBar.value = 0;

        ResourceRequest loadOperation = Resources.LoadAsync<GameObject>("Background/" + sltime);
        while (!loadOperation.isDone)
        {
            loadingProgressBar.value = loadOperation.progress * backgroundLoadWeight;
            yield return null;
        }

        if (loadOperation.asset != null)
        {
            Instantiate((GameObject)loadOperation.asset, objectManager.transform);
        }
        else
        {
            Debug.LogError("배경 로드 실패!");
        }

        yield return StartCoroutine(TrackObjectLoadProgress(objectLoadWeight));

        loadingProgressBar.value = 1;

        TutorialState patternState = (TutorialState)pc.GetCurrentPhase();
        tutostate = patternState;
        activeState = states[patternState];
        activeState.Enter(this, dot, this);

        // 초기 로딩 후 타이머 시작
        if (phaseTimerCoroutine != null) StopCoroutine(phaseTimerCoroutine);
        currentPattern = (GamePatternState)patternState;
        phaseTimerCoroutine = StartCoroutine(PhaseTimer());

        _ready = true;

        if (_pendingState.HasValue)
        {
            var st = _pendingState.Value;
            _pendingState = null;
            ChangeGameState(st);
        }

        AudioManager.Instance.UpdateBGMByChapter(pc.GetChapter(), currentPattern);
    }

    private IEnumerator TrackObjectLoadProgress(float weight)
    {
        float progress = 0f;

        Coroutine loadObjectCoroutine = StartCoroutine(objectManager.LoadObjectAsync(sltime.ToString(), pc.GetChapter()));
        while (!objectManager.IsLoadObjectComplete())
        {
            progress = objectManager.GetLoadProgress();
            loadingProgressBar.value = progress * weight;
            yield return null;
        }

        loadingProgressBar.value += weight;
    }
}
