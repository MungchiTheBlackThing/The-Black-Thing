using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class MainDialogue : GameState
{
    //���
    Dictionary<string, int> pos = new Dictionary<string, int>();
    protected GameObject background = null;
    protected DotController dot = null;

    protected int fixedPos = -1;

    public MainDialogue()
    {
        pos.Add("main_bed", 14);
        pos.Add("main_table", 15);
        pos.Add("main_door_close", 16);
        pos.Add("main_door_open", 16);
        pos.Add("main_web", 17);
    }

    public override void Enter(GameManager manager, DotController dot = null)
    {
        if (dot)
        {
            dot.gameObject.SetActive(true);
        }
        manager.ObjectManager.PlayThinking();
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
        background = manager.ObjectManager.SetMain("main_door_open"); // ���� ����� � ������ ����

        fixedPos = pos["main_door_open"]; //���� ���ȭ���� � ������ �������ֱ�
        //���ȭ���� ���� ��, ��ġ�� ��ġ�� �����Ѵ�.
        dot.ChangeState(DotPatternState.Main, "body_default1", fixedPos, "face_null");
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
