using Assets.Script.DialClass;
using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public abstract class GameState
{
    protected ObjectManager objectManager = null;
    
    public bool RunSubScript(DotController dot, GameManager manager)
    {
        Debug.Log("체크용" + dot.GetSubScriptListCount(manager.Pattern));
        
        if (dot.GetSubScriptListCount(manager.Pattern) == 0)
        {
            return false;
        }

        ScriptList sub = dot.GetSubScriptList(manager.Pattern);

        string animString = sub.DotAnim;
        float Position = sub.DotPosition;

        DotPatternState dotPatternState = DotPatternState.Default;
        Enum.TryParse(sub.AnimState, true, out dotPatternState);
        dot.StartSubDialogueAnimation(dotPatternState, animString, Position);
        manager.ShowSubDial();

        Debug.Log("현재 서브다이얼로그 진행중 " + animString + " " + Position);
        return true;
    }

    public abstract void Init();
    public abstract void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null);
    public abstract void Exit(GameManager manager, TutorialManager tutomanger = null);
}
