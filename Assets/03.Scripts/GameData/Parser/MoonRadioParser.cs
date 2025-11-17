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
        curLanguage = lan;
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

    public void ChangeLanguage(int chapter, LANGUAGE language)
    {
        curLanguage = language;
    }

    void LoadMoonRadioDial(string[] lines)
    {
        int chapter = 0;
        int number = 0;
        int prevChapter = -1;
        int prevMoonNumber = -1;
        int lineIndex = 0;

        //실제론 [ID][MoonNumber][entry]
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            string[] parts = ParseCSVLine(line);

            if (parts.Length >= 5)
            {
                int ID = int.Parse(parts[0]);
                int MoonNumber = int.Parse(parts[1]);
                string Actor = parts[2];

                string textKor = ApplyLineBreaks(parts[3]); 
                string textEng = ApplyLineBreaks(parts[4]);

                string sfx = parts.Length >= 6 ? parts[5] : string.Empty;

                EMoonChacter eMoonChacter;

                if (Enum.TryParse(Actor, true, out eMoonChacter))
                {
                    if (ID != prevChapter || MoonNumber != prevMoonNumber)
                    {
                        lineIndex = 1;
                        prevChapter = ID;
                        prevMoonNumber = MoonNumber;
                    }
                    else
                    {
                        lineIndex++;
                    }
                    //MoonRadioText 로컬라이제이션 테이블 키 생성
                    string key = $"MR{ID:D2}{MoonNumber:D2}_L{lineIndex:D3}";

                    MoonRadioDial entry = new MoonRadioDial
                    {
                        Actor = eMoonChacter,
                        TextKey = key,
                        Sfx = sfx
                    };

                    if (chapter != ID)
                    {
                        MoonRadios[ID] = new Dictionary<int, List<MoonRadioDial>>();
                        chapter = ID;
                    }

                    if (number != MoonNumber)
                    {
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
