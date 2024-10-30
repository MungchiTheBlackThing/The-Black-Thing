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
using System.Runtime.Serialization;
public enum TutorialState
{
    Sub,
    Main,
    End,//�� �ܰ�� �Ѿ�� ����, �����ܰ� 0���� �̵��ؾ���.
};


public class TutorialManager : GameManager
{
    private Dictionary<TutorialState, GameState> states;
    private TutorialState Tutostate;

    public TutorialState TutoPattern
    {
        get { return Tutostate; }
    }


    TutorialManager()
    {
        states = new Dictionary<TutorialState, GameState>();
        states[TutorialState.Sub] = new Tutorial.Sub();
        states[TutorialState.Main] = new Tutorial.Main();
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
        //pc.nextPhaseDelegate += ChangeGameState;
        objectManager = GameObject.FindWithTag("ObjectManager").gameObject.GetComponent<ObjectManager>();
        scrollManager = GameObject.FindWithTag("MainCamera").gameObject.GetComponent<ScrollManager>();
    }
    void Start()
    {
        if (mainDialoguePanel)
        {
            mainDialoguePanel.GetComponent<MainPanel>().InitializePanels();
        }

        InitGame();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeGameState(TutorialState patternState)
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
        Tutostate = patternState;
        activeState = states[patternState];
        activeState.Enter(this, dot);
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

        TutorialState patternState = (TutorialState)pc.GetAlreadyEndedPhase();
        Tutostate = patternState;
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
}
