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

        Debug.Log("SubDialogue " + dot.Position);

        if (Enum.TryParse(dot.AnimKey, true, out anim))
        {

            //dot�� position�� �����Ǿ��ִ°� Ȯ���Ѵ�. -1�� �������� ����, n�� ����
            //������ ���, IdlePos[animKey][position]�� �����Ѵ�(�ִϸ��̼� ������ȯ).
            if (dot.Position == -1)
            {
                int maxIdx = IdlePos[anim].Count;
                
                dot.Position = IdlePos[anim][UnityEngine.Random.Range(0, maxIdx)];
                
            }
            dot.transform.position = GetCoordinate(dot.Position); //��ġ ������Ʈ
            if (anim == DotAnimState.anim_mud)
            {
                //é�͸� �ľ��ؼ�, mold�� �����ų �� ���.
                dot.Animator.SetInteger("Chapter", dot.Chapter);
            }

            dot.Animator.SetInteger(Animator.StringToHash("DotAnimState"), (int)anim); //�ִϸ��̼� ������Ʈ
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

    //���¸� ������ �� 1ȸ ȣ�� -> Position �������� ����
    public Main()
    {
        MainPos = new Dictionary<DotAnimState, List<float>>();
        reader.ReadJson(this, Resources.Load<TextAsset>("FSM/MainState"));
    }

    public override void Init(DotAnimState state, List<float> pos)
    {
        MainPos.Add(state, pos);
    }

    public override void Enter(DotController dot)
    {
        dot.Eyes.SetActive(true);

        DotEyes eyes;

        if (Enum.TryParse(dot.DotExpression, true, out eyes))
        {
            dot.EyesAnim.SetInteger("FaceKey", (int)eyes);
        }

        DotAnimState anim;
        if (Enum.TryParse(dot.AnimKey, true, out anim))
        {
            //Debug.Log(dot.AnimKey);
            dot.Animator.SetInteger(Animator.StringToHash("DotAnimState"), (int)anim); //�ִϸ��̼� ������Ʈ
            dot.transform.position = GetCoordinate(dot.Position);
        }
    }

    //���¸� ���� �� 1ȸ ȣ�� -> Position -1�� ����
    public override void Exit(DotController dot)
    {
        dot.Eyes.SetActive(false);
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
            //dot�� position�� �����Ǿ��ִ°� Ȯ���Ѵ�. -1�� �������� ����, n�� ����
            //������ ���, IdlePos[animKey][position]�� �����Ѵ�(�ִϸ��̼� ������ȯ).
            if (dot.Position == -1)
            {
                int maxIdx = SubPos[anim].Count;

                Debug.Log("�ƽ� �ε���: " + maxIdx);

                dot.Position = SubPos[anim][UnityEngine.Random.Range(0, maxIdx)];
                Debug.Log("�̰� ��� �۵��Ǵ°� ������?:" + dot.Position);
            }
            dot.Animator.SetInteger(Animator.StringToHash("DotAnimState"), (int)anim); //�ִϸ��̼� ������Ʈ
        }
        dot.transform.position = GetCoordinate(dot.Position); //��ġ ������Ʈ
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
        DotAnimState anim;
        if (Enum.TryParse(dot.AnimKey, true, out anim))
        {
            dot.Position = PhasePos[anim][0];
            dot.transform.position = GetCoordinate(dot.Position); //��ġ ������Ʈ
            dot.Animator.SetInteger(Animator.StringToHash("DotAnimState"), (int)anim); //�ִϸ��̼� ������Ʈ
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

public class Trigger : DotState
{

    public override void Init(DotAnimState state, List<float> pos)
    {
        
    }

    public override void Enter(DotController dot)
    {
        DotAnimState anim;
        if (Enum.TryParse(dot.AnimKey, true, out anim))
        {
            dot.transform.position = GetCoordinate(dot.Position); //��ġ ������Ʈ
            Debug.Log(dot.transform.position);
            dot.Animator.SetInteger("DotAnimState", (int)anim); //�ִϸ��̼� ������Ʈ
        }
    }

    public void GoSleep(DotController dot)
    {
        //���ڷ� ���� �ִϸ��̼� ����.
        dot.Position = 19;
        dot.transform.position = GetCoordinate(dot.Position); //��ġ ������Ʈ
        dot.Animator.SetInteger(Animator.StringToHash("DotAnimState"), (int)DotAnimState.phase_sleep); //�ִϸ��̼� ������Ʈ
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

public class DotTutorial : DotState
{
    public override void Init(DotAnimState state, List<float> pos)
    {

    }

    public override void Enter(DotController dot)
    {
        Debug.Log("��ġ ��Ʈ��");
        dot.transform.position = GetCoordinate(dot.Position);
        Debug.Log(dot.Position);
        dot.Animator.SetInteger(Animator.StringToHash("DotAnimState"), (int)DotAnimState.anim_default);
        Debug.Log("Ʃ�� �ִ�" + dot.AnimKey);//�ִϸ��̼� ������Ʈ

    }
    public override void Exit(DotController dot)
    {

    }

    public override void Read()
    {

    }
}
