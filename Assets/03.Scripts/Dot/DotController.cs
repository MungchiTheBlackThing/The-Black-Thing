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
    private DotState currentState;//���� ����

    [SerializeField]
    private Dictionary<DotPatternState, DotState> states;

    [SerializeField]
    private float position;

    [SerializeField]
    private string dotExpression; //CSV�� ���ؼ� string ����

    [SerializeField]
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
    public List<List<ScriptList>> mainScriptLists;

    [SerializeField]
    public List<Dictionary<GamePatternState, List<ScriptList>>> subScriptLists; //List chapter Dictionary<gamestate,List<ScriptList>>> 

    [SerializeField]
    public GameObject subDialogue;
    [SerializeField]
    public GameObject subPanel;

    public bool tutorial = false; //DoorController�� ����
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
        Debug.Log("���� ����:" + mainScriptLists.Count);
        subDialogue = GameObject.Find("SubDialougue");
        subPanel = GameObject.Find("SubPanel");
        subPanel.GetComponent<SubPanel>().InitializePanels();
    }


    void Start()
    {
        chapter = manager.Chapter;
        Debug.Log("���� é��: " + chapter);
        animator.keepAnimatorStateOnDisable = true; //�ִϸ��̼� ����
    }

    public ScriptList GetMainScriptList(int index)
    {
        Debug.Log("GetmainScript");
        Debug.Log(index);
        Debug.Log(mainScriptLists[chapter - 1].Count);
        return mainScriptLists[chapter - 1][index];
    }

    public int GetSubScriptListCount(GamePatternState State)
    {
        
        Debug.Log("������Ʈ:" + State);
        Debug.Log("GetSubSCript");
        if (manager.Pattern == GamePatternState.MainA || manager.Pattern == GamePatternState.MainB || manager.Pattern == GamePatternState.Play || manager.Pattern == GamePatternState.Sleeping || manager.Pattern == GamePatternState.NextChapter)
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

    public ScriptList GetnxSubScriptList(GamePatternState State)
    {
        if (subScriptLists[chapter - 1][State].Count == 0 || subScriptLists[chapter - 1][State].Count == 1)
            return null;

        ScriptList tmp = subScriptLists[chapter - 1][State][1];
        tmpState = State;
        return tmp;
    }

    public void WaitEyesLoading()
    {
        DotEyes eyes;
        Eyes.SetActive(true);

        if (Enum.TryParse(DotExpression, true, out eyes))
        {
            EyesAnim.SetInteger("FaceKey", (int)eyes);
        }

        //StartCoroutine(ShowEyes());
    }

    private IEnumerator ShowEyes()
    {
        yield return new WaitForSeconds(2f);

        DotEyes eyes;
        Eyes.SetActive(true);

        if (Enum.TryParse(DotExpression, true, out eyes))
        {
            EyesAnim.SetInteger("FaceKey", (int)eyes);
        }
    }
    public void EndSubScriptList(GamePatternState State)
    {
        //���� é��?�� ���� ������ �ƹ� ������ �����ʴ´�.
        if (subScriptLists[chapter - 1][State].Count == 0 || State == GamePatternState.MainB || State == GamePatternState.MainA || State == GamePatternState.Play)
            return;

        //���� �ϳ��� ������ �� 0��° ���긦 �ڿ� �ִ� ������ ������ؼ�
        //���� ����� �Ѿ �� �ֵ��� �Ѵ�.
        for (int i = 1; i < subScriptLists[chapter - 1][State].Count; i++)
        {
            subScriptLists[chapter - 1][State][i - 1] = subScriptLists[chapter - 1][State][i];
            Debug.Log("���� ���꽺ũ��Ʈ Ű: " + subScriptLists[chapter - 1][State][i].ScriptKey);
        }

        int endIdx = subScriptLists[chapter - 1][State].Count - 1;

        //������ ��ȣ ���� (�ߺ�)
        subScriptLists[chapter - 1][State].RemoveAt(endIdx);

        //���� ���긦 Ʈ������ �� �ֵ��� �Ѵ�.
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
            Debug.Log(fileName);
            subDialogue.GetComponent<SubDialogue>().StartSub(fileName);
            //int phase, string subTitle
            ScriptList tmp = GetSubScriptList(tmpState);
            //pc.successSubDialDelegate((int)tmpState,tmp.ScriptKey);
            
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
        /*���⼭ OnClick �Լ��� �������ش�.*/
        //OutPos �� �ִٸ� �ش� Position���� �ٲ�����.
    }
    public void TriggerPlay(bool isActive)
    {
        Debug.Log(currentState);
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
        ChangeState(DotPatternState.Trigger);
        Trigger phase= (Trigger)currentState;

        if(phase!=null)
        {
            phase.GoSleep(this);
        }
    }

    public void EndPlay()
    {
        Debug.Log("�ڷ� �� �ð�");
        if (pc.GetChapter() == 1)
        {
            //this.position = 10;
            //this.transform.position = new Vector2(10.92f, -5.13f);
            return;
        }
        manager.NextPhase();
    }

    public void ChangeState(DotPatternState state = DotPatternState.Default, string OutAnimKey = "", float OutPos = -1, string OutExpression = "")
    {
        if (states == null) return;
        if (OutAnimKey == "") return;

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

        Debug.Log("Think SubDialogue " + state + " " + Position.ToString());

        currentState.Enter(this); //����
    }

    public void Invisible()
    {
        Debug.Log("�Ⱥ������ϴµ�.");
        SpriteRenderer dotRenderer = this.GetComponent<SpriteRenderer>();
        dotRenderer.sortingLayerName = "Default";
        this.GetComponent<BoxCollider2D>().enabled = false;
        this.GetComponent<BoxCollider2D>().enabled = true;
        this.GetComponent<BoxCollider2D>().enabled = false;
    }
    public void Visible()
    {
        Debug.Log("������ �ϴµ�.");
        SpriteRenderer dotRenderer = this.GetComponent<SpriteRenderer>();
        dotRenderer.sortingLayerName = "Dot";
        this.GetComponent<BoxCollider2D>().enabled = true;
    }
}
