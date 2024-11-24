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
    End,//이 단계로 넘어가면 오류, 다음단계 0으로 이동해야함.
};


public class TutorialManager : GameManager
{
    private Dictionary<TutorialState, GameState> states;
    private Dictionary<TutorialState, DotState> dots;
    protected TutorialState Tutostate;

    public TutorialState TutoPattern
    {
        get { return Tutostate; }
    }


    TutorialManager()
    {
       
    }

    private void Awake()
    {
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
            Debug.Log("없는 패턴 입니다.");
            return;
        }
        if (activeState != null)
        {
            activeState.Exit(this); //미리 정리한다.
        }
        Tutostate = patternState;
        activeState = states[patternState];
        activeState.Enter(this, dot);
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

        TutorialState patternState = (TutorialState)pc.GetAlreadyEndedPhase();
        Tutostate = patternState;
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
}
