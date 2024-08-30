using Assets.Script.TimeEnum;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class DotController : MonoBehaviour
{

    private DotState currentState; //���� ����
    private Dictionary<DotPatternState, DotState> states;

    private float position;
    private string dotExpression; //CSV�� ���ؼ� string ����
    private string animKey; //CSV�� ���ؼ� string���� ���� �Ľ� ������Ѵ�.

    [SerializeField] GameObject mainAlert;

    [SerializeField]
    private ChapterDay chapter;

    [SerializeField]
    private Animator animator;

    public Animator Animator
    { get { return animator; } }

    public ChapterDay Chapter
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

    void Start()
    {
        states = new Dictionary<DotPatternState, DotState>();
        states.Clear();
        //states.Add(DotPatternState.Defualt, new Idle());

        //animation phase -> main 
        states.Add(DotPatternState.Phase, new Phase());
        //states.Add(DotPatternState.Main, new Main());
        //states.Add(DotPatternState.Sub, new Sub());

        animator = GetComponent<Animator>();

        Position = -1;
        dotExpression = "";
        chapter = ChapterDay.C_1DAY;
        ChangeState(DotPatternState.Defualt, "anim_mud"); //ó�� default
    }

    private void OnEnable()
    {
        mainAlert = GameObject.Find("Dot").transform.Find("MainAlert").gameObject;
    }

    public void TriggerMain()
    {
        mainAlert.SetActive(true);
        /*���⼭ OnClick �Լ��� �������ش�.*/
    }
    public void ChangeState(DotPatternState state = DotPatternState.Defualt, string OutAnimKey = "", float OutPos = -1, string OutExpression = "")
    {
        if (states == null) return;

        if (states.ContainsKey(state) == null)
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

        //OutPos �� �ִٸ� �ش� Position���� �ٲ�����.
        currentState = states[state];
        currentState.Enter(this); //����
    }
}
