using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public enum EWatching
{
    Binocular,
    Letter,
    StayAtHome,
    None
}
public class Watching : GameState
{
    
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
            return;
        }

        if(objectManager == null)
        {
            objectManager = manager.ObjectManager;
        }
        objectManager.SettingChapter(manager.Chapter);

        watching = objectManager.GetWatchingObject(pattern[manager.Chapter]);
        
        if (watching!=null)
        {
            if(dot)
            {
                dot.gameObject.SetActive(false);
            }
            watching.OpenWatching(manager.Chapter);
        }
        else
        {
            dot.ChangeState(DotPatternState.Defualt, "anim_mud");
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
}

//MainA/MainB �������̽� ����ؼ� �Լ� �ϳ� ������ �� �ֵ��� �ϸ� ���ڽ�.
public class MainA : MainDialogue
{
    
    //��� ���� ��� 
    public override void Init()
    {
      
    }

    public override string GetData(int idx)
    {
       
        //�����Ϳ� ���� �ִϸ��̼����� �����Ѵ�., fixedPos �� �ǵ帮������!!! ��ġ ���ε� �׻� ����
        dot.ChangeState(DotPatternState.Main, "body_default1", fixedPos, "face_null");
        return null; //data[idx].Kor
    }


}

public class Thinking : GameState
{
    public override void Init()
    {
    }

    public override void Enter(GameManager manager, DotController dot = null)
    {
        //Default�� �������� ��뿹��
        DotAnimState anim = (DotAnimState)UnityEngine.Random.Range(0, (int)DotAnimState.anim_eyesblink);
        manager.ObjectManager.PlayThinking();

        Debug.Log(anim.ToString());
        dot.ChangeState(DotPatternState.Defualt, anim.ToString());
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

    public override string GetData(int idx)
    {
        
        //�����Ϳ� ���� �ִϸ��̼����� �����Ѵ�.,fixedPos �� �ǵ帮������!!! ��ġ ���ε� �׻� ����
        dot.ChangeState(DotPatternState.Main, "body_default1", fixedPos, "face_null");
        return null; //data[idx].Kor
    }

}

public class Writing : GameState
{
    public override void Init()
    {
    }

    public override void Enter(GameManager manager, DotController dot = null)
    {
        manager.ObjectManager.PlayThinking();
        dot.ChangeState(DotPatternState.Phase, "anim_diary");
    }

    public override void Exit(GameManager manager)
    {

    }
}

public class Play : GameState
{
    DotController dot =null;

    const int pos = 18;
    const string anim = "anim_trigger_play";
    public override void Init()
    {
    }
    public override void Enter(GameManager manager, DotController dot = null)
    {
        manager.ObjectManager.PlayThinking();
        this.dot = dot;
        dot.TriggerPlay(true);
        dot.ChangeState(DotPatternState.Tirgger, anim, pos);
    }
    public override void Exit(GameManager manager)
    {
        if(dot)
        {
            dot.TriggerPlay(false);
        }
        //�ڷ����� �ִϸ��̼� ���⿡ �߰��Ѵ�.
    }
}

public class Sleeping : GameState
{
    ISleepingInterface sleeping;
    public override void Init()
    {
    }

    public override void Enter(GameManager manager, DotController dot = null)
    {
        if (objectManager == null)
        {
            objectManager = manager.ObjectManager;
        }

        if(sleeping == null)
        {
            sleeping = objectManager.GetSleepingObject();
        }

        dot.ChangeState(DotPatternState.Tirgger, "anim_sleep", 10);

        manager.ObjectManager.PlayThinking();
        sleeping.OpenSleeping();
        
    }

    public override void Exit(GameManager manager)
    {

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