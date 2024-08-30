using System;
using System.Collections;
using System.Collections.Generic;
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
                    Debug.Log(enumVal.ToString());
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

        watching = objectManager.GetWatchingObject(pattern[manager.Chapter]);

        if(watching!=null)
        {
            watching.OpenWatching(manager.Chapter);
        }
        else
        {
            //Dot��ġ watching ǥ��
            dot.ChangeState(DotPatternState.Phase, "anim_watching",1.5f);
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
public class MainA : GameState
{
    //��� ���� ��� 
    public override void Init()
    {
      
    }

    public override void Enter(GameManager manager, DotController dot = null)
    {
        //��縦 �ε����� ������.
    }

    //���� idx, �Ѱ������� ��縦 �ִ� �׷��� �Լ� 

    public override void Exit(GameManager manager)
    {

    }
}

public class Thinking : GameState
{
    public override void Init()
    {
    }
    public override void Enter(GameManager manager, DotController dot = null)
    {

    }

    public override void Exit(GameManager manager)
    {

    }
}

public class MainB : GameState
{
    public override void Init()
    {
    }

    public override void Enter(GameManager manager, DotController dot = null)
    {
    }

    public override void Exit(GameManager manager)
    {

    }
}

public class Writing : GameState
{
    public override void Init()
    {
    }

    public override void Enter(GameManager manager, DotController dot = null)
    {

    }

    public override void Exit(GameManager manager)
    {

    }
}

public class Play : GameState
{
    public override void Init()
    {
    }
    public override void Enter(GameManager manager, DotController dot = null)
    {

    }
    public override void Exit(GameManager manager)
    {

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

        sleeping.OpenSleeping();
        
    }

    public override void Exit(GameManager manager)
    {

    }
}
