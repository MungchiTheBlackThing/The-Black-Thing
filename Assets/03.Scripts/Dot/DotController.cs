using Assets.Script.TimeEnum;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


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
    GameObject[] play;

    [SerializeField]
    private int chapter;

    [SerializeField]
    private GameManager manager;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private GameObject eyes;

    [SerializeField]
    private Animator eyesAnim;

    [SerializeField]
    GameObject dust;

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

    void Awake()
    {

        animator = GetComponent<Animator>();

        Position = -1;
        dotExpression = "";

        states = new Dictionary<DotPatternState, DotState>();
        states.Clear();
        states.Add(DotPatternState.Defualt, new Idle());
        states.Add(DotPatternState.Phase, new Phase());
        states.Add(DotPatternState.Main, new Main());
        states.Add(DotPatternState.Sub, new Sub());
        states.Add(DotPatternState.Tirgger, new Trigger());
    }
    void Start()
    {
        chapter = manager.Chapter;

        animator.keepAnimatorStateOnDisable = true; //�ִϸ��̼� ����
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
    }
    public void TriggerMain(bool isActive)
    {
        mainAlert.SetActive(isActive);
        /*���⼭ OnClick �Լ��� �������ش�.*/
        //OutPos �� �ִٸ� �ش� Position���� �ٲ�����.
    }
    public void TriggerPlay(bool isActive)
    {
        playAlert.SetActive(isActive);
        /*���⼭ OnClick �Լ��� �������ش�.*/
        //OutPos �� �ִٸ� �ش� Position���� �ٲ�����.
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

    public void ChangeState(DotPatternState state = DotPatternState.Defualt, string OutAnimKey = "", float OutPos = -1, string OutExpression = "")
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

}
