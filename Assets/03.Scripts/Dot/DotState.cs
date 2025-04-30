using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DotData
{
    public float dotPosition;
    public float X;
    public float Y;
}

[System.Serializable]
public class Coordinate
{
    public List<DotData> data;
}
[System.Serializable]
public abstract class DotState
{
    static protected Dictionary<float, Vector2> position; //State 클래스 1개에 모두 공유할 수 있도록 함.
    static protected StateReader reader;
    public Vector2 GetCoordinate(float idx) { return position[idx]; }

    public DotState()
    {
        reader = new StateReader();
        position = new Dictionary<float, Vector2>();
        ReadJson();
    }

    void ReadJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("FSM/DotPosition");
        Coordinate dotData = JsonUtility.FromJson<Coordinate>(jsonFile.text);

        // Example usage: Print all dot positions
        foreach (var Data in dotData.data)
        {
            Vector2 vector = new Vector2(Data.X, Data.Y);
            position.Add(Data.dotPosition, vector);
        }
    }

    //상태를 시작할 때 1회 호출 -> Position 랜덤으로 선택
    public abstract void Init(DotAnimState state, List<float> pos); //해당 상태 초기화를 위해서 필요하다.
    public abstract void Enter(DotController dot, bool changeAnim);
    //상태를 나갈 때 1회 호출 -> Position -1로 변경
    public abstract void Exit(DotController dot);

    //임시 데이터 읽기용
    public abstract void Read();
}
