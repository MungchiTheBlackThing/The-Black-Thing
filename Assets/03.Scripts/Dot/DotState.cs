using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DotData
{
    public float dotPosition;
    public int X;
    public int Y;
}

[System.Serializable]
public class Coordinate
{
    public List<DotData> data;
}
public abstract class DotState
{
    static protected Dictionary<float, Vector2> position; //State Ŭ���� 1���� ��� ������ �� �ֵ��� ��.
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
            Debug.Log($"Dot Position: {Data.dotPosition}, X: {Data.X}, Y: {Data.Y}");
        }
    }

    //���¸� ������ �� 1ȸ ȣ�� -> Position �������� ����
    public abstract void Init(DotAnimState state, List<float> pos); //�ش� ���� �ʱ�ȭ�� ���ؼ� �ʿ��ϴ�.
    public abstract void Enter(DotController dot);
    //���¸� ���� �� 1ȸ ȣ�� -> Position -1�� ����
    public abstract void Exit(DotController dot);

    //�ӽ� ������ �б��
    public abstract void Read();
}
