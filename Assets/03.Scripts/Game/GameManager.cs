using Assets.Script.TimeEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Android;
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
    private GameState activeState;
    private ObjectManager objectManager;
    private ScrollManager scrollManager;
    private Dictionary<GamePatternState, GameState> states;
    private PlayerController pc;

    private SITime time;

    [SerializeField]
    GameObject skipPhase;

    [SerializeField]
    private DotController dot;
    public int Chapter
    {
        get { return pc.GetChapter(); }
    }

    public ObjectManager ObjectManager
    {
        get { return objectManager; }
    }

    public ScrollManager ScrollManager
    {
        get { return scrollManager; }
    }

    GameManager()
    {
        states = new Dictionary<GamePatternState, GameState>();

        states[GamePatternState.Watching] = new Watching();
        states[GamePatternState.MainA] = new MainA();
        states[GamePatternState.Thinking] = new Thinking();
        states[GamePatternState.MainB] = new MainB();
        states[GamePatternState.Writing] = new Writing();
        states[GamePatternState.Play] = new Play();
        states[GamePatternState.Sleeping] = new Sleeping();
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
        //Player �ܰ踦 �����´�.
        pc = GameObject.FindWithTag("Player").gameObject.GetComponent<PlayerController>();
        pc.nextPhaseDelegate += ChangeGameState;
        objectManager = GameObject.FindWithTag("ObjectManager").gameObject.GetComponent<ObjectManager>();
        scrollManager = GameObject.FindWithTag("MainCamera").gameObject.GetComponent<ScrollManager>();
        InitGame();
    }

    public void ChangeGameState(GamePatternState patternState)
    {
        if (states == null) return;

        if(states.ContainsKey(patternState) == false)
        {
            Debug.Log("���� ���� �Դϴ�.");
            return; 
        }

        StartCoroutine(ChangeState(patternState));
    }

    public void StartMain()
    {
        MainDialogue mainState= (MainDialogue)activeState;

        if(mainState != null)
        {
            mainState.StartMain(this);
        }
    }
    //�ڷ�ƾ���� �Ѵ�.
    IEnumerator ChangeState(GamePatternState patternState)
    {
        skipPhase.SetActive(true);
        yield return new WaitForSeconds(5.0f);

        skipPhase.SetActive(false);
        if (activeState != null)
        {
            activeState.Exit(this); //�̸� �����Ѵ�.
        }
        activeState = states[patternState];
        activeState.Enter(this, dot);

        yield return null;
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


        //�ӽÿ�
        time = SITime.Morning;
        //�ش� ��׶���� �����Ѵ�.
        GameObject background = Resources.Load<GameObject>("Background/"+time.ToString());
        Instantiate<GameObject>(background, objectManager.transform);
        //���ҽ� ������ �ִ� ��� ������Ʈ�� �����ͼ� Ǯ�� ��� ä���.
        objectManager.LoadObject(time.ToString(), pc.GetChapter());
        objectManager.SettingChapter(pc.GetChapter());
        foreach (var state in states)
        {
            state.Value.Init();
        }
        string path = Path.Combine(Application.dataPath + "/AssetBundles/" + time.ToString());

        objectManager.InitMainBackground(path);

        GamePatternState patternState = (GamePatternState)pc.GetAlreadyEndedPhase();
        activeState = states[patternState];
        activeState.Enter(this,dot);
    }
}
