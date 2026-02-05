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
    public bool isEndPlay = false;
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

    public enum AlertType
    {
        Main,
        Sub,
        Play
    }

    Dictionary<float, Vector2> DotPositionDic = new Dictionary<float, Vector2>();
    Dictionary<DotPatternState, Dictionary<string, List<float>>> DotPositionKeyDic = new Dictionary<DotPatternState, Dictionary<string, List<float>>>();

    [SerializeField]
    SpriteRenderer spriteRenderer;

    [SerializeField]
    BoxCollider2D boxcollider;

    public static event Action DiaryGateChanged; // 뭉치 상태 변경 쏴 주기 (일기용)

    private bool isAfterScriptPlaying = false;
    private bool isSubDialogueAnimPlaying = false;
    
    // Diary 열람 제어
    public bool IsSubDialogueAnimPlaying
    {
        get { return isSubDialogueAnimPlaying; }
    }
    
    public bool IsAfterScriptPlaying
    {
        get { return isAfterScriptPlaying; }
    }
    private float _idleAnimationTimer;
    private const float IDLE_ANIMATION_DURATION = 180f; //(단위: 초) 랜덤 재생 애니메이션 재생 시간 제한 (3분) //[DEBUG] 3분 -> 10초

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

        InitializeAnimationPositions();

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

    void Update()
    {
        // 1. Alert가 활성화된 상태(이벤트 진입 대기)이면 타이머 동작 안함 (진입 애니메이션 유지)
        if (mainAlert.activeSelf || subAlert.activeSelf || playAlert.activeSelf)
        {
            _idleAnimationTimer = 0f;
            return;
        }

        // 2. 대화 중이거나 메인 스토리 진행 중이면 타이머 동작 안함. 
        // 단, AfterScript가 재생 중일 때는 예외적으로 타이머가 돌아야 함 (시간 경과 후 종료를 위해)
        bool isBlockedByDialogue = isSubDialogueAnimPlaying || manager.CurrentState is MainDialogue;
        
        if (subDialogue.activeSelf)
        {
            isBlockedByDialogue = true;
        }

        if (isBlockedByDialogue && !isAfterScriptPlaying)
        {
            _idleAnimationTimer = 0f;
            return;
        }

        // 3. 타이머 로직 실행 조건: AfterScript 재생 중이거나, Thinking 페이즈(랜덤 애니메이션)인 경우
        bool shouldRunTimer = isAfterScriptPlaying || (manager.Pattern == GamePatternState.Thinking) || (manager.Pattern == GamePatternState.Watching);

        if (shouldRunTimer)
        {
            _idleAnimationTimer += Time.deltaTime;
            if (_idleAnimationTimer >= IDLE_ANIMATION_DURATION)
            {
                Debug.Log("[DotController] IDLE_ANIMATION_DURATION 경과");
                
                if (isAfterScriptPlaying)
                {
                    // AfterScript 재생 중이었으면 종료하고 기본 상태로 복귀
                    StopAfterScript();
                }
                else
                {
                    // Thinking 페이즈라면 새로운 랜덤 애니메이션 재생
                    UpdateIdleAnimation();
                }
            }
        }
        else
        {
            _idleAnimationTimer = 0f;
        }
    }

    private IEnumerator InitStart()
    {
        yield return new WaitForSeconds(0.5f);
        if (manager == null)
        {
            manager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
            if (manager == null) Debug.LogError("GameManager를 찾을 수 없습니다");
        }

        if (pc == null)
        {
            pc = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
            if (pc == null) Debug.LogError("PlayerController를 찾을 수 없습니다");
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null) Debug.LogError("Animator를 찾을 수 없습니다");
        }
        DotControllerStart();
    }

    //백그라운드 전환시 애니메이션 변경
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) return; // 백그라운드 진입이면 무시

        // 서브/메인/알럿/서브애니 재생 중에는 절대 상태 갱신 금지
        if (subDialogue != null && subDialogue.activeSelf) return;
        if (subPanel != null && subPanel.activeSelf) return;
        if (subAlert != null && subAlert.activeSelf) return;
        if (mainAlert != null && mainAlert.activeSelf) return;
        if (playAlert != null && playAlert.activeSelf) return;
        if (isSubDialogueAnimPlaying) return;
        bool isMainPhase = manager.Pattern == GamePatternState.MainA || manager.Pattern == GamePatternState.MainB;
        if (isMainPhase) return;
        
        UpdateIdleAnimation();
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

        // AfterScript 상태 복원
        if (PlayerPrefs.GetInt("AS_IsPlaying", 0) == 1)
        {
            string savedAnim = PlayerPrefs.GetString("AS_AnimKey", "");
            float savedPos = PlayerPrefs.GetFloat("AS_Pos", -1f);
            if (!string.IsNullOrEmpty(savedAnim))
            {
                Debug.Log($"[DotController] Restoring AfterScript: {savedAnim}");
                PlayAfterScript(savedAnim, savedPos);
            }
        }
    }

    public ScriptList GetMainScriptList(int index)
    {
        if (manager != null)
        {
            chapter = manager.Chapter;
        }

        Debug.Log($"[DotController] GetMainScriptList called. Chapter: {chapter}, Index: {index}");

        if (mainScriptLists == null)
        {
            Debug.LogError("[DotController] mainScriptLists is null!");
            return null;
        }

        if (chapter <= 0 || chapter > mainScriptLists.Count)
        {
            Debug.LogError($"[DotController] Chapter {chapter} is out of range. mainScriptLists count: {mainScriptLists.Count}");
            return null;
        }

        var chapterList = mainScriptLists[chapter - 1];
        if (index < 0 || index >= chapterList.Count)
        {
            Debug.LogError($"[DotController] Index {index} is out of range for chapter {chapter}. List count: {chapterList.Count}");
            return null;
        }

        ScriptList script = chapterList[index];
        Debug.Log($"[DotController] Retrieved ScriptList - Anim: {script.DotAnim}, Pos: {script.DotPosition}");
        return script;
    }

    public int GetSubScriptListCount(GamePatternState State)
    {
        if (manager != null)
        {
            chapter = manager.Chapter;
        }
        int subseq = playerController.GetSubseq();
        if (State == GamePatternState.Thinking && chapter==1 && subseq == 1)
        {
            manager.Menu.moldOn();
        }
        Debug.Log("스테이트:" + State);
        Debug.Log("GetSubSCript");
        if (manager.Pattern == GamePatternState.MainA || manager.Pattern == GamePatternState.MainB || manager.Pattern == GamePatternState.Play || manager.Pattern == GamePatternState.NextChapter)
        {
            return 0;
        }
        else
            return subScriptLists[chapter - 1][State].Count;
    }
    public ScriptList GetSubScriptList(GamePatternState State)
    {
        if (manager != null)
        {
            chapter = manager.Chapter;
        }

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

    // [추가] 페이즈별 스크립트 가져오기 (MainA, MainB 등 subseq와 무관한 경우 처리)
    public ScriptList GetScriptForPhase(GamePatternState state)
    {
        if (manager != null) chapter = manager.Chapter;

        // MainA, MainB는 subseq와 상관없이 첫 번째 스크립트를 가져옴
        // ScriptListParser에서 MainA, MainB는 mainScriptLists에 저장됨
        if (state == GamePatternState.MainA || state == GamePatternState.MainB)
        {
            if (mainScriptLists != null && chapter > 0 && chapter <= mainScriptLists.Count)
            {
                var list = mainScriptLists[chapter - 1];
                foreach (var s in list)
                {
                    if (s.GameState == state) return s;
                }
            }
            return null;
        }
        return GetSubScriptList(state);
    }

    public ScriptList GetnxSubScriptList(GamePatternState State)
    {
        if (manager != null)
        {
            chapter = manager.Chapter;
        }

        if (subScriptLists[chapter - 1][State].Count == 0 || subScriptLists[chapter - 1][State].Count == 1)
            return null;

        ScriptList tmp = subScriptLists[chapter - 1][State][1];
        tmpState = State;
        return tmp;
    }

    public void PlayEyeAnimation()
    {
        Eyes.SetActive(true);
        EyesAnim.Play(DotExpression);
        //if (Enum.TryParse(DotExpression, true, out eyes))
        //{
        //    EyesAnim.SetInteger("FaceKey", (int)eyes);
        //}
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        // 콜라이더 크기 조정
        boxcollider.size = spriteSize;
        boxcollider.offset = spriteRenderer.sprite.bounds.center;
    }

    public void EndSubScriptList(GamePatternState State)
    {
        if (manager != null)
        {
            chapter = manager.Chapter;
        }

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
        if (InputGuard.BlockWorldInput()) return;
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
            manager.ScrollManager.MoveCamera(new Vector3(manager.ScrollManager.camLimitValue.y, 0, -10f), 2f);
            manager.ScrollManager.stopscroll();
            //같이 책을 읽을래? 라는 문구 뜨고 안읽는다고하면 총총총 sleep으로
            StartCoroutine(playOnafterdelay());
           
        }

        if (subAlert.activeSelf)
        {
            subDialogue.SetActive(true);
            RefreshDustState(this.animKey); 
            string fileName = "sub_ch" + Chapter;
            Debug.Log(fileName);
            subDialogue.GetComponent<SubDialogue>().StartSub(fileName);
            //int phase, string subTitle
            ScriptList tmp = GetSubScriptList(tmpState);
            //pc.successSubDialDelegate((int)tmpState,tmp.ScriptKey);


            TriggerSub(false);
        }
    }

    public IEnumerator playOnafterdelay()
    {
        yield return new WaitForSeconds(2f);
        for (int i = 0; i < play.Length; i++)
        {
            play[i].SetActive(true);
        }
    }

    public void OnAlertClicked(AlertType type)
    {
        switch(type)
        {
            case AlertType.Main:
                // 기존 OnMouseDown에서 mainAlert 눌렀을 때 하던 코드 그대로 붙여넣기
                mainAlert.SetActive(false);
                manager.StartMain();
                break;

            case AlertType.Play:
                playAlert.SetActive(false);
                for (int i = 0; i < play.Length; i++) play[i].SetActive(true);
                break;

            case AlertType.Sub:
                // 기존 subAlert 분기 코드 그대로
                subDialogue.SetActive(true);
                RefreshDustState(this.animKey); 
                string fileName = "sub_ch" + Chapter;
                subDialogue.GetComponent<SubDialogue>().StartSub(fileName);
            
                ScriptList tmp = GetSubScriptList(tmpState);
                TriggerSub(false);
                break;
        }
    }


    public void TriggerSub(bool isActive, string animKey = "", float position = -1)
    {
        alertOff();
        subAlert.SetActive(isActive);
        if (isActive) ForceStopAfterScript(); // 진입 시 AfterScript 강제 종료
        if (isActive && !string.IsNullOrEmpty(animKey))
        {
            ChangeState(DotPatternState.Default, animKey, position, "", true);
        }
        DiaryGateChanged?.Invoke();
    }

    public void TriggerMain(bool isActive)
    {
        alertOff();
        if (isActive) ForceStopAfterScript(); // 진입 시 AfterScript 강제 종료
        mainAlert.SetActive(isActive);
        /*여기서 OnClick 함수도 연결해준다.*/
        //OutPos 가 있다면 해당 Position으로 바껴야함.
        DiaryGateChanged?.Invoke();
    }
    public void TriggerPlay(bool isActive)
    {
        alertOff();
        if (isActive) ForceStopAfterScript(); // 진입 시 AfterScript 강제 종료
        playAlert.SetActive(isActive);
        /*여기서 OnClick 함수도 연결해준다.*/
        //OutPos 가 있다면 해당 Position으로 바껴야함.
        DiaryGateChanged?.Invoke();
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
            isEndPlay = true;
            return;
        }
        alertOff();
        manager.NextPhase();
    }

    private void _Internal_SetAnimation(DotPatternState state, string OutAnimKey, float OutPos, string OutExpression)
    {
        string prevAnim = animKey; // 필드 백업(바뀌기 전)

        Debug.Log($"애니메이션 함수 호출: {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name}");
        Debug.Log("키: " + OutAnimKey + " 표현: " + OutExpression + " 위치: " + OutPos);
        position = OutPos;
        bool bPlayEyeAnimation = dotExpression != OutExpression;
        dotExpression = OutExpression;
        _idleAnimationTimer = 0f; // 애니메이션이 변경될 때마다 타이머 초기화
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
                else
                {
                    Debug.LogWarning($"[DotController] '{OutAnimKey}'에 대한 위치 데이터가 {state} 상태에 정의되지 않았으므로 기본 위치 사용");
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
            Debug.Log("뭉치 애니메이션 : " + OutAnimKey);
            AnimKey = OutAnimKey;
            if (gameObject.activeInHierarchy)
            {
                animator.Play(OutAnimKey, 0, 0f);
                animator.Update(0f);
            }
            else
            {
                Debug.LogWarning($"[DotController] GameObject is inactive. Skipping animation play: {OutAnimKey}");
            }
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
        if (state == DotPatternState.Main && bPlayEyeAnimation)
        {
            Debug.Log("눈 애니메이션 : " + dotExpression);
            PlayEyeAnimation();
        }

        // 스프라이트의 실제 픽셀 단위 크기 가져오기 (로컬 단위로 변환됨)
        spriteSize = spriteRenderer.sprite.bounds.size;
        // 콜라이더 크기 조정
        boxcollider.size = spriteSize;
        boxcollider.offset = spriteRenderer.sprite.bounds.center;

            
        if (!string.IsNullOrEmpty(OutAnimKey))
        {
            AnimKey = OutAnimKey; // 여기서 animKey 필드가 바뀜
            // animator.Play...
        }

        if (prevAnim != AnimKey)
        {
            RefreshDustState(animKey);
            DiaryGateChanged?.Invoke();
        }
        
    }

    public void RefreshDustState(string currentAnimKey)
    {
        if (Dust == null) return;

        // SubDialogue 패널이 켜져 있으면 무조건 OFF
        bool blockBySubPanel = (subDialogue != null && subDialogue.activeSelf);

        bool shouldEnable = (currentAnimKey == "anim_sleep") && !blockBySubPanel;

        if (shouldEnable)
        {
            if (!Dust.activeSelf) Dust.SetActive(true);

            var spawner = Dust.GetComponent<DustSpawner>();
            if (spawner != null) spawner.ResumeSpawner();
        }
        else
        {
            // anim_sleep 아니면 DustSpawner 자체 OFF
            if (Dust.activeSelf) Dust.SetActive(false);
        }
    }


    public void ChangeState(DotPatternState state = DotPatternState.Default, string OutAnimKey = "", float OutPos = -1, string OutExpression = "", bool force = false)
    {
        //우선순위 체크
        if (!force)
        {
            // 1. 서브 다이얼로그 애니메이션
            if (isSubDialogueAnimPlaying && state != DotPatternState.Sub && state != DotPatternState.Main)
            {
                Debug.Log($"[DotController] ChangeState for '{OutAnimKey}' blocked by SubDialogue animation.");
                //return;
            }
            // 2. AfterScript
            if (isAfterScriptPlaying)
            {
                // 진입 애니메이션(Alert 활성화 상황) 또는 대화 진행 중(Panel 활성화)인 경우에 AfterScript보다 우선순위 높음
                bool isSubHighPriority = state == DotPatternState.Sub && (subAlert.activeSelf || subDialogue.activeSelf);
                bool isMainHighPriority = state == DotPatternState.Main && (mainAlert.activeSelf || (manager.mainDialoguePanel != null && manager.mainDialoguePanel.activeSelf));
                bool isPlayHighPriority = state == DotPatternState.Trigger && playAlert.activeSelf;
                bool isTutorial = state == DotPatternState.Tutorial || this.tutorial;

                bool isEntryAnimation = isSubHighPriority || isMainHighPriority || isPlayHighPriority || isTutorial;

                if (isEntryAnimation)
                {
                    Debug.Log($"[DotController] Stopping AfterScript because entry animation ({state}) requested.");
                    isAfterScriptPlaying = false;
                    PlayerPrefs.SetInt("AS_IsPlaying", 0);
                    PlayerPrefs.Save();
                }
                else
                {
                    Debug.Log($"[DotController] ChangeState for '{OutAnimKey}' blocked by AfterScript.");
                    return;
                }
            }
        }

        _Internal_SetAnimation(state, OutAnimKey, OutPos, OutExpression);
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
        //inactive 상태일때 오류 방지
        if (gameObject.activeInHierarchy)
            StartCoroutine(DotvisibleCheck(set));

    }
    public IEnumerator DotvisibleCheck(bool setoff)
    {
        yield return new WaitForSeconds(0.01f);
        if (setoff)
        {
            Invisible();
        }
        else
        {
            Visible();
        }
    }

    public void StartSubDialogueAnimation(DotPatternState state, string animKey, float position)
    {
        Debug.Log($"[DotController] Starting Sub-dialogue animation: {animKey}");
        isSubDialogueAnimPlaying = true;
        ChangeState(state, animKey, position, "", false); // force = true로 강제 실행
    }

    public void StopSubDialogueAnimation()
    {
        if (!isSubDialogueAnimPlaying) return;
        Debug.Log("[DotController] Stopping Sub-dialogue animation state.");
        isSubDialogueAnimPlaying = false;
        UpdateIdleAnimation(); // IDLE 상태로 복귀
    }

    public void PlayAfterScript(string animKey, float position)
    {
        if (string.IsNullOrEmpty(animKey)) return;

        string currentMudAnim = "anim_mud_day" + chapter.ToString();
        if (this.animKey == currentMudAnim)
        {
            Debug.Log($"[DotController] PlayAfterScript blocked because {currentMudAnim} is playing.");
            return;
        }

        if (isSubDialogueAnimPlaying)
        {
            Debug.Log($"[DotController] PlayAfterScript: Resetting isSubDialogueAnimPlaying to false.");
            isSubDialogueAnimPlaying = false;
        }

        Debug.Log($"[DotController] Starting AfterScript: {animKey} at {position}");
        isAfterScriptPlaying = true;

        PlayerPrefs.SetString("AS_AnimKey", animKey);
        PlayerPrefs.SetFloat("AS_Pos", position);
        PlayerPrefs.SetInt("AS_IsPlaying", 1);
        PlayerPrefs.Save();

        ChangeState(DotPatternState.Default, animKey, position, "", true);
    }

    private void StopAfterScript()
    {
        Debug.Log("[DotController] Stopping AfterScript");
        isAfterScriptPlaying = false;
        
        if (isSubDialogueAnimPlaying)
        {
            Debug.Log("[DotController] StopAfterScript: Resetting isSubDialogueAnimPlaying to false.");
            isSubDialogueAnimPlaying = false;
        }

        PlayerPrefs.DeleteKey("AS_AnimKey");
        PlayerPrefs.DeleteKey("AS_Pos");
        PlayerPrefs.DeleteKey("AS_IsPlaying");
        PlayerPrefs.Save();

        // 14일차 Watching: AfterScript 종료 후 anim_mud로 복귀
        if (manager.Pattern == GamePatternState.Watching && chapter == 14)
        {
            Debug.Log("[DotController] 14일차 Watching: AfterScript 종료 후 anim_mud_day13으로 복귀");
            PlayMudAnimation(14);
        }
        else if (manager.Pattern == GamePatternState.Writing)
        {
            // Writing 페이즈: AfterScript 종료 후 anim_diary로 복귀
            Debug.Log("[DotController] Writing 페이즈: AfterScript 종료 후 anim_diary로 복귀");
            ChangeState(DotPatternState.Phase, "anim_diary", -1, "", true);
        }
        else
        {
            UpdateIdleAnimation();
        }
    }

    // Trigger 함수들에서 호출할 강제 종료 헬퍼 (UpdateIdleAnimation 호출 안 함)
    private void ForceStopAfterScript()
    {
        if (isAfterScriptPlaying)
        {
            Debug.Log("[DotController] Force stopping AfterScript for Event Trigger.");
            isAfterScriptPlaying = false;
            PlayerPrefs.DeleteKey("AS_AnimKey");
            PlayerPrefs.DeleteKey("AS_Pos");
            PlayerPrefs.DeleteKey("AS_IsPlaying");
            PlayerPrefs.Save();
        }
    }

    // 기본 랜덤 애니메이션 재생
    // 애니메이션 우선순위 로직
    public void UpdateIdleAnimation()
    {
        // 0. 서브 다이얼로그 애니메이션 재생 중이면 아무것도 하지 않음
        // 1. AfterScript가 활성화 상태이면 AfterScript를 재생
        if (isAfterScriptPlaying) {
            Debug.Log($"[DotController] AfterScript is active, playing '{PlayerPrefs.GetString("AS_AnimKey")}'.");
            ChangeState(DotPatternState.Default, PlayerPrefs.GetString("AS_AnimKey"), PlayerPrefs.GetFloat("AS_Pos"), "", true);
            return;
        }

        // 2. 페이즈별 예외 처리
        if (_TryPlayPhaseExceptionAnimation()) {
            return;
        }

        // 3. 일반적인 일차/페이즈별 기본 로직
        switch (manager.Pattern)
        {
            case GamePatternState.Thinking:
                string randomAnim = GetRandomAnimationForChapter(chapter);
                Debug.Log($"[DotController] Playing daily random animation for Thinking phase: {randomAnim}");
                ChangeState(DotPatternState.Default, randomAnim, -1);
                break;
            case GamePatternState.Watching:
                // Watching: States.cs에서 외출 시 SetActive(false) 처리함.
                // 따라서 여기 들어왔다는 것은 외출하지 않았다는 뜻이므로 anim_mud 재생 (상태 유지)
                PlayMudAnimation(manager.Chapter);
                break;
            case GamePatternState.Sleeping:
                // Sleeping 페이즈의 기본 애니메이션 복구
                // Sleeping: 기본 애니메이션 anim_sleep 고정 (랜덤 X)
                Debug.Log($"[DotController] Playing default animation for Sleeping phase: anim_sleep");
                ChangeState(DotPatternState.Trigger, "anim_sleep", 10);
                // Trigger 타입으로 재생하여 우선순위 확보
                ChangeState(DotPatternState.Trigger, "anim_sleep", 10, "", true);
                break;
            case GamePatternState.Writing:
                // Writing 페이즈의 기본 애니메이션 복구
                // Writing: 기본 애니메이션 anim_diary 고정 (랜덤 X)
                Debug.Log($"[DotController] Playing default animation for Writing phase: anim_diary");
                ChangeState(DotPatternState.Phase, "anim_diary", -1, "", true);
                break;
            default:
                Debug.Log($"[DotController] No specific idle animation for phase '{manager.Pattern}'.");
                break;
        }
    }

    //예외처리
    private bool _TryPlayPhaseExceptionAnimation()
    {
        // 7일차 phase_thinking 예외처리
        if (manager.Pattern == GamePatternState.Thinking && chapter == 7)
        {
            // subseq 1 (sub_feelings)을 아직 보지 않았다면 예외 애니메이션 재생
            if (pc != null && !pc.IsSubWatched(1))
            {
                string[] ch7Animations = { "anim_sub_ch7_1", "anim_sub_ch7_2" };
                string randomAnim = ch7Animations[UnityEngine.Random.Range(0, ch7Animations.Length)];
                Debug.Log($"[DotController] 7일차 Thinking 예외 애니메이션 재생: {randomAnim}");
                ChangeState(DotPatternState.Default, randomAnim, -1);
                return true;
            }
        }

        //14일차 phase_watching 예외처리는 SubDialogue의 Subexit()에서 처리
        return false;
    }

    private string GetRandomAnimationForChapter(int chapter)
    {
        List<string> animList = new List<string>();
        List<string> defaultSet =   new List<string> { 
            "anim_reading", "anim_bed", "anim_mold", 
            "anim_laptop", "anim_walking", "anim_mold2", "anim_spiderweb1", 
            "anim_spiderweb2", "anim_eyesclosed", "anim_eyescorner", "anim_eyesdown", 
            "anim_eyesside", "anim_eyesup", "anim_sleepy_bed", "anim_sleepy_spiderweb" };
        
        switch (chapter)
        {
            case 1:
                animList.AddRange(defaultSet);
                animList.Add("anim_happy");
                break;
            case 2:
            case 5:
            case 8:
            case 12:
            case 13:
                animList.AddRange(defaultSet);
                break;
            case 3:
                animList.AddRange(new string[] { "anim_mud_day2", "anim_omg", "anim_sleepy_bed", "anim_sleepy_spiderweb", "anim_mold2" });
                break;
            case 4:
                animList.AddRange(defaultSet);
                animList.Add("anim_mud_day3");
                break;
            case 6:
                animList.AddRange(new string[] { "anim_omg", "anim_mud_day5", "anim_reading", "anim_walking", "anim_laptop" });
                break;
            case 7:
                animList.AddRange(new string[] { "anim_sleepy_bed", "anim_sleepy_spiderweb", "anim_reading", "anim_reading", "anim_bed" });
                break;
            case 9:
                animList.AddRange(new string[] { "anim_mud_day8", "anim_omg", "anim_eyesdown" });
                break;
            case 10:
                animList.AddRange(new string[] { "anim_mud_day9", "anim_omg", "anim_eyeswide", "anim_mold2", "anim_sleepy_spiderweb" });
                break;
            case 11:
                animList.AddRange(new string[] { "anim_mud_day10", "anim_walking", "anim_omg", "anim_eyesdown" });
                break;
            case 14:
                animList.AddRange(new string[] { "anim_reading", "anim_writing", "anim_mud_day13", "anim_eyesclosed", "anim_bed", "anim_walking", "anim_eyesdown" });
                break;
            default:
                animList.AddRange(defaultSet);
                break;
        }

        return animList[UnityEngine.Random.Range(0, animList.Count)];
    }

    public void PlayMudAnimation(int chapter)
    {
        if (chapter > 1)
        {
            if (isAfterScriptPlaying)
            {
                Debug.Log($"[DotController] PlayMudAnimation is blocked because AfterScript is playing.");
                return;
            }
            int mudDay = chapter-1;

            string mudName = "anim_mud_day" + mudDay.ToString();
            ChangeState(DotPatternState.Phase, mudName, 3);
        }
    }

    private void InitializeAnimationPositions()
    {
        if (!DotPositionKeyDic.ContainsKey(DotPatternState.Default))
        {
            DotPositionKeyDic[DotPatternState.Default] = new Dictionary<string, List<float>>();
        }

        var dic = DotPositionKeyDic[DotPatternState.Default];

        void SetPos(string key, params float[] pos)
        {
            if (dic.ContainsKey(key)) dic[key] = new List<float>(pos);
            else dic.Add(key, new List<float>(pos));
        }

        SetPos("anim_default", 0, 1, 3, 5, 6, 8, 11);
        SetPos("anim_bed", 6, 8);
        SetPos("anim_reading", 0, 1, 3, 5, 6);
        SetPos("anim_writing", 6, 8);
        SetPos("anim_mold", 0.5f);
        SetPos("anim_bounce", 0, 1, 3, 5, 6, 8, 11);
        SetPos("anim_laptop", 7);
        SetPos("anim_walking", 5, 8);
        SetPos("anim_mold2", 0);
        SetPos("anim_happy", 1, 3, 6, 8);
        SetPos("anim_spiderweb1", 10);
        SetPos("anim_spiderweb2", 10);
        SetPos("anim_eyesclosed", 0, 1, 3, 5, 6, 8, 11);
        SetPos("anim_eyescorner", 0, 1, 3, 5, 6, 8, 11);
        SetPos("anim_eyesdown", 0, 1, 3, 5, 6, 8, 11);
        SetPos("anim_eyesside", 0, 1, 3, 5, 6, 8, 11);
        SetPos("anim_eyesup", 0, 1, 3, 5, 6, 8, 11);
        SetPos("anim_sleepy_bed", 6, 8);
        SetPos("anim_sleepy_spiderweb", 10);
        for (int i = 1; i <= 14; i++) SetPos($"anim_mud_day{i}", 3.5f);
        SetPos("anim_eyeswide", 0, 1, 3, 5, 6, 8, 11);
        SetPos("anim_eyesblink", 0, 1, 3, 5, 6, 8, 11);
        SetPos("anim_move", 0, 1, 3, 5, 6, 8, 11);
        SetPos("anim_sleep", 10);
        SetPos("anim_sub_ch7_1", 0, 1, 3, 5, 6, 8, 11);
        SetPos("anim_sub_ch7_2", 0, 1, 3, 5, 6, 8, 11);
    }
}
