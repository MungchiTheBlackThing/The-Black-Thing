using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script.DialClass;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;


public abstract class MainDialogue : GameState, ILoadingInterface
{
    //대사
    Dictionary<string, int> pos = new Dictionary<string, int>();
    protected GameObject background = null;
    protected DotController dot = null;
    protected LANGUAGE CurrentLanguage = LANGUAGE.KOREAN;
    protected List<DialogueEntry> DialogueEntries = new List<DialogueEntry>();
    public List<object> currentDialogueList = new List<object>();
    public GameObject SystemUI;
    public GameManager manager;
    public int phase = 1;
    MainPanel mainPanel;
    MenuController menuController;
    UITutorial uITutorial;
    PlayerController PlayerController;


    protected int fixedPos = -1;

    protected float prePos;
    protected string preanimkey;
    
    public MainDialogue()
    {
        pos.Add("main_bed", 14);
        pos.Add("main_table", 15);
        pos.Add("main_door_close", 16);
        pos.Add("main_door_open", 16);
        pos.Add("main_web", 17);
    }
    //dot 활성화, 생각 이펙트, 배경 변경, 메뉴 비활성화, 대사 시작, 카메라 고정
    //대사 끝나면 도트 원래대로, 카메라 풀기, 메뉴 활성화, 배경 원래대로, 시스템UI 활성화
    //대사 시작할 때, SystemUI 비활성화
    public override void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
    {
        if (dot)
        {
            dot.gameObject.SetActive(true);
        }
        //n초 뒤에 아래가 뜬다.
        manager.ObjectManager.PlayThinking();
        Debug.Log("메인 부분");
        //실제로는 뭉치가 먼저 뜬다.
        //dot State 변경 -> 클릭 시 아래 두개 고정 및 SetMain 설정.
        this.manager = manager;
        this.dot = dot;
        phase = (int)manager.Pattern;
        dot.TriggerMain(true);
        //dot 한테 chapterList 에서 해당 위치랑 애니메이션이 변함.
        SystemUI = GameObject.Find("SystemUI");
        menuController = GameObject.FindWithTag("Menu").GetComponent<MenuController>();
        PlayerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        CurrentLanguage = PlayerController.GetLanguage(); 
        if (manager.mainVideo)
        {
            Debug.Log("미리 영상 가져오기");
            manager.mainVideo.Setting(manager.Chapter, CurrentLanguage);
        }
    }

    public void LoadData(string[] lines)
    {
        PlayerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        CurrentLanguage = PlayerController.GetLanguage();
        listclear();
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            string[] parts = ParseCSVLine(line);

            if (parts.Length >= 15)
            {
                int main = int.Parse(parts[0]);
                if (main == phase)
                {
                    //Debug.Log("엔트리 시작");
                    DialogueEntry entry = new DialogueEntry
                    {
                        Main = main,
                        ScriptNumber = parts[1],
                        LineKey = int.Parse(parts[2]),
                        Background = parts[3],
                        Actor = parts[4],
                        AnimState = parts[5],
                        DotBody = parts[6],
                        DotExpression = parts[7],
                        TextType = parts[8],
                        KorText = ApplyLineBreaks(parts[9]),
                        EngText = ApplyLineBreaks(parts[10]),
                        NextLineKey = parts[11],
                        AnimScene = parts[12],
                        AfterScript = parts[13],
                        Deathnote = parts[14]
                    };

                    //string displayedText = CurrentLanguage == LANGUAGE.KOREAN ? entry.KorText : entry.EngText;
                    //entry.KorText = displayedText;
                    DialogueEntries.Add(entry);
                    currentDialogueList.Add(entry);


                }
            }
            if (parts.Length >= 16)
            { 
                DialogueEntries[DialogueEntries.Count - 1].LocTable = parts[15].Trim();
            }
            if (parts.Length >= 17)
            { 
                DialogueEntries[DialogueEntries.Count - 1].LocKey = parts[16].Trim();
            }
            else
            {
                Debug.LogError($"Line {i} does not have enough parts: {line}");
            }
        }
    }
    //준현아 여기에 함수 만들어놓을게 파라미터랑 리턴값 등 너가 필요한대로 바꿔
 
    public main GetData(int idx)
    {
        main maindata = new main();
        maindata.ScriptNumber = DialogueEntries[idx].ScriptNumber;
        maindata.LineKey = DialogueEntries[idx].LineKey;
        maindata.Actor = DialogueEntries[idx].Actor;
        maindata.TextType = DialogueEntries[idx].TextType;
        //maindata.Text = DialogueEntries[idx].KorText;
        maindata.Text = GetDisplayText(DialogueEntries[idx]);
        maindata.DeathNote = DialogueEntries[idx].Deathnote;

        //이 Text안에서 <name>이 있을 경우 변경
        maindata.NextLineKey = DialogueEntries[idx].NextLineKey;
        maindata.AnimScene = DialogueEntries[idx].AnimScene;
        fixedPos = pos[DialogueEntries[idx].Background];

        //Debug.Log("테스트: " + fixedPos.ToString());
        //데이터에 대한 애니메이션으로 변경한다., fixedPos 은 건드리지말길!!! 위치 값인데 항상 고정
        
        dot.ChangeState(DotPatternState.Main, DialogueEntries[idx].DotBody, fixedPos, DialogueEntries[idx].DotExpression);
        return maindata; //data[idx].Kor
    }

    public string GetDisplayText(DialogueEntry entry)
    {   
        //LocKey가 있으면 가져오고, 없으면 기존 텍스트 사용
        if (!string.IsNullOrEmpty(entry.LocKey))
        {
            if (string.IsNullOrEmpty(entry.LocTable))
            {
                Debug.LogError($"LocTable이 비어 있음 (LineKey={entry.LineKey})");
            }
            else
            {
                string localizedText = LocalizationSettings.StringDatabase.GetLocalizedString(entry.LocTable, entry.LocKey);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    return ApplyLineBreaks(localizedText);
                }
            }
            
        }
        return CurrentLanguage == LANGUAGE.KOREAN ? entry.KorText : entry.EngText;
    }

    public void StartMain(GameManager manager, string fileName)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        dot = GameObject.Find("Dot").GetComponent<DotController>();
        Debug.Log(dot);
        if (dot)
        {
            dot.gameObject.SetActive(true);
        }
        mainPanel = GameObject.Find("MainDialougue").GetComponent<MainPanel>();
        //델리게이트를 사용해서 옵저버 패턴 구현
        //메인을 시작할때 SystemUI를 끄기 위해서는 아래 주석을 풀어주면 된다.
        //manager.ObjectManager.activeSystemUIDelegate(false);

        //대사를 로드했음 좋겠음.
        //배경화면을 로드한다.
        //카메라를 0,0,10에서 정지시킨다.움직이지 못하게한다.
        if (uITutorial == null && currentSceneName == "MainScene" && manager.Chapter == 1)
            uITutorial = mainPanel.UITutorial.GetComponent<UITutorial>();
        Debug.Log(uITutorial);
        prePos = dot.Position;
        preanimkey = dot.AnimKey;
        TextAsset dialogueData = Resources.Load<TextAsset>("CSV/" + fileName);

        if (dialogueData == null)
        {
            Debug.LogError("Dialogue file not found in Resources folder.");
            return;
        }
        Debug.Log(dialogueData.name);

        string[] lines = dialogueData.text.Split('\n');
        LoadData(lines);
        Debug.Log(DialogueEntries[0].Background);
        fixedPos = pos[DialogueEntries[0].Background]; //현재 배경화면이 어떤 값인지 변경해주길
        //dot.ChangeState(DotPatternState.Main, "body_default1", fixedPos, "face_null");
        mainPanel.Day = manager.Chapter;
        mainPanel.LANGUAGE = CurrentLanguage;
        mainPanel.ShowNextDialogue();
        manager.ScrollManager.StopCamera(true);
        background = manager.ObjectManager.SetMain(DialogueEntries[0].Background); // 현재 배경이 어떤 값인지 변경
        Debug.Log("배경:" + background);

        AudioManager.instance.PlayOneShot(FMODEvents.instance.mainEnter, dot.transform.position);
        //배경화면이 켜질 때, 뭉치의 위치도 고장한다.
        //파라미터로 배경값을 전달하면 된다.
        //Day 7을 제외하곤 모두 배경값을 Enter에서 수정하면 되고, 데이 7일때만 변경해준다.
        if (menuController)
            menuController.alloff();
    }
    public override void Exit(GameManager manager, TutorialManager tutomanger = null)
    {
        Debug.Log("테스트1");
    }

    public void MainEnd()
    {
        Debug.Log("메인 Exit1");
        dot.TriggerMain(false);
        manager.ScrollManager.StopCamera(false);
        if (background)
        {
            Debug.Log("현재 배경:" + background.name);
            background.SetActive(false);
        }
        manager.ObjectManager.activeSystemUIDelegate(true);
        menuController.allon();
        dot.ChangeState(DotPatternState.Default, preanimkey, prePos);
        menuController.tuto();
        if (phase == 1 && manager.Chapter == 1)
        {
            menuController.onlyskipoff();
            uITutorial.gameObject.SetActive(true);
        }
        if (phase == 3 && manager.Chapter == 1)
        {
            menuController.onlyskipoff();
            //Tutorial_9 대사 실행
            GameObject subdial = manager.subDialoguePanel;
            subdial.SetActive(true);
            subdial.GetComponent<SubDialogue>().Tuto_start(106);
        }
        if (phase == 3 && manager.Chapter == 14)
        {
            manager.Ending();
        }
    }

    void listclear()
    {
        DialogueEntries.Clear();
        currentDialogueList.Clear();
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
