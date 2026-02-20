using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script.DialClass;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class SubDialogue : MonoBehaviour
{
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

    private SubDialogueEntry _lastDisplayedEntry;
    private float prePos;
    private string preanimkey;

    public static bool isSubmoldtutoend = true;
    public bool IsTutorialDialogue { get; private set; } = false;

    static readonly Dictionary<float, float> CameraXByDotPos = new Dictionary<float, float>
    {
        { 0f,  -2.6f },
        { 1f,  -0.5f },
        { 3f,   4.9f },
        { 5f,  -2.0f },
        { 6f,   4.5f },
        { 8f,   4.6f },
        { 9f,   4.6f },
        { 10f,  1.5f },
        { 11f,  4.6f },
    };

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
                if (sub == playerController.GetSubseq()) //****************테스트용으로 1을 넣어놨음**************** (서브 트리거 작동을 아직 모름)
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
                        Exeption = parts[13],
                        LocTable = (parts.Length > 14) ? parts[14]: "",
                        LocKey = (parts.Length > 15) ? parts[15] : ""
                    };

                    //string displayedText = CurrentLanguage == LANGUAGE.KOREAN ? entry.KorText : entry.EngText;
                    //entry.KorText = displayedText;
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
        Debug.Log("현재 서브 길이: " + currentDialogueList.Count);
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

    public void StartSub(string fileName, int index = 0)
    {
        IsTutorialDialogue = (fileName == "tutorial_sub"); // 일반 서브면 false
        InputGuard.WorldInputLocked = true;
        dialogueData = null;
        SubPanel subPanel = this.transform.GetChild(0).GetComponent<SubPanel>();
        if (!SystemUI)
            SystemUI = GameObject.Find("SystemUI");
        dialogueData = Resources.Load<TextAsset>("CSV/" + "sub_script/" + fileName);
        if (dialogueData == null)
        {
            Debug.LogError("Dialogue file not found in Resources folder.");
            return;
        }

        // 새로운 subseq 시작 시 이전 reward 초기화
        //if (playerController != null)
        //{
            //playerController.currentReward = "";
        //}

        if (dot != null)
        {
            prePos = dot.Position;
            preanimkey = dot.AnimKey;
        }
        if (manager.Pattern == GamePatternState.Writing)
        {
            playerController.SetSubseq(3);
        }
        if (manager.Pattern == GamePatternState.Sleeping)
        {
            playerController.SetSubseq(4);
        }

        string[] lines = dialogueData.text.Split('\n');
        LoadSubDialogue(lines);

        if (manager != null && manager.GetType() == typeof(GameManager))
        {
            ScriptList sclist = dot.GetSubScriptList(manager.Pattern);
            Debug.Log("테스트1: " + sclist);
            float dPosition = sclist.DotPosition;
            Debug.Log("테스트2: " + dPosition);
            float camx = SclistGetCameraX(dPosition);
            scroll.MoveCamera(
                new Vector3(camx, 0, -10), 
                1f,
                onComplete: () => {
                    if (index == 0)
                        subPanel.ShowNextDialogue();
                    else
                    {
                        subPanel.dialogueIndex = index;
                        subPanel.ShowNextDialogue();
                    }
                }
            );
        }
        else
        {
            scroll.stopscroll();
            if (index == 0)
                subPanel.ShowNextDialogue();
            else
            {
                subPanel.dialogueIndex = index;
                subPanel.ShowNextDialogue();
            }
        }

        //manager.ScrollManager.StopCamera(true); -> 자꾸 오류 발생함
        if (menuController)
            menuController.alloff();
    }



    public sub GetData(int idx)
    {
        _lastDisplayedEntry = SubDialogueEntries[idx];

        sub subdata = new sub();
        subdata.ScriptNumber = SubDialogueEntries[idx].ScriptNumber;
        subdata.LineKey = SubDialogueEntries[idx].LineKey;
        subdata.Actor = SubDialogueEntries[idx].Actor;
        subdata.TextType = SubDialogueEntries[idx].TextType;
        
        if (string.IsNullOrEmpty(SubDialogueEntries[idx].LocTable))
        {
            Debug.Log("LocTable 비어있음 :" + SubDialogueEntries[idx].KorText);
            subdata.Text = SubDialogueEntries[idx].KorText;
        }
        else
        {
            //Debug.Log("LocTable 있음 :" + GetDisplayText(SubDialogueEntries[idx]));
            subdata.Text = GetDisplayText(SubDialogueEntries[idx]);
        }

        subdata.Color = SubDialogueEntries[idx].Color;
        subdata.DotAnim = SubDialogueEntries[idx].DotAnim;
        //여기서 dot 값을 변경할 예정
        Debug.Log("DotAnim 값: " + subdata.DotAnim);
        //이 Text안에서 <name>이 있을 경우 변경
        subdata.NextLineKey = SubDialogueEntries[idx].NextLineKey;
        //subdata.LocTable = SubDialogueEntries[idx].LocTable;
        //subdata.LocKey = SubDialogueEntries[idx].LocKey;

        subdata.Exeption = SubDialogueEntries[idx].Exeption;

        return subdata;
    }

    public string GetDisplayText(SubDialogueEntry entry)
    {
        string text = string.Empty;
        if (!string.IsNullOrEmpty(entry.LocKey))
        {
            if (string.IsNullOrEmpty(entry.LocTable))
            {
                Debug.LogError($"LocTable이 비어 있음 (LineKey={entry.LineKey})");
            }
            else
            {
                StringTable stringTable = LocalizationSettings.StringDatabase.GetTable(entry.LocTable);
                string localizedText = stringTable.GetEntry(entry.LocKey)?.GetLocalizedString();
                if (!string.IsNullOrEmpty(localizedText))
                    return ApplyLineBreaks(localizedText);  
            }

            text = CurrentLanguage == LANGUAGE.KOREAN ? entry.KorText : entry.EngText;

            // <nickname>을 닉네임으로 교체
            if (text.Contains("<nickname>"))
            {
                string playerName = playerController.GetNickName();
                text = text.Replace("<nickname>", playerName);
            }
        }
        return text;
    }

    public void Subexit()
    {
        InputGuard.WorldInputLocked = false;
        ScreenShield.Off();

        //이벤트가 완전히 종료되는 시점에 타이머 키를 삭제
        if (manager != null && playerController != null)
        {
            // MarkSubWatched/plusSubseq가 호출되기 전의 subseq 번호 사용
            int completedSubseq = playerController.GetSubseq();
            string timestampKey = "PendingEventTimestamp_" + manager.Chapter + "_" + manager.Pattern.ToString() + "_" + completedSubseq;
            PlayerPrefs.DeleteKey(timestampKey);
            PlayerPrefs.Save();
            Debug.Log($"[삭제] 서브 이벤트 타이머 키 삭제 (종료 시점): {timestampKey}");
        }

        if (dialogueData != null && dialogueData.name == "tutorial_sub")
        {
            Debug.Log(dialogueData.name);
            TutoExit();
            manager.NextPhase();
            return;
        }

        GamePatternState gamePattern = manager.Pattern;
        SubPanel = this.transform.GetChild(0).GetComponent<SubPanel>();

        if (menuController)
            menuController.allon();

        scroll.scrollable();
        

        if (SceneManager.GetActiveScene().name != "Tutorial")
        {
            int completedSubseq = playerController.GetSubseq();
            
            // 서브를 본 것으로 표시
            playerController.MarkSubWatched(completedSubseq);
            Debug.Log("이미 본 서브로 저장" + completedSubseq);
            
            // SetSubPhase 호출하여 ProgressUI에 반영
            // phaseIdx는 0-based (0, 1, 2, 3), subseq는 1-based (1, 2, 3, 4)
            int phaseIdx = completedSubseq - 1;
            if (phaseIdx >= 0 && phaseIdx < 4)
            {
                Debug.Log($"[SubDialogue.Subexit] SetSubPhase 호출: subseq={completedSubseq}, phaseIdx={phaseIdx}");
                playerController.SetSubPhase(phaseIdx);
            }
            
            playerController.plusSubseq();
            if (playerController.GetSubseq() > 4)
                playerController.SetSubseq(1);
        }

        if (playerController.GetSubseq() == 2 && manager.Chapter == 1)
        {
            isSubmoldtutoend = false;
            menuController.onlyskipoff();
            subTutorial.gameObject.SetActive(true);
        }

        Debug.Log("끝났을때 서브 번호: " + playerController.GetSubseq());

        // AfterScript가 존재한다면 재생
        bool afterScriptPlayed = false;
        Debug.Log("[SubDialogue] Subexit: AfterScript 실행");
        if (_lastDisplayedEntry != null)
        {
            Debug.Log($"[SubDialogue] 마지막으로 표시된 대사 항목 AfterScript 값: '{_lastDisplayedEntry.AfterScript}'");
            if (!string.IsNullOrEmpty(_lastDisplayedEntry.AfterScript))
            {
                Debug.Log($"[SubDialogue] AfterScript 값 존재, AfterScript 재생: {_lastDisplayedEntry.AfterScript}");
                dot.PlayAfterScript(_lastDisplayedEntry.AfterScript, this.prePos);
                afterScriptPlayed = true;
            }
            else
            {
                Debug.Log("[SubDialogue] AfterScript 값 없음");
            }
        }
        else
        {
            Debug.LogWarning("[SubDialogue] 마지막으로 표시된 대사 항목 찾을 수 없음");
        }

        if (!afterScriptPlayed)
        {
            if (manager.Pattern == GamePatternState.Writing)
            {
                string diaryKey = (dot != null) ? dot.GetDiaryAnimKeyForChapter(dot.Chapter) : "anim_diary";
                Debug.Log($"[SubDialogue] Writing 페이즈, {diaryKey}로 복귀");
                if (dot != null)
                    dot.ChangeState(DotPatternState.Phase, diaryKey, -1, "", true);
            }
            else if (manager.Pattern == GamePatternState.Watching && manager.Chapter == 14)
            {
                // 14일차 Watching: AfterScript 없을 때 anim_mud로 복귀
                Debug.Log("[SubDialogue] 14일차 Watching 페이즈, anim_mud_day13으로 복귀");
                if (dot != null)
                    dot.PlayMudAnimation(14);
            }
            else
            {
                Debug.Log("[SubDialogue] AfterScript가 재생 X, 이전 애니메이션으로 돌아감");
                dot.UpdateIdleAnimation();
            }
        }

        // 다음 서브 이벤트 타이머 시작
        manager.ShowSubDial();
        this.gameObject.SetActive(false);
        if (dot != null)
            dot.RefreshDustState(dot.AnimKey);

        ScreenShield.Off();
        InputGuard.WorldInputLocked = false;
        
    }

    public void TutoExit()
    {
        InputGuard.WorldInputLocked = false;
        ScreenShield.Off();
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
    public void Tuto_start(int index, float delay)
    {
        IsTutorialDialogue = true;
        playerController.SetSubseq(1);
        dialogueData = null;
        SubPanel subPanel = this.transform.GetChild(0).GetComponent<SubPanel>();
        if (!SystemUI)
            SystemUI = GameObject.Find("SystemUI");
        dialogueData = Resources.Load<TextAsset>("CSV/sub_script/" + "tutorial_sub");
        if (dialogueData == null)
        {
            Debug.LogError("Dialogue file not found in Resources folder.");
            return;
        }
        scroll.stopscroll(); //임시 방편
        string[] lines = dialogueData.text.Split('\n');
        LoadSubDialogue(lines);
        subPanel.dialogueIndex = index;
        StartCoroutine(ShowNextDialogueAfterDelay(delay, subPanel));
        //manager.ScrollManager.StopCamera(true); -> 자꾸 오류 발생함
        if (menuController)
            menuController.alloff();
    }
    private IEnumerator ShowNextDialogueAfterDelay(float delay,SubPanel subPanel)
    {
        yield return new WaitForSeconds(delay);
        subPanel.ShowNextDialogue();
    }

    public static float SclistGetCameraX(float pos)
    {
        if (CameraXByDotPos.TryGetValue(pos, out float x))
            return x;

        Debug.LogWarning($"[DotCameraPosTable] unmapped pos: {pos}");
        return 0f;
    }
}