using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script.DialClass;
using UnityEngine.UI;
using TMPro;


public abstract class MainDialogue : GameState, ILoadingInterface
{
    //���
    Dictionary<string, int> pos = new Dictionary<string, int>();
    protected GameObject background = null;
    protected DotController dot = null;
    protected LANGUAGE CurrentLanguage = LANGUAGE.KOREAN;
    protected List<DialogueEntry> DialogueEntries = new List<DialogueEntry>();
    public List<object> currentDialogueList = new List<object>();
    GameManager manager;
    MainPanel mainPanel;

    protected int fixedPos = -1;

    public MainDialogue()
    {
        pos.Add("main_bed", 14);
        pos.Add("main_table", 15);
        pos.Add("main_door_close", 16);
        pos.Add("main_door_open", 16);
        pos.Add("main_web", 17);
    }

    public override void Enter(GameManager manager, DotController dot = null)
    {
        if (dot)
        {
            dot.gameObject.SetActive(true);
        }
        manager.ObjectManager.PlayThinking();
        //�����δ� ��ġ�� ���� ���.
        //dot State ���� -> Ŭ�� �� �Ʒ� �ΰ� ���� �� SetMain ����.
        this.manager = manager;
        this.dot = dot;
        dot.TriggerMain(true);
        dot.ChangeState(DotPatternState.Defualt, "anim_default");
    }

    public void LoadData(string[] lines)
    {
        listclear();
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            string[] parts = ParseCSVLine(line);
            Debug.Log($"Parsed line {i}: {string.Join(", ", parts)}");

            if (parts.Length >= 15)
            {
                int main = int.Parse(parts[0]);
                if (main == (int)manager.Pattern)
                {
                    DialogueEntry entry = new DialogueEntry
                    {
                        Main = main,
                        ScriptKey = int.Parse(parts[1]),
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

                    string displayedText = CurrentLanguage == LANGUAGE.KOREAN ? entry.KorText : entry.EngText;
                    entry.KorText = displayedText;
                    DialogueEntries.Add(entry);
                    currentDialogueList.Add(entry);

                    Debug.Log($"Added DialogueEntry: {displayedText}");
                }
            }
            else
            {
                Debug.LogError($"Line {i} does not have enough parts: {line}");
            }
        }
    }
    //������ ���⿡ �Լ� ���������� �Ķ���Ͷ� ���ϰ� �� �ʰ� �ʿ��Ѵ�� �ٲ�
 
    public main GetData(int idx)
    {
        main maindata = new main();

        maindata.LineKey = DialogueEntries[idx].LineKey;
        maindata.Actor = DialogueEntries[idx].Actor;
        maindata.TextType = DialogueEntries[idx].TextType;
        maindata.Text = DialogueEntries[idx].KorText;
        maindata.NextLineKey = DialogueEntries[idx].NextLineKey;

        //�����Ϳ� ���� �ִϸ��̼����� �����Ѵ�., fixedPos �� �ǵ帮������!!! ��ġ ���ε� �׻� ����
        dot.ChangeState(DotPatternState.Main, DialogueEntries[idx].AnimState, fixedPos, DialogueEntries[idx].DotExpression);
        return maindata; //data[idx].Kor
    }

    public void StartMain(GameManager manager, string fileName)
    {
        mainPanel = GameObject.Find("MainDialougue").GetComponent<MainPanel>();
        fixedPos = pos["main_door_open"]; //���� ���ȭ���� � ������ �������ֱ�
        dot.ChangeState(DotPatternState.Main, "body_default1", fixedPos, "face_null");
        //��������Ʈ�� ����ؼ� ������ ���� ����
        //������ �����Ҷ� SystemUI�� ���� ���ؼ��� �Ʒ� �ּ��� Ǯ���ָ� �ȴ�.
        //manager.ObjectManager.activeSystemUIDelegate(false);

        //��縦 �ε����� ������.
        //���ȭ���� �ε��Ѵ�.
        //ī�޶� 0,0,10���� ������Ų��.�������� ���ϰ��Ѵ�.
        TextAsset dialogueData = Resources.Load<TextAsset>("CSV/" + fileName);

        if (dialogueData == null)
        {
            Debug.LogError("Dialogue file not found in Resources folder.");
            return;
        }

        Debug.Log(fileName);
        string[] lines = dialogueData.text.Split('\n');
        LoadData(lines);
        mainPanel.ShowNextDialogue();
        manager.ScrollManager.StopCamera(true);
        background = manager.ObjectManager.SetMain("main_door_open"); // ���� ����� � ������ ����
        //���ȭ���� ���� ��, ��ġ�� ��ġ�� �����Ѵ�.
        //�Ķ���ͷ� ��氪�� �����ϸ� �ȴ�.
        //Day 7�� �����ϰ� ��� ��氪�� Enter���� �����ϸ� �ǰ�, ���� 7�϶��� �������ش�.

    }
    public override void Exit(GameManager manager)
    {
        dot.TriggerMain(false);
        manager.ScrollManager.StopCamera(false);
        if (background)
        {
            background.SetActive(false);
        }

        manager.ObjectManager.activeSystemUIDelegate(true);
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
