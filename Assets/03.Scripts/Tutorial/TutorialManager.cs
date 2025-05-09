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
    End, // �� �ܰ�� �Ѿ�� ����, ���� �ܰ� 0���� �̵��ؾ� ��.
};

public class TutorialManager : GameManager
{
    private Dictionary<TutorialState, GameState> states;
    private Dictionary<TutorialState, DotState> dots;
    private TutorialState tutostate;

    public TutorialState TutoPattern => tutostate;
    private static TutorialManager instance;

    public static TutorialManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TutorialManager>();
                if (instance == null)
                {
                    Debug.LogError("TutorialManager �ν��Ͻ��� ã�� �� �����ϴ�.");
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
        Debug.Log("��ġ Ȯ��: " + Application.persistentDataPath);
        states = new Dictionary<TutorialState, GameState>();
        dots = new Dictionary<TutorialState, DotState>();
        states[TutorialState.Sub] = new Tutorial.Sub();
        states[TutorialState.Main] = new Tutorial.Main();

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }

        pc = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        if (pc == null) Debug.LogError("PlayerController�� ã�� �� �����ϴ�!");

        objectManager = GameObject.FindWithTag("ObjectManager")?.GetComponent<ObjectManager>();
        if (objectManager == null) Debug.LogError("ObjectManager�� ã�� �� �����ϴ�!");

        scrollManager = Camera.main?.GetComponent<ScrollManager>();
        if (scrollManager == null) Debug.LogError("ScrollManager�� ã�� �� �����ϴ�!");

        cameraZoom = Camera.main?.GetComponent<CameraZoom>();
        if (cameraZoom == null) Debug.LogError("CameraZoom�� ã�� �� �����ϴ�!");

        dot = GameObject.FindWithTag("DotController").GetComponent<DotController>();
        if (dot == null) Debug.LogError("DotController�� ã�� �� �����ϴ�!");
    }

    private void Start()
    {
        mainDialoguePanel?.GetComponent<MainPanel>()?.InitializePanels();
        InitGame();
    }

    public void ChangeGameState(TutorialState patternState)
    {
        if (states == null)
        {
            Debug.LogError("states�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        if (!states.ContainsKey(patternState))
        {
            Debug.LogError($"'{patternState}'�� ��ȿ���� ���� �����Դϴ�.");
            return;
        }

        if (activeState != null)
        {
            activeState.Exit(this, this); // 'this'�� �ùٸ� TutorialManager�� �����ؾ� ��.
        }

        tutostate = patternState;
        activeState = states[patternState];
        activeState.Enter(this, dot, this);
    }

    private void InitGame()
    {
        int hh = DateTime.Now.Hour; // ���� �ð� ��������

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
        float backgroundLoadWeight = 0.5f; // ��� �ε� ����
        float objectLoadWeight = 0.5f;     // ������Ʈ �ε� ����

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
            Debug.LogError("��� �ε� ����!");
        }

        yield return StartCoroutine(TrackObjectLoadProgress(objectLoadWeight));

        loadingProgressBar.value = 1;

        TutorialState patternState = (TutorialState)pc.GetAlreadyEndedPhase();
        tutostate = patternState;
        activeState = states[patternState];
        activeState.Enter(this, dot, this);
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
