using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class GameState
{
    protected ObjectManager objectManager = null;
    
    public abstract void Init();
    public abstract void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null);
    public abstract void Exit(GameManager manager, TutorialManager tutomanger = null);
}
