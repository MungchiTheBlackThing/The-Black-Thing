using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script.DialClass;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class SubDialogue : MonoBehaviour
{
    public int subseq = 1;
    Dictionary<string, int> pos = new Dictionary<string, int>();
    protected GameObject background = null;
    public DotController dot = null;
    protected LANGUAGE CurrentLanguage = LANGUAGE.KOREAN;

    [SerializeField]
    protected List<SubDialogueEntry> SubDialogueEntries = new List<SubDialogueEntry>();

    [SerializeField]
    protected ScrollManager scroll;

    public List<object> currentDialogueList = new List<object>();
    public GameObject SystemUI;
    public SubPanel SubPanel;
    public GameManager manager;
    public MenuController menuController;
    public SubTutorial subTutorial;
    public bool check1 = false;
    public PlayerController playerController;

    [SerializeField]
    TextAsset dialogueData;


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

            if (parts.Length >= 13)
            {
                int sub = int.Parse(parts[0]);
                if (sub == subseq) //****************테스트용으로 1을 넣어놨음**************** (서브 트리거 작동을 아직 모름)
                {
                    SubDialogueEntry entry = new SubDialogueEntry
                    {
                        Sub = sub,
                        ScriptNumber = parts[1],
                        LineKey = int.Parse(parts[2]),
                        Color = int.Parse(parts[3]),
                        Actor = parts[4],
                        AnimState = parts[5],
                        DotAnim = parts[6],
                        TextType = parts[7],
                        KorText = ApplyLineBreaks(parts[8]),
                        EngText = ApplyLineBreaks(parts[9]),
                        NextLineKey = parts[10],
                        Deathnote = parts[11],
                        AfterScript = parts[12],
                        Exeption = parts[13]
                    };

                    string displayedText = CurrentLanguage == LANGUAGE.KOREAN ? entry.KorText : entry.EngText;
                    entry.KorText = displayedText;
                    SubDialogueEntries.Add(entry);
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
        SubDialogueEntries.Clear();
        currentDialogueList.Clear();
    }

    public void StartSub(string fileName)
    {
        dialogueData = null;
        SubPanel subPanel = this.transform.GetChild(0).GetComponent<SubPanel>();
        if (!SystemUI)
            SystemUI = GameObject.Find("SystemUI");
        dialogueData = Resources.Load<TextAsset>("CSV/" + fileName);
        if (dialogueData == null)
        {
            Debug.LogError("Dialogue file not found in Resources folder.");
            return;
        }
        if (manager.Pattern == GamePatternState.Writing)
        {
            subseq = 3;
        }
        if (manager.Pattern == GamePatternState.Sleeping)
        {
            subseq = 4;
        }
        scroll.stopscroll(); //임시 방편
        string[] lines = dialogueData.text.Split('\n');
        LoadSubDialogue(lines);
        
        subPanel.ShowNextDialogue();
        //manager.ScrollManager.StopCamera(true); -> 자꾸 오류 발생함
        if (menuController)
            menuController.alloff();
        
    }

    public sub GetData(int idx)
    {
        sub subdata = new sub();
        subdata.ScriptNumber = SubDialogueEntries[idx].ScriptNumber;
        subdata.LineKey = SubDialogueEntries[idx].LineKey;
        subdata.Actor = SubDialogueEntries[idx].Actor;
        subdata.TextType = SubDialogueEntries[idx].TextType;
        subdata.Text = SubDialogueEntries[idx].KorText;
        subdata.Color = SubDialogueEntries[idx].Color;
        subdata.DotAnim = SubDialogueEntries[idx].DotAnim;
        //여기서 dot 값을 변경할 예정
        Debug.Log("SubDialogueEntries[idx].DotAnim 여기 변경변경");
        //이 Text안에서 <name>이 있을 경우 변경
        subdata.NextLineKey = SubDialogueEntries[idx].NextLineKey;

        return subdata;
    }

    public void Subexit()
    {
        if (dialogueData.name == "tutorial_sub")
        {
            Debug.Log(dialogueData.name);
            TutoExit();
            manager.NextPhase();
            return;
        }
        GamePatternState gamePattern = manager.Pattern;
        SubPanel = this.transform.GetChild(0).GetComponent<SubPanel>();
        if (menuController)
        {
            menuController.allon();
        }

        scroll.scrollable();
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName != "Tutorial")
            subseq += 1;

        if (subseq>4)
        {
            subseq = 1;
        }
        
        if (subseq == 2 && manager.Chapter == 1)
        {
            menuController.onlyskipoff();
            subTutorial.gameObject.SetActive(true);
        }
        
        Debug.Log("끝났을때 서브 번호: " + subseq);
        manager.CurrentState.RunSubScript(dot, manager);
    }
    public void TutoExit()
    {
        if (menuController)
        {
            menuController.allon();
        }
        SubPanel = this.transform.GetChild(0).GetComponent<SubPanel>();
        scroll.scrollable();
        SubPanel.clear();
    }

    public void SubContinue()
    {
        if (menuController)
        {
            menuController.alloff();
        }
        Debug.Log("이어서 하기");
        scroll.stopscroll(); //임시 방편
        SubPanel.gameObject.SetActive(true);
        SubPanel.Subcontinue();
    }
    public void Tuto_start(int index)
    {
        dialogueData = null;
        SubPanel subPanel = this.transform.GetChild(0).GetComponent<SubPanel>();
        if (!SystemUI)
            SystemUI = GameObject.Find("SystemUI");
        dialogueData = Resources.Load<TextAsset>("CSV/" + "tutorial_sub");
        if (dialogueData == null)
        {
            Debug.LogError("Dialogue file not found in Resources folder.");
            return;
        }
        scroll.stopscroll(); //임시 방편
        string[] lines = dialogueData.text.Split('\n');
        LoadSubDialogue(lines);
        subPanel.dialogueIndex = index;
        StartCoroutine(ShowNextDialogueAfterDelay(2.0f, subPanel));
        //manager.ScrollManager.StopCamera(true); -> 자꾸 오류 발생함
        if (menuController)
            menuController.alloff();
    }
    private IEnumerator ShowNextDialogueAfterDelay(float delay,SubPanel subPanel)
    {
        yield return new WaitForSeconds(delay);
        subPanel.ShowNextDialogue();
    }
}