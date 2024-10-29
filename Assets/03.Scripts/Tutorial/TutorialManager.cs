using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tutorial;
using static UnityEditor.VersionControl.Asset;
public enum TutorialState
{
    Sub,
    Main,
    End,//�� �ܰ�� �Ѿ�� ����, �����ܰ� 0���� �̵��ؾ���.
};


public class TutorialManager : GameManager
{
    private Dictionary<TutorialState, GameState> states;
    private TutorialState currentPattern;

    TutorialManager()
    {
        states = new Dictionary<TutorialState, GameState>();

        states[TutorialState.Sub] = new Tutorial.Sub();
        states[TutorialState.Main] = new Tutorial.Main();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeGameState(TutorialState patternState)
    {
        if (states == null) return;

        if (states.ContainsKey(patternState) == false)
        {
            Debug.Log("���� ���� �Դϴ�.");
            return;
        }
        if (activeState != null)
        {
            activeState.Exit(this); //�̸� �����Ѵ�.
        }
        currentPattern = patternState;
        activeState = states[patternState];
        activeState.Enter(this, dot);
    }
}
