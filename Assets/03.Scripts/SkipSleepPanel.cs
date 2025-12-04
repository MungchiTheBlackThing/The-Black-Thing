using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script.DialClass;
using UnityEngine.UI;
using TMPro;
using System;
public class SkipSleepPanel : MonoBehaviour
{
    private int chapter = 1;
    protected LANGUAGE CurrentLanguage = LANGUAGE.KOREAN;

    [SerializeField]
    protected List<SkipSleep> skipSleeps = new List<SkipSleep>();

    [SerializeField]
    protected TextMeshProUGUI red;
    [SerializeField]
    protected TextMeshProUGUI sircello;
    [SerializeField]
    protected TextMeshProUGUI edison;
    [SerializeField]
    protected GameObject SkipClick;
    [SerializeField]
    int dialogueindex = 0;

    public List<object> currentDialogueList = new List<object>();
    public GameManager manager;
    public PlayerController playerController;
    


    public void OnEnable()
    {
        dialogueindex = 0;
        TextAsset dialogueData = Resources.Load<TextAsset>("CSV/" + "skip_sleeping");
        if (dialogueData == null)
        {
            Debug.LogError("Dialogue file not found in Resources folder.");
            return;
        }
        chapter = playerController.GetChapter();
        string[] lines = dialogueData.text.Split('\n');
        LoadSubDialogue(lines);
        ShowNext();
    }
    public void LoadSubDialogue(string[] lines)
    {
        CurrentLanguage = playerController.GetLanguage();
        listclear();
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            string[] parts = ParseCSVLine(line);
            //Debug.Log($"Parsed line {i}: {string.Join(", ", parts)}");

            if (parts.Length >= 4)
            {
                int id = int.Parse(parts[0]);
                if (id == chapter)
                {
                    SkipSleep entry = new SkipSleep
                    {
                        ID = id,
                        Actor = parts[1],
                        KorText = ApplyLineBreaks(parts[2]),
                        EngText = ApplyLineBreaks(parts[3])
                    };

                    string displayedText = CurrentLanguage == LANGUAGE.KOREAN ? entry.KorText : entry.EngText;
                    entry.KorText = displayedText;
                    skipSleeps.Add(entry);
                    currentDialogueList.Add(entry);

                    //Debug.Log($"Added SubDialogueEntry: {displayedText}");
                }
            }
            else
            {
                Debug.LogError($"Line {i} does not have enough parts: {line}");
            }
        }
        Debug.Log("현재 인덱스 숫자: " + currentDialogueList.Count);
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
    public void listclear()
    {
        skipSleeps.Clear();
        currentDialogueList.Clear();
    }
    public void ShowNext()
    {
        actoroff();
        switch (skipSleeps[dialogueindex].Actor)
        {
            case "red":
                red.gameObject.SetActive(true);
                red.text = skipSleeps[dialogueindex].KorText;
                break;


            case "edison":
                edison.gameObject.SetActive(true);
                edison.text = skipSleeps[dialogueindex].KorText;
                break;


            case "sircello":
                sircello.gameObject.SetActive(true);
                sircello.text = skipSleeps[dialogueindex].KorText;
                break;
        }

    }
    public void dialend()
    {
        actoroff();
    }
    public void actoroff()
    {
        red.gameObject.SetActive(false);
        sircello.gameObject.SetActive(false);
        edison.gameObject.SetActive(false);
    }
    public void next()
    {
        dialogueindex++;
        if (dialogueindex >= currentDialogueList.Count)
        {
            dialend();
            return;
        }
        ShowNext();
    }
}
