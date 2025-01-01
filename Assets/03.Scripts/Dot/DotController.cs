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
    private DotState currentState; //���� ����
    private Dictionary<DotPatternState, DotState> states;
    private float position;
    private string dotExpression; //CSV�� ���ؼ� string ����
    private string animKey; //CSV�� ���ؼ� string���� ���� �Ľ� ������Ѵ�.

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

    public bool tutorial = true; //DoorController�� ����
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
        states.Add(DotPatternState.Tirgger, new Trigger());
        states.Add(DotPatternState.Tutorial, new DotTutorial());

        ScriptListParser scriptListParser = new ScriptListParser();
        mainScriptLists = new List<List<ScriptList>>();
        subScriptLists = new List<Dictionary<GamePatternState, List<ScriptList>>>();

        scriptListParser.Load(mainScriptLists, subScriptLists);

        subDialogue = GameObject.Find("SubDialougue");
        subPanel = GameObject.Find("SubPanel");
        subPanel.GetComponent<SubPanel>().InitializePanels();
        subDialogue.SetActive(false);
    }

    
    void Start()
    {
        chapter = manager.Chapter;

        animator.keepAnimatorStateOnDisable = true; //�ִϸ��̼� ����
    }

    public ScriptList GetMainScriptList(int index)
    {
        return mainScriptLists[chapter - 1][index];
    }

    public int GetSubScriptListCount(GamePatternState State) 
    {
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

    private void OnMouseDown()
    {
        if (mainAlert.activeSelf)
        {
            mainAlert.SetActive(false);
            //main ���ȭ���� Ʈ�����Ѵ�.
            manager.StartMain();
        }

        if(playAlert.activeSelf)
        {
            playAlert.SetActive(false);
            //���� å�� ������? ��� ���� �߰� ���д´ٰ��ϸ� ������ sleep����
            for (int i = 0; i < play.Length; i++)
            {
                play[i].SetActive(true);
            }
        }

        if (subAlert.activeSelf)
        {
            subDialogue.SetActive(true);
            string fileName = "sub_ch" + Chapter;
            subDialogue.GetComponent<SubDialogue>().StartSub(fileName);
            //int phase, string subTitle
            ScriptList tmp = GetSubScriptList(tmpState);
            //pc.successSubDialDelegate((int)tmpState,tmp.ScriptKey);
            subScriptLists[chapter - 1][tmpState].RemoveAt(0);
            TriggerSub(false);
            //sub trigger ���� 
            //������ ���� ������ ���� �� ���⿡�� ���� �߰�
            //�Ϸ�� ������ ��ȭ�� ������ �� ���� �߰�

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
        /*���⼭ OnClick �Լ��� �������ش�.*/
        //OutPos �� �ִٸ� �ش� Position���� �ٲ�����.
    }
    public void TriggerPlay(bool isActive)
    {
        alertOff();
        playAlert.SetActive(isActive);
        /*���⼭ OnClick �Լ��� �������ش�.*/
        //OutPos �� �ִٸ� �ش� Position���� �ٲ�����.
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
            currentState.Exit(this); //���� ���� �����ָ鼭, ���� �ʱ�ȭ ��Ų��.
        }

        /*Main���� �Ѿ�� ���� anim_default�� ���.*/

        animator.SetInteger("DotState", (int)state); //���� ���¸� �������ش�.
        position = OutPos; //���� ��ġ�� �ʱ�ȭ��, �׷��� �ϸ� ��� ���·� �������� �� -1�� �ƴϿ��� �������� �����ʴ´�.

        dotExpression = OutExpression; //Update, Main������ ����ϱ� ������ �ٸ� �������� ������� ����.
        animKey = OutAnimKey;
        chapter = manager.Chapter;
        //OutPos �� �ִٸ� �ش� Position���� �ٲ�����.
        currentState = states[state];
        currentState.Enter(this); //����
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
