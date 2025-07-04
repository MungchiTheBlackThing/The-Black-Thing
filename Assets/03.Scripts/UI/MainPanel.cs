using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Assets.Script.DialClass;
using UnityEngine.EventSystems;

public class MainPanel : MonoBehaviour
{
    //게임매니저
    [SerializeField]
    GameManager gameManager;
    [SerializeField] 
    PlayerController pc;
    
    MainDialogue mainDialogue;
    [SerializeField] TextMeshProUGUI DotTextUI;
    [SerializeField] TextMeshProUGUI PlayTextUI;
    [SerializeField] TextMeshProUGUI InputTextUI;
    [SerializeField] GameObject DotPanel;
    [SerializeField] GameObject PlayPanel;
    [SerializeField] GameObject InputPanel;
    [SerializeField] GameObject SelectionPanel;
    [SerializeField] GameObject Checkbox3Panel;
    [SerializeField] GameObject Checkbox4Panel;
    [SerializeField] GameObject Selection3Panel;
    [SerializeField] GameObject Selection4Panel;
    [SerializeField] Button NextButton;
    [SerializeField] private TMP_InputField Textinput;
    [SerializeField] GameObject MainClick;
    [SerializeField] GameObject BackBut;
    [SerializeField] public GameObject UITutorial;
    [SerializeField] int backindex = -1;
    [SerializeField] string backtag = "";

    public int dialogueIndex = 0;  // Current dialogue index
    public int Day = 0;  // Current day
    public LANGUAGE LANGUAGE;

    // Start is called before the first frame update
    void OnEnable()
    {
        //게임매니저 게임패턴
        mainDialogue = (MainDialogue)gameManager.CurrentState;
        pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        MainClick = GameObject.Find("MainClick");
    }


    public void InitializePanels()
    {
        DotPanel = Instantiate(Resources.Load("DialBalloon/DotBalloon") as GameObject, transform);
        DotTextUI = DotPanel.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        DotPanel.SetActive(false);
        DotPanel.AddComponent<CanvasGroup>();

        PlayPanel = Instantiate(Resources.Load("DialBalloon/PlayerOneLineBallum") as GameObject, transform);
        PlayTextUI = PlayPanel.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        PlayPanel.SetActive(false);
        PlayPanel.AddComponent<CanvasGroup>();

        InputPanel = Instantiate(Resources.Load("DialBalloon/InputPlayerOpinion") as GameObject, transform);
        InputTextUI = InputPanel.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        InputPanel.SetActive(false);
        InputPanel.AddComponent<CanvasGroup>();

        Checkbox3Panel = Instantiate(Resources.Load("DialBalloon/CheckBox3Selection") as GameObject, transform);
        Checkbox3Panel.SetActive(false);
        Checkbox3Panel.AddComponent<CanvasGroup>();

        Checkbox4Panel = Instantiate(Resources.Load("DialBalloon/CheckBox4Selection") as GameObject, transform);
        Checkbox4Panel.SetActive(false);
        Checkbox4Panel.AddComponent<CanvasGroup>();

        SelectionPanel = Instantiate(Resources.Load("DialBalloon/TwoSelectionBallum") as GameObject, transform);
        SelectionPanel.SetActive(false);
        SelectionPanel.AddComponent<CanvasGroup>();

        Selection3Panel = Instantiate(Resources.Load("DialBalloon/Selection3Selection") as GameObject, transform);
        Selection3Panel.SetActive(false);
        Selection3Panel.AddComponent<CanvasGroup>();

        Selection4Panel = Instantiate(Resources.Load("DialBalloon/Selection4Selection") as GameObject, transform);
        Selection4Panel.SetActive(false);
        Selection4Panel.AddComponent<CanvasGroup>();

       

        if (MainClick != null && BackBut != null && BackBut.transform.parent == transform)
        {
            BackBut.transform.SetSiblingIndex(transform.childCount - 1);
            MainClick.transform.SetSiblingIndex(transform.childCount - 2);
        }
        else
        {
            MainClick.transform.SetSiblingIndex(transform.childCount - 1);
        }
    }
    void ShowSelection(string options)
    {
        string[] selections = options.Split('|');
        for (int i = 0; i < selections.Length; i++)
        {
            Button button = SelectionPanel.transform.GetChild(i).GetComponent<Button>();
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = selections[i];
            int index = i;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnSelectionClicked(index));
        }
    }

    void ShowCheckboxOptions(GameObject checkboxPanel, string options)
    {
        string[] selections = options.Split('|');
        for (int i = 0; i < selections.Length; i++)
        {
            TextMeshProUGUI text = checkboxPanel.transform.GetChild(2).GetChild(0).GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
            text.text = selections[i];
        }
    }

    void ShowSelectionOptions(GameObject checkboxPanel, string options)
    {
        string[] selections = options.Split('|');
        for (int i = 0; i < selections.Length; i++)
        {
            TextMeshProUGUI text = checkboxPanel.transform.GetChild(2).GetChild(0).GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
            text.text = selections[i];
        }
    }

    public void OnSelectionClicked(int index)
    {
        var currentEntry = mainDialogue.GetData(dialogueIndex);
        
        if (currentEntry.NextLineKey != null)
        {
            string[] nextKeys = currentEntry.NextLineKey.Split('|');
            Debug.Log(currentEntry.DeathNote);
            //여기서 sun, moon, active, passive 체크해서 올리기
            if (currentEntry.DeathNote != "")
            {
                string[] archeTags = currentEntry.DeathNote.Split('|'); // 예: "sun|moon", "moon|sun", "active|passive", "passive|active" 등

                if (archeTags.Length == 2) // 항상 두 개의 값이 존재해야 함
                {
                    string firstTag = archeTags[0].Trim().ToLower();
                    string secondTag = archeTags[1].Trim().ToLower();

                    if (index == 0)
                    {
                        Debug.Log(firstTag);
                        backtag = firstTag;
                        pc.UpdateArcheType(firstTag);
                    }
                    else if (index == 1)
                    {
                        Debug.Log(secondTag);
                        backtag= secondTag;
                        pc.UpdateArcheType(secondTag);
                    }
                }

                else if (archeTags.Length == 4)
                {
                    pc.checkdeath(index);
                }
            }
            else
            {
                backtag = "";
            }

            if (index < nextKeys.Length && int.TryParse(nextKeys[index], out int nextLineKey))
            {
                int nextIndex = mainDialogue.currentDialogueList.FindIndex(entry => (entry as DialogueEntry)?.LineKey == nextLineKey);

                if (nextIndex != -1)
                {
                    backindex = dialogueIndex;
                    dialogueIndex = nextIndex;
                }
                else
                {
                    Debug.Log("Next LineKey not found in dialogue list. Ending dialogue.");
                    DialEnd();
                    return;
                }
            }
            else
            {
                Debug.Log("Invalid NextLineKey index or parse failure. Ending dialogue.");
                DialEnd();
                return;
            }
        }
        else
        {
            Debug.Log("Current entry is null. Ending dialogue.");
            DialEnd();
            return;
        }

        SelectionPanel.SetActive(false);
        Selection3Panel.SetActive(false);
        Selection4Panel.SetActive(false);
        Debug.Log("돌아갈수 있는 번호: " + backindex);
        ShowNextDialogue();
    }
    
    public void DialEnd()
    {
        Debug.Log("메인 끝");
        mainDialogue.currentDialogueList.Clear();
        dialogueIndex = 0;
        backindex = -1;
        Debug.Log(gameManager.GetComponent<TutorialManager>());
        if (gameManager.GetComponent<TutorialManager>() != null)
        {
            PanelOff();
            TutorialManager.Instance.ChangeGameState(TutorialState.Sub);
        }
        else
        {
            PanelOff();
            Debug.Log("버튼 끄기");
            BackBut.SetActive(false);
            mainDialogue.MainEnd();
        }
    }
    void PanelOff()
    {
        GameObject[] panels = { DotPanel, PlayPanel, SelectionPanel, InputPanel, Checkbox3Panel, Checkbox4Panel, Selection3Panel, Selection4Panel };
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
        if (MainClick)
            MainClick.SetActive(false);
    }

    public void ShowNextDialogue()
    {
        PanelOff();
        if (dialogueIndex >= mainDialogue.currentDialogueList.Count)
        {
            DialEnd();
            return;
        }

        main mainDial = mainDialogue.GetData(dialogueIndex);

        string textType = mainDial.TextType;
        string actor = mainDial.Actor;
        string korText = mainDial.Text;
        string animScene = mainDial.AnimScene;

        switch (textType)
        {
            case "text":
                if (actor == "Dot")
                {
                    MainClick.SetActive(true);
                    if (korText.Contains("<nickname>"))
                    {
                        if(pc)
                        {
                            korText = korText.Replace("<nickname>", pc.GetNickName());
                        }
                    }
                    if (animScene == "1")
                    {
                        Debug.Log("영상 시작");
                        gameManager.mainVideo.PlayVideo();
                    }
                    DotPanel.SetActive(true);
                    DotTextUI.text = $"{korText}";
                    StartCoroutine(FadeIn(DotPanel.GetComponent<CanvasGroup>(), 0.5f, MainClick.GetComponent<Button>()));
                    RegisterNextButton(MainClick.GetComponent<Button>());
                }
                else if (actor == "Player")
                {
                    MainClick.SetActive(true);
                    if (animScene == "1")
                    {
                        Debug.Log("영상 시작");
                        gameManager.mainVideo.PlayVideo();
                    }
                    PlayPanel.SetActive(true);
                    PlayTextUI.text = $"{korText}";
                    StartCoroutine(FadeIn(PlayPanel.GetComponent<CanvasGroup>(), 0.5f, PlayPanel.transform.GetChild(0).GetComponent<Button>()));
                    RegisterNextButton(MainClick.GetComponent<Button>());
                }
                break;
            case "selection":
                if (animScene == "1")
                {
                    Debug.Log("영상 시작");
                    gameManager.mainVideo.PlayVideo();
                }
                SelectionPanel.SetActive(true);
                StartCoroutine(FadeIn(SelectionPanel.GetComponent<CanvasGroup>(), 0.5f, SelectionPanel.transform.GetComponentInChildren<Button>()));
                ShowSelection(korText);
                break;
            case "textbox":
                if (animScene == "1")
                {
                    Debug.Log("영상 시작");
                    gameManager.mainVideo.PlayVideo();
                }
                InputPanel.SetActive(true);
                InputTextUI.text = korText;
                Resetinputfield(InputPanel);
                StartCoroutine(FadeIn(InputPanel.GetComponent<CanvasGroup>(), 0.5f, InputPanel.transform.GetChild(1).GetComponent<Button>()));
                RegisterNextButton(InputPanel.transform.GetChild(1).GetComponent<Button>());
                break;
            case "checkbox3":
                if (animScene == "1")
                {
                    Debug.Log("영상 시작");
                    gameManager.mainVideo.PlayVideo();
                }
                Checkbox3Panel.SetActive(true);
                ShowCheckboxOptions(Checkbox3Panel, korText);
                StartCoroutine(FadeIn(Checkbox3Panel.GetComponent<CanvasGroup>(), 0.5f, Checkbox3Panel.transform.GetChild(1).GetComponent<Button>()));
                RegisterNextButton(Checkbox3Panel.transform.GetChild(1).GetComponent<Button>());
                break;
            case "checkbox4":
                if (animScene == "1")
                {
                    Debug.Log("영상 시작");
                    gameManager.mainVideo.PlayVideo();
                }
                Checkbox4Panel.SetActive(true);
                ShowCheckboxOptions(Checkbox4Panel, korText);
                StartCoroutine(FadeIn(Checkbox4Panel.GetComponent<CanvasGroup>(), 0.5f, Checkbox4Panel.transform.GetChild(1).GetComponent<Button>()));
                RegisterNextButton(Checkbox4Panel.transform.GetChild(1).GetComponent<Button>());
                break;

            case "selection3":
                if (animScene == "1")
                {
                    Debug.Log("영상 시작");
                    gameManager.mainVideo.PlayVideo();
                }
                Selection3Panel.SetActive(true);
                ShowSelectionOptions(Selection3Panel, korText);
                StartCoroutine(FadeIn(Selection3Panel.GetComponent<CanvasGroup>(), 0.5f, Selection3Panel.transform.GetChild(1).GetComponent<Button>()));
                break;

            case "selection4":
                if (animScene == "1")
                {
                    Debug.Log("영상 시작");
                    gameManager.mainVideo.PlayVideo();
                }
                Selection4Panel.SetActive(true);
                ShowSelectionOptions(Selection4Panel, korText);
                StartCoroutine(FadeIn(Selection4Panel.GetComponent<CanvasGroup>(), 0.5f, Selection4Panel.transform.GetChild(1).GetComponent<Button>()));
                break;
        }
    }
    IEnumerator FadeIn(CanvasGroup canvasGroup, float duration, Button button)
    {
        float counter = 0f;
        button.interactable = false;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, counter / duration);
            yield return null;
        }
        canvasGroup.alpha = 1;
        button.interactable = true;
    }

    void RegisterNextButton(Button button)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(NextDialogue);
    }

    void NextDialogue()
    {
        var currentEntry = mainDialogue.GetData(dialogueIndex);
        if (currentEntry.NextLineKey != null)
        {
            if (int.TryParse(currentEntry.NextLineKey, out int nextLineKey))
            {
                int nextIndex = mainDialogue.currentDialogueList.FindIndex(entry => (entry as DialogueEntry)?.LineKey == nextLineKey);

                if (nextIndex != -1)
                {
                    dialogueIndex = nextIndex;
                }
                else
                {
                    DialEnd();
                    return;
                }
            }
            else
            {
                Debug.Log("NextLineKey is not a valid integer. Moving to the next entry by index.");
                DialEnd();
                return;
            }
        }
        else
        {
            Debug.Log("Current entry is null. Ending dialogue.");
            DialEnd();
            return;
        }
        AudioManager.instance.PlayOneShot(FMODEvents.instance.dialougueDefault, this.transform.position);
        ShowNextDialogue();
    }

    public void Resetinputfield(GameObject field)
    {
        TextMeshProUGUI inputfield = field.transform.GetChild(2).GetChild(1).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        Textinput = field.transform.GetChild(2).GetChild(1).GetComponent<TMP_InputField>();
        Textinput.text = "";
        inputfield.text = "";
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Maincontinue()
    {
        if (backindex != -1)
        {
            // 아직 backindex에 도달하지 않았다면
            if (dialogueIndex > backindex)
            {
                dialogueIndex--; // 한 칸 뒤로 가기
                ShowNextDialogue();
            }
            // 딱 backindex에 도달했을 때 동작
            else if (dialogueIndex == backindex)
            {
                if (!string.IsNullOrEmpty(backtag))
                {
                    pc.DownArcheType(backtag);
                }
                ShowNextDialogue();
            }
        }
    }
}
