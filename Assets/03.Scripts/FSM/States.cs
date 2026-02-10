using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Assets.Script.DialClass;
using System.IO;
using System.Threading.Tasks;
public enum EWatching
{
    Binocular,
    Letter,
    StayAtHome,
    None
}
public class Watching : GameState, IResetStateInterface
{

    const GamePatternState state = GamePatternState.Watching;

    //뭉치의 외출 여부를 알아야한다.
    List<EWatching> pattern = new List<EWatching>();
    IWatchingInterface watching = null;

    private Coroutine _retryEnterCo;
    private int _retryEnterCount;
    public override void Init()
    {
        if (pattern.Count <= 0)
        {
            if (DataManager.Instance.Watchinginfo == null) return;

            foreach (string strVal in DataManager.Instance.Watchinginfo.pattern)
            {
                EWatching enumVal;
                if (Enum.TryParse(strVal, true, out enumVal))
                {
                    pattern.Add(enumVal);
                }
                else
                {
                    pattern.Add(EWatching.None);
                }
            }
        }

    }

    public override void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        // 1) 먼저 init 시도
        if (pattern.Count == 0) Init();

        // 3) 인덱스 계산 (챕터 1~n → 0~n-1)

        int idx = manager.Chapter;

        // 4) 호출 이상한 경우 재시도하고 return
        if (idx < 0 || idx >= pattern.Count)
        {
            if (_retryEnterCo == null && _retryEnterCount < 60)
                _retryEnterCo = manager.StartCoroutine(RetryEnterNextFrame(manager, dot, tutomanger));
            return;
        }

        _retryEnterCount = 0;
        _retryEnterCo = null;

        if (objectManager == null)
        {
            objectManager = manager.ObjectManager;
        }
        objectManager.SettingChapter(manager.Chapter, manager.Pattern);

        EWatching currentPattern = pattern[idx];
        
        // StayAtHome일 때도 쪽지(Letter) 기능을 사용해야 하므로 Letter 컨트롤러를 가져오도록 설정
        EWatching targetController = currentPattern;
        if (currentPattern == EWatching.StayAtHome)
        {
            targetController = EWatching.Letter;
        }

        watching = objectManager.GetWatchingObject(targetController);
    
        // watching 오브젝트 유무와 상관없이, 패턴이 Binocular거나 Letter면 외출(Dot 숨김) 처리
        if (currentPattern == EWatching.Binocular || currentPattern == EWatching.Letter)
        {
            if (dot) dot.gameObject.SetActive(false);
        }
        else // StayAtHome 등: 뭉치 등장 (anim_mud 재생)
        {
            if (dot != null) dot.PlayMudAnimation(manager.Chapter);
        }

        // 패턴에 맞는 Watching 오브젝트(Alert) 활성화 (Binocular, Letter, StayAtHome 모두 포함)
        if (watching != null)
        {
            watching.OpenWatching(manager.Chapter);
        }
    }

    private IEnumerator RetryEnterNextFrame(GameManager manager, DotController dot, TutorialManager tutomanger)
    {
        _retryEnterCount++;
        yield return null;
        _retryEnterCo = null;
        Enter(manager, dot, tutomanger);
    }

    public override void Exit(GameManager manager, TutorialManager tutomanger = null)
    {
        if(watching != null)
        {
            watching.CloseWatching();     
        }
    }

    public void ResetState(GameManager manager, DotController dot = null)
    {
        //watching은 어떤작업하는지 잘모르겠네..?
    }
}

//MainA/MainB 인터페이스 사용해서 함수 하나 연결할 수 있도록 하면 좋겠슴.
public class MainA : MainDialogue
{

    //멤버 변수 대사 
    public override void Init()
    {
      
    }

    public override void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        // Dot을 활성화하고 기본 설정을 하기 위해 base.Enter를 먼저 호출
        base.Enter(manager, dot);

        Debug.Log("[MainA] Enter - Attempting to play entry animation from ScriptList(0)");
        ScriptList scriptList = dot.GetMainScriptList(0);
        
        if (scriptList != null)
        {
            Debug.Log($"[MainA] Playing animation: {scriptList.DotAnim} at {scriptList.DotPosition}");
            dot.ChangeState(DotPatternState.Main, scriptList.DotAnim, scriptList.DotPosition);
        }
        else
        {
            Debug.LogError("[MainA] Failed to retrieve ScriptList(0)");
        }
    }
    public override void Exit(GameManager manager, TutorialManager tutomanger = null)
    {
        
    }

}

public class Thinking : GameState, ILoadingInterface
{
    const GamePatternState state = GamePatternState.Thinking;   

    public override void Init()
    {

    }

    public override void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        // The sub-dialogue timer is started by GameManager.ChangeGameState.
        // This state's entry should only handle the "thinking" visuals.
        Think(manager, dot);
    }

    public void ResetState(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        DotAnimState anim = (DotAnimState)UnityEngine.Random.Range(0, (int)DotAnimState.anim_eyesblink);

        dot.ChangeState(DotPatternState.Default, anim.ToString());
    }
    public void Think(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        //Default값 랜덤으로 사용예정
        dot.UpdateIdleAnimation();
        manager.ObjectManager.PlayThinking();
    }

    public override void Exit(GameManager manager, TutorialManager tutomanger = null)
    {

    }
}

public class MainB : MainDialogue
{
    //데이터를 가지고 있는다.

    public override void Init()
    {

    }

    public override void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        // Dot을 활성화하고 기본 설정을 하기 위해 base.Enter를 먼저 호출합니다.
        base.Enter(manager, dot);

        Debug.Log("[MainB] Enter - Attempting to play entry animation from ScriptList(1)");
        ScriptList scriptList = dot.GetMainScriptList(1);

        if (scriptList != null)
        {
            Debug.Log($"[MainB] Playing animation: {scriptList.DotAnim} at {scriptList.DotPosition}");
            dot.ChangeState(DotPatternState.Main, scriptList.DotAnim, scriptList.DotPosition);
        }
        else
        {
            Debug.LogError("[MainB] Failed to retrieve ScriptList(1)");
        }
    }
    public override void Exit(GameManager manager, TutorialManager tutomanger = null)
    {

    }

}

public class Writing : GameState, ILoadingInterface, IResetStateInterface
{
    public override void Init()
    {
    }

    public override void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        Debug.Log("뭉치 일기 써야함");
        Write(manager, dot);
    }

    public void Write(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        manager.ObjectManager.PlayThinking();

        string diaryKey = dot != null ? dot.GetDiaryAnimKeyForChapter(dot.Chapter) : "anim_diary";
        dot.ChangeState(DotPatternState.Phase, diaryKey);
        //다이어리 업데이트
    }
   
    public override void Exit(GameManager manager, TutorialManager tutomanger = null)
    {
    }

    public void ResetState(GameManager manager, DotController dot = null)
    {
        string diaryKey = dot != null ? dot.GetDiaryAnimKeyForChapter(dot.Chapter) : "anim_diary";
        dot.ChangeState(DotPatternState.Phase, diaryKey);
    }
}

public class Play : GameState, ILoadingInterface
{
    DotController dot =null;

    const int pos = 18;
    const string anim = "anim_trigger_play";
    public override void Init()
    {
    }
    public override void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        this.dot = dot;
        manager.ObjectManager.PlayThinking();
        //manager.ScrollManager.StopCameraByPlayPhase(true);
        //카메라 고정
        Debug.Log("트리거 켜짐");
        dot.TriggerPlay(true);
        dot.ChangeState(DotPatternState.Trigger, anim, pos);
    }
    public override void Exit(GameManager manager, TutorialManager tutomanger = null)
    {
        manager.ScrollManager.StopCameraByPlayPhase(false);
    }
}

public class Sleeping : GameState, IResetStateInterface
{
    ISleepingInterface sleeping;
    DotController dot;
    public override void Init()
    {
    }

    public override void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        // The sub-dialogue timer is started by GameManager.ChangeGameState.
        // This state's entry should only handle the "sleeping" visuals.
        Sleep(manager, dot);
    }
    public void Sleep(GameManager manager, DotController dot, TutorialManager tutomanger = null)
    {
        this.dot = null;

        if (objectManager == null)
        {
            objectManager = manager.ObjectManager;
        }

        if (sleeping == null)
        {
            sleeping = objectManager.GetSleepingObject();
        }

        manager.ObjectManager.PlayThinking();

        sleeping.OpenSleeping();
       
        this.dot = dot;
        string sleepKey = (dot != null) ? dot.GetSleepAnimKeyForChapter(dot.Chapter) : "anim_sleep";
        dot.ChangeState(DotPatternState.Trigger, sleepKey, 10);
    }

    public override void Exit(GameManager manager, TutorialManager tutomanger = null)
    {
    }

    public void ResetState(GameManager manager, DotController dot = null)
    {
        string sleepKey = (dot != null) ? dot.GetSleepAnimKeyForChapter(dot.Chapter) : "anim_sleep";
        dot.ChangeState(DotPatternState.Trigger, sleepKey, 10);
    }
}

public class NextChapter : GameState, ILoadingInterface
{
    public override void Init()
    {
    }

    public override void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        Debug.Log("SkipSleepingON");
        //다음 챕터로 넘어가는 달나라를 띄운다.
        if (objectManager == null)
        {
            objectManager = manager.ObjectManager;
        }

        manager.ObjectManager.SkipSleeping(true);
    }

    public override void Exit(GameManager manager, TutorialManager tutomanger = null)
    {
        manager.ObjectManager.SkipSleeping(false);
    }
}
