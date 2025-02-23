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
        int preID = 1;
        List<ScriptList> Mtmp = new List<ScriptList>();
        Stmp = new Dictionary<GamePatternState, List<ScriptList>>();

        Stmp[GamePatternState.Watching] = new List<ScriptList>();
        Stmp[GamePatternState.Thinking] = new List<ScriptList>();
        Stmp[GamePatternState.Writing] = new List<ScriptList>();
        Stmp[GamePatternState.Sleeping] = new List<ScriptList>();

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

                // 메인이랑 서브 분류해서 엔트리 넣어주기 OK
                if (entry.GameState == GamePatternState.MainA || entry.GameState == GamePatternState.MainB)
                {
                    // main
                    Mtmp.Add(entry);
                }
                else
                {
                    // sub
                    Stmp[entry.GameState].Add(entry);
                }

                if (entry.ID != preID)
                {
                    InmainStart.Add(Mtmp);
                    InsubStart.Add(Stmp);

                    Mtmp = new List<ScriptList>();
                    Stmp = new Dictionary<GamePatternState, List<ScriptList>>();
                    Stmp[GamePatternState.Watching] = new List<ScriptList>();
                    Stmp[GamePatternState.Thinking] = new List<ScriptList>();
                    Stmp[GamePatternState.Writing] = new List<ScriptList>();
                    Stmp[GamePatternState.Sleeping] = new List<ScriptList>();

                    preID = entry.ID;
                }            
            }
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
