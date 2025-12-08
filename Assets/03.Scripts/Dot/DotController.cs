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
    public List<List<ScriptList>> mainScriptLists;

    [SerializeField]
    public List<Dictionary<GamePatternState, List<ScriptList>>> subScriptLists; //List chapter Dictionary<gamestate,List<ScriptList>>> 

    [SerializeField]
    public GameObject subDialogue;
    [SerializeField]
    public GameObject subPanel;
    [SerializeField]
    PlayerController playerController;

    [SerializeField]
    private bool visible = true;
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

    Dictionary<float, Vector2> DotPositionDic = new Dictionary<float, Vector2>();
    Dictionary<DotPatternState, Dictionary<string, List<float>>> DotPositionKeyDic = new Dictionary<DotPatternState, Dictionary<string, List<float>>>();

    [SerializeField]
    SpriteRenderer spriteRenderer;

    [SerializeField]
    BoxCollider2D boxcollider;

    GamePatternState tmpState;

    private Vector2 spriteSize;
    void Awake()
    {
        animator = GetComponent<Animator>();
        Position = -1;
        dotExpression = "";
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();

        parser = new ScriptListParser();
        mainScriptLists = new List<List<ScriptList>>();
        subScriptLists = new List<Dictionary<GamePatternState, List<ScriptList>>>();

        parser.Load(mainScriptLists, subScriptLists);
        Debug.Log("메인 길이:" + mainScriptLists.Count);
        subPanel.GetComponent<SubPanel>().InitializePanels();

        spriteRenderer = this.GetComponent<SpriteRenderer>();
        boxcollider = this.GetComponent<BoxCollider2D>();
        //애니메이션과 위치 관련 초기화
        TextAsset jsonFile = Resources.Load<TextAsset>("FSM/DotPosition");
        Coordinate dotData = JsonUtility.FromJson<Coordinate>(jsonFile.text);
        foreach (var Data in dotData.data)
        {
            Vector2 vector = new Vector2(Data.X, Data.Y);
            DotPositionDic.Add(Data.dotPosition, vector);
        }

        string getFileName(DotPatternState state)
        {
            return state switch
            {
                DotPatternState.Default => "IdleState",
                DotPatternState.Sub => "PhaseState",
                DotPatternState.Phase => "PhaseState",
                DotPatternState.Trigger => "",
                DotPatternState.Main => "MainState",
                DotPatternState.Tutorial => "",
                _ => "",
            };
        }

        for (DotPatternState state = DotPatternState.Default; state <= DotPatternState.Tutorial; state++)
        {
            TextAsset idlePosTextAsset = Resources.Load<TextAsset>("FSM/" + getFileName(state));
            if (idlePosTextAsset == null) continue;

            DotPositionKeyDic.Add(state, new Dictionary<string, List<float>>());

            AnimationData animationData = JsonUtility.FromJson<AnimationData>(idlePosTextAsset.text);
            foreach (var anim in animationData.animations)
            {
                DotPositionKeyDic[state].Add(anim.key, anim.value.positions);
            }
        }

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        // 콜라이더 크기 조정
        boxcollider.size = spriteSize;
        boxcollider.offset = spriteRenderer.sprite.bounds.center;
    }


    void Start()
    {
        StartCoroutine(InitStart());
        spriteSize = spriteRenderer.sprite.bounds.size;

        // 콜라이더 크기 조정
        boxcollider.size = spriteSize;
        boxcollider.offset = spriteRenderer.sprite.bounds.center;
    }

    private IEnumerator InitStart()
    {
        yield return new WaitForSeconds(0.5f);
        if (manager == null)
        {
            manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
            if (manager == null) Debug.LogError("GameManager를 찾을 수 없습니다!");
        }

        if (pc == null)
        {
            pc = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
            if (pc == null) Debug.LogError("PlayerController를 찾을 수 없습니다!");
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) Debug.LogError("Animator를 찾을 수 없습니다!");
        }
        DotControllerStart();
    }

    public void DotControllerStart()
    {
        chapter = pc.GetChapter();
        Debug.Log("현재 챕터: " + chapter);
        animator.keepAnimatorStateOnDisable = true; //애니메이션 유지
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        // 콜라이더 크기 조정
        boxcollider.size = spriteSize;
        boxcollider.offset = spriteRenderer.sprite.bounds.center;
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

        Debug.Log("스테이트:" + State);
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
        int subseq = playerController.GetSubseq();
        /* 원래 방식은 subseq랑 연동되지 않는 문제 발생*/
        if (subScriptLists[chapter - 1][State].Count == 0)
            return null;

       
        if (subseq == 2 && subScriptLists[chapter - 1][State].Count == 2) //처음 시작할때 저장되었을 경우 subseq 1을 보고 끄고 다시 켰을때
        {
            Debug.Log("subseq 1을 봤다");
            ScriptList tmp = subScriptLists[chapter - 1][State][1];
            tmpState = State;
            return tmp;
        }
        else
        {
            ScriptList tmp = subScriptLists[chapter - 1][State][0];
            tmpState = State;
            return tmp;
        }
        

       
        //List<ScriptList> scripts = subScriptLists[chapter - 1][State];

        //if (scripts == null || scripts.Count == 0)
        //{
        //    Debug.Log("return null" + (subseq - 1) + "scriptcount:" + scripts.Count);
        //    return null;
        //}

        //return scripts[subseq - 1];
    }

    public ScriptList GetnxSubScriptList(GamePatternState State)
    {
        if (subScriptLists[chapter - 1][State].Count == 0 || subScriptLists[chapter - 1][State].Count == 1)
            return null;

        ScriptList tmp = subScriptLists[chapter - 1][State][1];
        tmpState = State;
        return tmp;
    }

    public void PlayEyeAnimation()
    {
        DotEyes eyes;
        Eyes.SetActive(true);

        if (Enum.TryParse(DotExpression, true, out eyes))
        {
            EyesAnim.SetInteger("FaceKey", (int)eyes);
        }
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        // 콜라이더 크기 조정
        boxcollider.size = spriteSize;
        boxcollider.offset = spriteRenderer.sprite.bounds.center;
    }

    public void EndSubScriptList(GamePatternState State)
    {
        //다음 챕터?가 없을 때에는 아무 행위를 하지않는다.
        if (subScriptLists[chapter - 1][State].Count == 0 || State == GamePatternState.MainB || State == GamePatternState.MainA || State == GamePatternState.Play)
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

        if (playAlert.activeSelf)
        {
            Debug.Log("트리거 꺼짐");
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
        //잠자러 가는 애니메이션 실행.
        ChangeState(DotPatternState.Phase, "phase_sleep", 19);
        alertOff();
    }

    public void EndPlay()
    {
        Debug.Log("자러 갈 시간");
        if (pc.GetChapter() == 1)
        {
            ChangeState(DotPatternState.Phase, "anim_spiderweb1", 10);
            return;
        }
        alertOff();
        manager.NextPhase();
    }

    public void ChangeState(DotPatternState state = DotPatternState.Default, string OutAnimKey = "", float OutPos = -1, string OutExpression = "")
    {
        Debug.Log($"애니메이션 함수 호출: {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name}");
        Debug.Log("키: " + OutAnimKey + "표현: " + OutExpression + "위치: " + OutPos);
        position = OutPos;
        dotExpression = OutExpression;
        animKey = OutAnimKey;
        string prevAnimKey = animKey;
        chapter = manager.Chapter;

        //outPos -1일경우 랜덤위치
        if (position == -1)
        {
            if (DotPositionKeyDic.TryGetValue(state, out var dic))
            {
                if (dic.TryGetValue(OutAnimKey, out var list))
                {
                    int maxIdx = list.Count;
                    position = list[UnityEngine.Random.Range(0, maxIdx)];
                }
            }
        }

        //위치 조절
        if (DotPositionDic.ContainsKey(position))
        {
            transform.position = DotPositionDic[position];
        }

        if (!visible)
            return;


        if (OutAnimKey != "")
        {
            Debug.Log("플레이 되어야 하는 애니메이션 : " + OutAnimKey);
            animator.Play(OutAnimKey, 0, 0f);
            animator.Update(0f);
            spriteSize = spriteRenderer.sprite.bounds.size;
            boxcollider.size = spriteSize;
            boxcollider.offset = spriteRenderer.sprite.bounds.center;
            var split = OutAnimKey.Split("_");
            if (split[0] == "anim")
            {
                Eyes.gameObject.SetActive(false);
            }
        }

        //눈 작동
        if (state == DotPatternState.Main)
        {
            Debug.Log(dotExpression);
            PlayEyeAnimation();
        }

        // 스프라이트의 실제 픽셀 단위 크기 가져오기 (로컬 단위로 변환됨)
        spriteSize = spriteRenderer.sprite.bounds.size;
        // 콜라이더 크기 조정
        boxcollider.size = spriteSize;
        boxcollider.offset = spriteRenderer.sprite.bounds.center;


        if (Dust != null)
        {
            var spawner = Dust.GetComponent<DustSpawner>();
            if (spawner != null)
            {
                bool wasSleep = prevAnimKey == "anim_sleep";
                bool nowSleep = OutAnimKey == "anim_sleep";

                // anim_sleep → 다른 애니 : 스폰 정지
                if (wasSleep && !nowSleep)
                {
                    spawner.PauseSpawner();
                }
                // 다른 애니 → anim_sleep : 스폰 재개
                else if (!wasSleep && nowSleep)
                {
                    spawner.ResumeSpawner();
                }
            }
        }
    }

    public void Invisible()
    {
        Debug.Log("Dot invisible");
        visible = false;
        // 애니메이터 비활성화 (색상 덮어쓰기 방지)
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        SpriteRenderer dotRenderer = GetComponent<SpriteRenderer>();
        Color color = dotRenderer.color;
        color.a = 0f;
        dotRenderer.color = color;

        // 콜라이더 비활성화
        GetComponent<BoxCollider2D>().enabled = false;
    }

    public void Visible()
    {
        Debug.Log("Dot visible");
        visible = true;
        SpriteRenderer dotRenderer = GetComponent<SpriteRenderer>();
        Color color = dotRenderer.color;
        color.a = 1f;
        dotRenderer.color = color;

        // 콜라이더 다시 활성화
        GetComponent<BoxCollider2D>().enabled = true;

        // 애니메이터 다시 켜기
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
        }
    }

    public void Spriteoff()
    {
        Invisible();
        eyes.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void Spriteon()
    {
        Visible();
        eyes.GetComponent<SpriteRenderer>().enabled = true;
    }
    public void dotvicheck(bool set)
    {
        StartCoroutine(DotvisibleCheck(set));
    }
    public IEnumerator DotvisibleCheck(bool setoff)
    {
        yield return new WaitForSeconds(0.08f);
        if (setoff)
        {
            Invisible();
        }
        else
        {
            Visible();
        }
    }
}
