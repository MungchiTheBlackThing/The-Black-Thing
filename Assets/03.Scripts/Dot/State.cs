using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : DotState
{
    [SerializeField]
    Dictionary<DotAnimState, List<float>> IdlePos;

    //���¸� ������ �� 1ȸ ȣ�� -> Position �������� ����
    public Idle()
    {
        IdlePos = new Dictionary<DotAnimState, List<float>>();
        reader.ReadJson(this, Resources.Load<TextAsset>("FSM/IdleState"));
    }

    public override void Init(DotAnimState state, List<float> pos)
    {
        IdlePos.Add(state, pos);
    }
    public override void Enter(DotController dot)
    {

        //dot�� animKey�� �����´�.
        //animKey�� ����� List<float> Length �� �� Random.Range �Լ��� ����ؼ� �̴´�.
        //IdlePos[animKey][position]�� �����Ѵ�(�ִϸ��̼� ������ȯ).

        DotAnimState anim;

        if (Enum.TryParse(dot.AnimKey, true, out anim))
        {

            //dot�� position�� �����Ǿ��ִ°� Ȯ���Ѵ�. -1�� �������� ����, n�� ����
            //������ ���, IdlePos[animKey][position]�� �����Ѵ�(�ִϸ��̼� ������ȯ).
            if (dot.Position == -1)
            {
                int maxIdx = IdlePos[anim].Count;

                dot.Position = UnityEngine.Random.Range(0, maxIdx);
            }

            dot.transform.localPosition = GetCoordinate(dot.Position); //��ġ ������Ʈ

            dot.Animator.SetInteger("DotAnimState", (int)anim); //�ִϸ��̼� ������Ʈ

            if (anim == DotAnimState.anim_mud)
            {
                //é�͸� �ľ��ؼ�, mold�� �����ų �� ���.
                dot.Animator.SetInteger("Chapter", (int)dot.Chapter);
            }
        }

    }

    //���¸� ���� �� 1ȸ ȣ�� -> Position -1�� ����
    public override void Exit(DotController dot)
    {
        //���� �� ��ġ�� -1�� �ٲ۴�.
        dot.Position = -1;
    }

    //�ӽ� print��
    public override void Read()
    {
        /*foreach (var anim in IdlePos)
        {
            //Debug.Log($"Animation: {anim.Key}, Positions: {string.Join(", ", anim.Value)}");
        }*/
    }
}

public class Main : DotState
{
    [SerializeField]
    Dictionary<DotAnimState, List<float>> MainPos;
    GameObject dotEyes;
    Animator dotEyesAnim; // �� �ִϸ����͵� ������ �ִ´�.

    //���¸� ������ �� 1ȸ ȣ�� -> Position �������� ����
    public Main()
    {
        MainPos = new Dictionary<DotAnimState, List<float>>();
        reader.ReadJson(this, Resources.Load<TextAsset>("FSM/MainState"));

        //1. �����ִ� �ڽ� �� Eyes�� ã�Ƽ� dotEyes�� ������ ���´�.
        dotEyes = GameObject.Find("Dot").transform.Find("DotEyes").gameObject;
        dotEyesAnim = dotEyes.GetComponent<Animator>();
    }

    public override void Init(DotAnimState state, List<float> pos)
    {
        MainPos.Add(state, pos);
    }
    public override void Enter(DotController dot)
    {
        //2. eyes�� Ų��.
        dotEyes.SetActive(true);

        DotEyes eyes;

        if (Enum.TryParse(dot.DotExpression, true, out eyes))
        {
            dotEyesAnim.SetInteger("FaceKey", (int)eyes);
        }

        DotAnimState anim;
        if (Enum.TryParse(dot.AnimKey, true, out anim))
        {
            dot.Animator.SetInteger("DotAnimState", (int)anim); //�ִϸ��̼� ������Ʈ
        }

        //dot.transform.localPosition = GetCoordinate(dot.Position); //��ġ ������Ʈ

    }

    //���¸� ���� �� 1ȸ ȣ�� -> Position -1�� ����
    public override void Exit(DotController dot)
    {
    }

    //�ӽ� print��
    public override void Read()
    {
    }
}

public class Sub : DotState
{
    [SerializeField]
    Dictionary<DotAnimState, List<float>> SubPos;

    //���¸� ������ �� 1ȸ ȣ�� -> Position �������� ����
    public Sub()
    {
        SubPos = new Dictionary<DotAnimState, List<float>>();
        reader.ReadJson(this, Resources.Load<TextAsset>("FSM/SubState"));
    }

    public override void Init(DotAnimState state, List<float> pos)
    {
        SubPos.Add(state, pos);
    }
    public override void Enter(DotController dot)
    {
        DotAnimState anim;
        if (Enum.TryParse(dot.AnimKey, true, out anim))
        {
            dot.Animator.SetInteger("DotAnimState", (int)anim); //�ִϸ��̼� ������Ʈ
        }
    }

    //���¸� ���� �� 1ȸ ȣ�� -> Position -1�� ����
    public override void Exit(DotController dot)
    {
    }

    //�ӽ� print��
    public override void Read()
    {
    }
}

public class Phase : DotState
{
    [SerializeField]
    Dictionary<DotAnimState, List<float>> PhasePos;

    //���¸� ������ �� 1ȸ ȣ�� -> Position �������� ����
    public Phase()
    {
        PhasePos = new Dictionary<DotAnimState, List<float>>();
        reader.ReadJson(this, Resources.Load<TextAsset>("FSM/PhaseState"));
    }

    public override void Init(DotAnimState state, List<float> pos)
    {
        PhasePos.Add(state, pos);
    }
    public override void Enter(DotController dot)
    {
        Debug.Log(2);

        Debug.Log(dot.AnimKey);

        DotAnimState anim;
        if (Enum.TryParse(dot.AnimKey, true, out anim))
        {
            dot.Animator.SetInteger("DotAnimState", (int)anim); //�ִϸ��̼� ������Ʈ
        }
    }

    //���¸� ���� �� 1ȸ ȣ�� -> Position -1�� ����
    public override void Exit(DotController dot)
    {
    }

    //�ӽ� print��
    public override void Read()
    {
    }
}