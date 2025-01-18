using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script.DialClass;

public class SubDiaryParser : MonoBehaviour
{
    [SerializeField]
    protected List<SubDiary> SubDiaryEntries = new List<SubDiary>();
    protected LANGUAGE CurrentLanguage = LANGUAGE.KOREAN;

    [SerializeField]
    List<string> texts = new List<string>();
    public bool suc = true;
    // Start is called before the first frame update
    void Start()
    {
        TextAsset dialogueData = Resources.Load<TextAsset>("CSV/SubDiary");
        if (dialogueData == null)
        {
            Debug.LogError("Dialogue file not found in Resources folder.");
            return;
        }
        Debug.Log("Dialogue file loaded successfully.");
        string[] lines = dialogueData.text.Split('\n');
        LoadParse(lines);
    }

    public void LoadParse(string[] lines)
    {
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
                SubDiary entry = new SubDiary
                {
                    ID = int.Parse(parts[0]),
                    ScriptKey = parts[1],
                    SucKorText = parts[2],
                    FailKorText = parts[3],
                    SucEngText = parts[4],
                    FailEngText = parts[5],
                    ArtFilePath = parts[6],
                };
                string successText = CurrentLanguage == LANGUAGE.KOREAN ? entry.SucKorText : entry.SucEngText;
                string failText = CurrentLanguage == LANGUAGE.KOREAN ? entry.FailKorText : entry.FailEngText;

                // 선택된 텍스트를 저장
                entry.SucKorText = successText;
                entry.FailKorText = failText;
                if (suc)
                {
                    texts.Add(successText);
                }
                else
                {
                    texts.Add(failText);
                }

                SubDiaryEntries.Add(entry);

                //Debug.Log($"서브 다이어리: Success = {successText}, Fail = {failText}");
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
