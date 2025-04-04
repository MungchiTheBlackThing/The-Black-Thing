using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Assets.Script.DialClass;
using Assets.Script.TimeEnum;
using UnityEngine.XR;
using Unity.VisualScripting;
using System;

public class MoonRadioParser
{
    Dictionary<int, Dictionary<int, List<MoonRadioDial>>> MoonRadios;
    LANGUAGE curLanguage = LANGUAGE.KOREAN;

    public MoonRadioParser()
    {
        MoonRadios = new Dictionary<int, Dictionary<int, List<MoonRadioDial>>>();
    }

    public List<MoonRadioDial> GetMoonRadioDial(int chapter, int number, LANGUAGE lan)
    {
        ChangeLanguage(chapter, lan); //바꾼 후 전달
        return MoonRadios[chapter][number];
    }

    public void LoadMoonRadio()
    {
        TextAsset dialogueData = Resources.Load<TextAsset>("CSV/moonradio");

        if (dialogueData == null)
        {
            Debug.LogError("Dialogue file not found in Resources folder.");
            return;
        }

        string[] lines = dialogueData.text.Split('\n');
        LoadMoonRadioDial(lines);
    }

    //Change Korean -> English or English -> Korean
    public void ChangeLanguage(int chapter, LANGUAGE language)
    {
        if(curLanguage != language)
        {
            foreach (var Dial in MoonRadios[chapter])
            {
                foreach(var List in Dial.Value)
                {
                    string displayedText = curLanguage == LANGUAGE.KOREAN ? List.KorText : List.EngText;
                    List.KorText = displayedText;
                }
            }
            curLanguage = language;
        }
    }

    void LoadMoonRadioDial(string[] lines)
    {
        int chapter = 0;
        int number = 0;

        //실제론 [ID][MoonNumber][entry]
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            string[] parts = ParseCSVLine(line);
            //Debug.Log($"Parsed line {i}: {string.Join(", ", parts)}");

            if (parts.Length >= 5)
            {

                int ID = int.Parse(parts[0]);
                int MoonNumber = int.Parse(parts[1]);
                string Actor = parts[2];

                EMoonChacter eMoonChacter;

                if (Enum.TryParse(Actor, true, out eMoonChacter))
                {
                    MoonRadioDial entry = new MoonRadioDial
                    {
                        Actor = eMoonChacter,
                        KorText = ApplyLineBreaks(parts[3]),
                        EngText = ApplyLineBreaks(parts[4]),
                    };

                    if (chapter != ID)
                    {
                        MoonRadios[ID] = new Dictionary<int, List<MoonRadioDial>>();
                        chapter = ID;
                    }

                    if (number != MoonNumber)
                    {
                        //새로운 Dictionary
                        MoonRadios[ID][MoonNumber] = new List<MoonRadioDial>();
                        number = MoonNumber;
                    }

                    MoonRadios[ID][MoonNumber].Add(entry);
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
    string ApplyLineBreaks(string text)
    {
        return text.Replace(@"\n", "\n");
    }
}
