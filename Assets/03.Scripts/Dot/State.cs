using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : DotState
{
    [SerializeField]
    Dictionary<DotAnimState, List<float>> IdlePos;

    //상태를 시작할 때 1회 호출 -> Position 랜덤으로 선택
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

        //dot의 animKey를 가져온다.
        //animKey의 저장된 List<float> Length 값 중 Random.Range 함수를 사용해서 뽑는다.
        //IdlePos[animKey][position]을 동작한다(애니메이션 상태전환).
        DotAnimState anim;

        Debug.Log("SubDialogue " + dot.Position);

        if (Enum.TryParse(dot.AnimKey, true, out anim))
        {

            //dot의 position이 지정되어있는가 확인한다. -1은 지정되지 않음, n은 지정
            //지정된 경우, IdlePos[animKey][position]을 동작한다(애니메이션 상태전환).
            if (dot.Position == -1)
            {
                int maxIdx = IdlePos[anim].Count;
                
                dot.Position = IdlePos[anim][UnityEngine.Random.Range(0, maxIdx)];
                
            }
            dot.transform.position = GetCoordinate(dot.Position); //위치 업데이트
            if (anim == DotAnimState.anim_mud)
            {
                //챕터를 파악해서, mold를 변경시킬 때 사용.
                dot.Animator.SetInteger("Chapter", dot.Chapter);
            }

            dot.Animator.SetInteger(Animator.StringToHash("DotAnimState"), (int)anim); //애니메이션 업데이트
        }
    }

    //상태를 나갈 때 1회 호출 -> Position -1로 변경
    public override void Exit(DotController dot)
    {
        //나갈 때 위치를 -1로 바꾼다.
        dot.Position = -1;
    }

    //임시 print용
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

    //상태를 시작할 때 1회 호출 -> Position 랜덤으로 선택
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
            dot.Animator.SetInteger(Animator.StringToHash("DotAnimState"), (int)anim); //애니메이션 업데이트
            dot.transform.position = GetCoordinate(dot.Position);
        }
    }

    //상태를 나갈 때 1회 호출 -> Position -1로 변경
    public override void Exit(DotController dot)
    {
        dot.Eyes.SetActive(false);
    }

    //임시 print용
    public override void Read()
    {
    }
}

public class Sub : DotState
{
    [SerializeField]
    Dictionary<DotAnimState, List<float>> SubPos;

    //상태를 시작할 때 1회 호출 -> Position 랜덤으로 선택
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
            //dot의 position이 지정되어있는가 확인한다. -1은 지정되지 않음, n은 지정
            //지정된 경우, IdlePos[animKey][position]을 동작한다(애니메이션 상태전환).
            if (dot.Position == -1)
            {
                int maxIdx = SubPos[anim].Count;

                Debug.Log("맥스 인덱스: " + maxIdx);

                dot.Position = SubPos[anim][UnityEngine.Random.Range(0, maxIdx)];
                Debug.Log("이게 계속 작동되는거 같은데?:" + dot.Position);
            }
            dot.Animator.SetInteger(Animator.StringToHash("DotAnimState"), (int)anim); //애니메이션 업데이트
        }
        dot.transform.position = GetCoordinate(dot.Position); //위치 업데이트
    }

    //상태를 나갈 때 1회 호출 -> Position -1로 변경
    public override void Exit(DotController dot)
    {
    }

    //임시 print용
    public override void Read()
    {
    }
}

public class Phase : DotState
{
    [SerializeField]
    Dictionary<DotAnimState, List<float>> PhasePos;

    //상태를 시작할 때 1회 호출 -> Position 랜덤으로 선택
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
            dot.transform.position = GetCoordinate(dot.Position); //위치 업데이트
            dot.Animator.SetInteger(Animator.StringToHash("DotAnimState"), (int)anim); //애니메이션 업데이트
        }
    }

    //상태를 나갈 때 1회 호출 -> Position -1로 변경
    public override void Exit(DotController dot)
    {
    }

    //임시 print용
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
            dot.transform.position = GetCoordinate(dot.Position); //위치 업데이트
            Debug.Log(dot.transform.position);
            dot.Animator.SetInteger("DotAnimState", (int)anim); //애니메이션 업데이트
        }
    }

    public void GoSleep(DotController dot)
    {
        //잠자러 가는 애니메이션 실행.
        dot.Position = 19;
        dot.transform.position = GetCoordinate(dot.Position); //위치 업데이트
        dot.Animator.SetInteger(Animator.StringToHash("DotAnimState"), (int)DotAnimState.phase_sleep); //애니메이션 업데이트
    }

    //상태를 나갈 때 1회 호출 -> Position -1로 변경
    public override void Exit(DotController dot)
    {
    }

    //임시 print용
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
        Debug.Log("뭉치 컨트롤");
        dot.transform.position = GetCoordinate(dot.Position);
        Debug.Log(dot.Position);
        dot.Animator.SetInteger(Animator.StringToHash("DotAnimState"), (int)DotAnimState.anim_default);
        Debug.Log("튜토 애니" + dot.AnimKey);//애니메이션 업데이트

    }
    public override void Exit(DotController dot)
    {

    }

    public override void Read()
    {

    }
}
