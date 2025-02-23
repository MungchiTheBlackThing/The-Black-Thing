using Assets.Script.TimeEnum;
using System.Collections;
using System.Collections.Generic;
using Assets.Script.DialClass;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using static Unity.Burst.Intrinsics.X86.Avx;


public class DotController : MonoBehaviour
{
    [SerializeField]
    private DotState currentState;//현재 상태

    [SerializeField]
    private Dictionary<DotPatternState, DotState> states;

    [SerializeField]
    private float position;

    [SerializeField]
    private string dotExpression; //CSV에 의해서 string 들어옴

    [SerializeField]
    private string animKey; //CSV에 의해서 string으로 들어옴 파싱 해줘야한다.

    [SerializeField]
    GameObject mainAlert;
    [SerializeField]
    GameObject playAlert;
    [SerializeField]
    GameObject subAlert;

    [SerializeField]
    GameObject[] play;

    [SerializeField]
    private int chapter;

    [SerializeField]
    private GameManager manager;

    [SerializeField]
    private PlayerController pc;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private GameObject eyes;

    [SerializeField]
    private Animator eyesAnim;

    [SerializeField]
    GameObject dust;

    [SerializeField]
    ScriptListParser parser;

    [SerializeField]
    List<List<ScriptList>> mainScriptLists;

    [SerializeField]
    List<Dictionary<GamePatternState,List<ScriptList>>> subScriptLists; //List chapter Dictionary<gamestate,List<ScriptList>>> 
    [SerializeField]
    public GameObject subDialogue;
    [SerializeField]
    public GameObject subPanel;

    public bool tutorial = false; //DoorController에 쓰임
    public GameObject Dust
    {
        get { return dust; }
    }
    public GameObject Eyes
    {
        get { return eyes; }
    }

    public Animator Animator
    { get { return animator; } }
    
    public Animator EyesAnim
    { get { return eyesAnim; } }

    public int Chapter
    {
        get { return chapter; }
        set { chapter = value; }
    }

    public float Position
    {
        get { return position; }
        set { position = value; }
    }

    public string AnimKey
    {
        get { return animKey; }
        set { animKey = value; }
    }

    public string DotExpression
    {
        get { return dotExpression; }
        set { dotExpression = value; }
    }

    GamePatternState tmpState;

    void Awake()
    {

        animator = GetComponent<Animator>();
        Position = -1;
        dotExpression = "";

        states = new Dictionary<DotPatternState, DotState>();
        states.Clear();
        states.Add(DotPatternState.Default, new Idle());
        states.Add(DotPatternState.Phase, new Phase());
        states.Add(DotPatternState.Main, new Main());
        states.Add(DotPatternState.Sub, new Sub());
        states.Add(DotPatternState.Trigger, new Trigger());
        states.Add(DotPatternState.Tutorial, new DotTutorial());

        parser = new ScriptListParser();
        mainScriptLists = new List<List<ScriptList>>();
        subScriptLists = new List<Dictionary<GamePatternState, List<ScriptList>>>();

        parser.Load(mainScriptLists, subScriptLists);

        subDialogue = GameObject.Find("SubDialougue");
        subPanel = GameObject.Find("SubPanel");
        subPanel.GetComponent<SubPanel>().InitializePanels();
        subDialogue.SetActive(false);
    }

    
    void Start()
    {
        chapter = manager.Chapter;

        animator.keepAnimatorStateOnDisable = true; //애니메이션 유지
    }

    public ScriptList GetMainScriptList(int index)
    {
        Debug.Log("GetmainScript");
        return mainScriptLists[chapter - 1][index];
    }

    public int GetSubScriptListCount(GamePatternState State) 
    {
        Debug.Log("스테이트:" + State);
        Debug.Log("GetSubSCript");
        if (manager.Pattern == GamePatternState.MainA || manager.Pattern == GamePatternState.MainB || manager.Pattern == GamePatternState.Play || manager.Pattern == GamePatternState.Sleeping )
        {
            return 0;
        }
        else
            return subScriptLists[chapter - 1][State].Count;
    }
    public ScriptList GetSubScriptList(GamePatternState State)
    {
        if (subScriptLists[chapter - 1][State].Count == 0)
            return null;
        
        ScriptList tmp = subScriptLists[chapter - 1][State][0];
        tmpState = State;
        return tmp;
    }

    public void EndSubScriptList(GamePatternState State)
    {
        //다음 챕터?가 없을 때에는 아무 행위를 하지않는다.
        if (subScriptLists[chapter - 1][State].Count == 0)
            return;

        //서브 하나가 끝났을 때 0번째 서브를 뒤에 있는 서브들로 덮어쓰기해서
        //다음 서브로 넘어갈 수 있도록 한다.
        for (int i = 1; i < subScriptLists[chapter - 1][State].Count; i++)
        {
            subScriptLists[chapter - 1][State][i - 1] = subScriptLists[chapter - 1][State][i];
            Debug.Log("다음 서브스크립트 키: " + subScriptLists[chapter - 1][State][i].ScriptKey);
        }

        int endIdx = subScriptLists[chapter - 1][State].Count - 1;

        //마지막 번호 삭제 (중복)
        subScriptLists[chapter - 1][State].RemoveAt(endIdx);

        //다음 서브를 트리거할 수 있도록 한다.
    }

    private void OnMouseDown()
    {
        if (mainAlert.activeSelf)
        {
            mainAlert.SetActive(false);
            //main 배경화면을 트리거한다.
            manager.StartMain();
        }

        if(playAlert.activeSelf)
        {
            playAlert.SetActive(false);
            //같이 책을 읽을래? 라는 문구 뜨고 안읽는다고하면 총총총 sleep으로
            for (int i = 0; i < play.Length; i++)
            {
                play[i].SetActive(true);
            }
        }

        if (subAlert.activeSelf)
        {
            subDialogue.SetActive(true);
            string fileName = "sub_ch" + Chapter;
            Debug.Log(fileName);
            subDialogue.GetComponent<SubDialogue>().StartSub(fileName);
            //int phase, string subTitle
            ScriptList tmp = GetSubScriptList(tmpState);
            //pc.successSubDialDelegate((int)tmpState,tmp.ScriptKey);
            subScriptLists[chapter - 1][tmpState].RemoveAt(0);
            TriggerSub(false);
        }
    }

    public void TriggerSub(bool isActive)
    {
        alertOff();
        subAlert.SetActive(isActive);
    }

    public void TriggerMain(bool isActive)
    {
        alertOff();
        mainAlert.SetActive(isActive);
        /*여기서 OnClick 함수도 연결해준다.*/
        //OutPos 가 있다면 해당 Position으로 바껴야함.
    }
    public void TriggerPlay(bool isActive)
    {
        alertOff();
        playAlert.SetActive(isActive);
        /*여기서 OnClick 함수도 연결해준다.*/
        //OutPos 가 있다면 해당 Position으로 바껴야함.
    }

    public void alertOff()
    {
        subAlert.SetActive(false);
        mainAlert.SetActive(false);
        playAlert.SetActive(false);
    }

    public void GoSleep()
    {
        Trigger phase= (Trigger)currentState;

        if(phase!=null)
        {
            phase.GoSleep(this);
        }
    }

    public void EndPlay()
    {
        manager.NextPhase();
    }

    public void ChangeState(DotPatternState state = DotPatternState.Default, string OutAnimKey = "", float OutPos = -1, string OutExpression = "")
    {
        if (states == null) return;


        if (states.ContainsKey(state) == false)
        {
            return;
        }


        if (currentState != null)
        {
            currentState.Exit(this); //이전 값을 나가주면서, 값을 초기화 시킨다.
        }

        /*Main으로 넘어가기 전에 anim_default가 뜬다.*/

        animator.SetInteger("DotState", (int)state); //현재 상태를 변경해준다.
        position = OutPos; //이전 위치를 초기화함, 그렇게 하면 모든 상태로 입장했을 때 -1이 아니여서 랜덤으로 뽑지않는다.

        dotExpression = OutExpression; //Update, Main에서만 사용하기 때문에 다른 곳에서는 사용하지 않음.
        animKey = OutAnimKey;
        chapter = manager.Chapter;
        //OutPos 가 있다면 해당 Position으로 바껴야함.
        currentState = states[state];

        Debug.Log("Think SubDialogue " + state + " " + Position.ToString());

        currentState.Enter(this); //실행
    }

    public void Invisible()
    {
        SpriteRenderer dotRenderer = this.GetComponent<SpriteRenderer>();
        Color color = dotRenderer.color;
        color.a = 0f;
        dotRenderer.color = color;
        this.GetComponent<BoxCollider2D>().enabled = false;
        this.GetComponent<BoxCollider2D>().enabled = true;
        this.GetComponent<BoxCollider2D>().enabled = false;
    }
    public void Visible()
    {
        SpriteRenderer dotRenderer = this.GetComponent<SpriteRenderer>();
        Color color = dotRenderer.color;
        color.a = 255f;
        dotRenderer.color = color;
        this.GetComponent<BoxCollider2D>().enabled = true;
    }
}
