using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class MainDialogue : GameState
{
    //���

    protected GameObject background = null;
    protected DotController dot = null;

    public override void Enter(GameManager manager, DotController dot = null)
    {

        //�����δ� ��ġ�� ���� ���.
        //dot State ���� -> Ŭ�� �� �Ʒ� �ΰ� ���� �� SetMain ����.
        this.dot = dot;
        dot.TriggerMain(true);
        dot.ChangeState(DotPatternState.Defualt, "anim_default");
    }

    //������ ���⿡ �Լ� ���������� �Ķ���Ͷ� ���ϰ� �� �ʰ� �ʿ��Ѵ�� �ٲ�
    public abstract string GetData(int index);

    public void StartMain(GameManager manager)
    {
        //��縦 �ε����� ������.
        //���ȭ���� �ε��Ѵ�.
        //ī�޶� 0,0,10���� ������Ų��.�������� ���ϰ��Ѵ�.

        manager.ScrollManager.StopCamera(true);
        background = manager.ObjectManager.SetMain("main_door_open");
        //�Ķ���ͷ� ��氪�� �����ϸ� �ȴ�.
        //Day 7�� �����ϰ� ��� ��氪�� Enter���� �����ϸ� �ǰ�, ���� 7�϶��� �������ش�.
    }
    public override void Exit(GameManager manager)
    {
        dot.TriggerMain(false);
        manager.ScrollManager.StopCamera(false);
        if (background)
        {
            background.SetActive(false);
        }
    }
}
