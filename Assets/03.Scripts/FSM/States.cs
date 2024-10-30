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

    //��ġ�� ���� ���θ� �˾ƾ��Ѵ�.
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

    public override void Enter(GameManager manager, DotController dot = null)
    {

        if (pattern[manager.Chapter] == EWatching.None)
        {
            manager.ShowSubDial();
            return;
        }

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
        //Stay�� �� ��ġ ����
    }

    public override void Exit(GameManager manager)
    {
        if(watching != null)
        {
            watching.CloseWatching();
        }
    }

    public void ResetState(GameManager manager, DotController dot = null)
    {
        //watching�� ��۾��ϴ��� �߸𸣰ڳ�..?
    }
}

//MainA/MainB �������̽� ����ؼ� �Լ� �ϳ� ������ �� �ֵ��� �ϸ� ���ڽ�.
public class MainA : MainDialogue
{

    //��� ���� ��� 
    public override void Init()
    {
      
    }

    public override void Enter(GameManager manager, DotController dot = null)
    {
        ScriptList scriptList = dot.GetMainScriptList(0);
        dot.ChangeState(DotPatternState.Default, scriptList.DotAnim, scriptList.DotPosition);

        base.Enter(manager, dot);
    }

}

public class Thinking : GameState, ILoadingInterface
{
    const GamePatternState state = GamePatternState.Thinking;   

    public override void Init()
    {

    }

    public override void Enter(GameManager manager, DotController dot = null)
    {
        //Default�� �������� ��뿹��
        Think(manager,dot);
    }

    public void ResetState(GameManager manager, DotController dot = null)
    {
        DotAnimState anim = (DotAnimState)UnityEngine.Random.Range(0, (int)DotAnimState.anim_eyesblink);
        dot.ChangeState(DotPatternState.Default, anim.ToString());
    }

    public void Think(GameManager manager, DotController dot = null)
    {
        //Default�� �������� ��뿹��
        DotAnimState anim = (DotAnimState)UnityEngine.Random.Range(0, (int)DotAnimState.anim_eyesblink);
        manager.ObjectManager.PlayThinking();
        dot.ChangeState(DotPatternState.Default, anim.ToString());

        manager.ShowSubDial();
    }

    public override void Exit(GameManager manager)
    {

    }
}

public class MainB : MainDialogue
{
    //�����͸� ������ �ִ´�.

    public override void Init()
    {

    }

    public override void Enter(GameManager manager, DotController dot = null)
    {
        ScriptList scriptList = dot.GetMainScriptList(1);
        dot.ChangeState(DotPatternState.Default, scriptList.DotAnim, scriptList.DotPosition);

        base.Enter(manager, dot);
    }

}

public class Writing : GameState, ILoadingInterface, IResetStateInterface
{
    public override void Init()
    {
    }

    public override void Enter(GameManager manager, DotController dot = null)
    {
        Write(manager, dot);
    }

    public void Write(GameManager manager, DotController dot = null)
    {
        manager.ObjectManager.PlayThinking();
        dot.ChangeState(DotPatternState.Phase, "anim_diary");
    }
   
    public override void Exit(GameManager manager)
    {

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
    public override void Enter(GameManager manager, DotController dot = null)
    {
        this.dot = dot;
        manager.ObjectManager.PlayThinking();
        manager.ScrollManager.StopCameraByPlayPhase(true);
        //ī�޶� ����
        dot.TriggerPlay(true);
        dot.ChangeState(DotPatternState.Tirgger, anim, pos);
    }
    public override void Exit(GameManager manager)
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

    public override void Enter(GameManager manager, DotController dot = null)
    {

        Sleep(manager, dot);
        
    }
    public void Sleep(GameManager manager, DotController dot)
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
        dot.ChangeState(DotPatternState.Tirgger, "anim_sleep", 10);
        dot.Dust.SetActive(true);
    }

    public override void Exit(GameManager manager)
    {
        this.dot.Dust.SetActive(false);
    }

    public void ResetState(GameManager manager, DotController dot = null)
    {
        dot.ChangeState(DotPatternState.Tirgger, "anim_sleep", 10);
        dot.Dust.SetActive(true);
    }
}

public class NextChapter : GameState
{
    public override void Init()
    {
    }

    public override void Enter(GameManager manager, DotController dot = null)
    {

        //���� é�ͷ� �Ѿ�� �޳��� ����.
        if (objectManager == null)
        {
            objectManager = manager.ObjectManager;
        }

        manager.ObjectManager.SkipSleeping(true);
    }
    public override void Exit(GameManager manager)
    {
        manager.ObjectManager.SkipSleeping(false);
    }
}

