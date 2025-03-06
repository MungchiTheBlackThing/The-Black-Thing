using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Assets.Script.DialClass;
using System;
using UnityEngine.UIElements;
using System.Linq;


public class ScriptListParser
{

    [SerializeField]
    Dictionary<GamePatternState, List<ScriptList>> Stmp;

    public void Load(List<List<ScriptList>> InMainStart, List<Dictionary<GamePatternState, List<ScriptList>>> InSubStart)
    {
        TextAsset dialogueData = Resources.Load<TextAsset>("CSV/ScriptList");

        if (dialogueData == null)
        {
            Debug.LogError("Dialogue file not found in Resources folder.");
            return;
        }
        Debug.Log("Dialogue file loaded successfully.");
        string[] lines = dialogueData.text.Split('\n');
        LoadScriptList(lines, InMainStart, InSubStart);
    }

    public void LoadScriptList(string[] lines, List<List<ScriptList>> InmainStart, List<Dictionary<GamePatternState, List<ScriptList>>> InsubStart)
    {
        int preID = -1; // 초기값을 -1로 설정하여 첫 번째 ID와 무조건 다르게 만듦
        List<ScriptList> Mtmp = new List<ScriptList>();
        Dictionary<GamePatternState, List<ScriptList>> Stmp = new Dictionary<GamePatternState, List<ScriptList>>();

        // Stmp 초기화
        void InitializeStmp()
        {
            Stmp = new Dictionary<GamePatternState, List<ScriptList>>
            {
            { GamePatternState.Watching, new List<ScriptList>() },
            { GamePatternState.Thinking, new List<ScriptList>() },
            { GamePatternState.Writing, new List<ScriptList>() },
            { GamePatternState.Sleeping, new List<ScriptList>() }
            };
        }
        InitializeStmp();

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            string[] parts = ParseCSVLine(line);

            if (parts.Length >= 7)
            {
                ScriptList entry = new ScriptList
                {
                    ID = int.Parse(parts[0]),
                    GameState = (GamePatternState)int.Parse(parts[1]),
                    ScriptKey = parts[2],
                    AnimState = parts[3],
                    DotAnim = parts[4],
                    DotPosition = int.Parse(parts[5]),
                    Delay = int.Parse(parts[6])
                };

                // ID가 바뀌었을 때 기존 데이터 저장 후 초기화
                if (entry.ID != preID && preID != -1)
                {
                    InmainStart.Add(new List<ScriptList>(Mtmp));  // 기존 Mtmp 저장
                    InsubStart.Add(new Dictionary<GamePatternState, List<ScriptList>>(Stmp)); // 기존 Stmp 저장

                    Mtmp.Clear();
                    InitializeStmp(); // Stmp 초기화
                }

                // 메인과 서브 데이터 분류
                if (entry.GameState == GamePatternState.MainA || entry.GameState == GamePatternState.MainB)
                {
                    Mtmp.Add(entry);
                }
                else
                {
                    Stmp[entry.GameState].Add(entry);
                }

                preID = entry.ID; // ID 업데이트
            }
        }

        // 마지막 데이터 저장
        if (Mtmp.Count > 0 || Stmp.Values.Any(list => list.Count > 0))
        {
            InmainStart.Add(Mtmp);
            InsubStart.Add(Stmp);
        }
    }



    string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string value = "";

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(value.Trim());
                value = "";
            }
            else
            {
                value += c;
            }
        }

        if (!string.IsNullOrEmpty(value))
        {
            result.Add(value.Trim());
        }
        return result.ToArray();
    }
    
}
