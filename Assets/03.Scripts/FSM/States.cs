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
    public override void Init()
    {
        if (pattern.Count <= 0)
        {
            if (DataManager.Instance.Settings == null) return;

            foreach (string strVal in DataManager.Instance.Settings.watching.pattern)
            {
                EWatching enumVal;
                if (Enum.TryParse(strVal, true, out enumVal))
                {
                    pattern.Add(enumVal);
                }
            }
        }

    }

    public override void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        if (objectManager == null)
        {
            objectManager = manager.ObjectManager;
        }
        objectManager.SettingChapter(manager.Chapter);

        watching = objectManager.GetWatchingObject(pattern[manager.Chapter]);
        
        if (watching!=null)
        {
            if (dot)
            {
                dot.gameObject.SetActive(false);
            }
            watching.OpenWatching(manager.Chapter);
        }
        //Stay일 때 뭉치 등장
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
        ScriptList scriptList = dot.GetMainScriptList(0);
        dot.ChangeState(DotPatternState.Default, scriptList.DotAnim, scriptList.DotPosition);

        base.Enter(manager, dot);
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
        //Default값 랜덤으로 사용예정
        
        //코루틴으로 N분 뒤에 실행 할 수 있도록 변경하기
        if (RunSubScript(dot, manager) == false)
        {
            Think(manager, dot);
        }
    }

    public void ResetState(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        DotAnimState anim = (DotAnimState)UnityEngine.Random.Range(0, (int)DotAnimState.anim_eyesblink);

        dot.ChangeState(DotPatternState.Default, anim.ToString());
    }
    public void Think(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        //Default값 랜덤으로 사용예정

        DotAnimState anim = (DotAnimState)UnityEngine.Random.Range(0, (int)DotAnimState.anim_eyesblink);

        dot.ChangeState(DotPatternState.Default, anim.ToString());

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
        ScriptList scriptList = dot.GetMainScriptList(1);
        dot.ChangeState(DotPatternState.Default, scriptList.DotAnim, scriptList.DotPosition);

        base.Enter(manager, dot);
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
        if(RunSubScript(dot, manager) == false)
        {
            Write(manager, dot);
        }
    }

    public void Write(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        manager.ObjectManager.PlayThinking();
        manager.ObjectManager.ShowDiary(false);
        dot.ChangeState(DotPatternState.Phase, "anim_diary");
        //다이어리 업데이트
    }
   
    public override void Exit(GameManager manager, TutorialManager tutomanger = null)
    {
        manager.ObjectManager.ShowDiary(true);
    }

    public void ResetState(GameManager manager, DotController dot = null)
    {
        dot.ChangeState(DotPatternState.Phase, "anim_diary");
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
        manager.ScrollManager.StopCameraByPlayPhase(true);
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
        if (RunSubScript(dot, manager) == false)
        {
            Sleep(manager, dot);
        }
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
        dot.ChangeState(DotPatternState.Trigger, "anim_sleep", 10);
        dot.Dust.SetActive(true);
    }

    public override void Exit(GameManager manager, TutorialManager tutomanger = null)
    {
        this.dot.Dust.SetActive(false);
    }

    public void ResetState(GameManager manager, DotController dot = null)
    {
        dot.ChangeState(DotPatternState.Trigger, "anim_sleep", 10);
        dot.Dust.SetActive(true);
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

