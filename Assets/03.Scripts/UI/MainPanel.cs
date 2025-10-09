using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Assets.Script.DialClass;
using UnityEngine.EventSystems;

public class MainPanel : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] PlayerController pc;

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
    [SerializeField] TMP_InputField Textinput;

    [SerializeField] GameObject MainClick;
    [SerializeField] GameObject BackBut;
    [SerializeField] public GameObject UITutorial;

    [SerializeField] int backindex = -1;
    [SerializeField] string backtag = "";

    public int dialogueIndex = 0;
    public int Day = 0;
    public LANGUAGE LANGUAGE;

    void OnEnable()
    {
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
            if (MainClick) MainClick.transform.SetSiblingIndex(transform.childCount - 1);
        }
    }

    void ShowSelection(string options)
    {
        var selections = options.Split('|');
        var buttons = SelectionPanel.GetComponentsInChildren<Button>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            var btn = buttons[i];
            bool has = i < selections.Length;

            btn.gameObject.SetActive(has);
            btn.onClick.RemoveAllListeners();
            if (!has) continue;

            var text = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            if (text) text.text = selections[i];

            int index = i;
            btn.onClick.AddListener(() => OnSelectionClicked(index));
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
        Debug.Log($"선택 클릭됨: {index}");
        var currentEntry = mainDialogue.GetData(dialogueIndex);
        Debug.Log("currentEntry.NextLineKey: " + currentEntry.NextLineKey);

        if (!string.IsNullOrEmpty(currentEntry.NextLineKey))
        {
            string[] nextKeys = currentEntry.NextLineKey.Split('|');

            if (!string.IsNullOrEmpty(currentEntry.DeathNote))
            {
                string[] archeTags = currentEntry.DeathNote.Split('|');

                if (archeTags.Length == 2)
                {
                    string firstTag = archeTags[0].Trim().ToLower();
                    string secondTag = archeTags[1].Trim().ToLower();

                    if (index == 0) { backtag = firstTag; pc.UpdateArcheType(firstTag); }
                    else if (index == 1) { backtag = secondTag; pc.UpdateArcheType(secondTag); }
                }
                else if (archeTags.Length == 4)
                {
                    pc.checkdeath(index);
                }
                else backtag = "";
            }
            else backtag = "";

            if (index < nextKeys.Length && int.TryParse(nextKeys[index], out int nextLineKey))
            {
                int nextIndex = mainDialogue.currentDialogueList.FindIndex(entry => (entry as DialogueEntry)?.LineKey == nextLineKey);
                if (nextIndex != -1)
                {
                    backindex = dialogueIndex;
                    dialogueIndex = nextIndex;
                }
                else { DialEnd(); return; }
            }
            else { DialEnd(); return; }
        }
        else { DialEnd(); return; }

        SelectionPanel.SetActive(false);
        Selection3Panel.SetActive(false);
        Selection4Panel.SetActive(false);
        ShowNextDialogue();
    }

    public void DialEnd()
    {
        Debug.Log("메인 끝");
        mainDialogue.currentDialogueList.Clear();
        dialogueIndex = 0;
        backindex = -1;

        if (gameManager.GetComponent<TutorialManager>() != null)
        {
            PanelOff();
            TutorialManager.Instance.ChangeGameState(TutorialState.Sub);
        }
        else
        {
            PanelOff();
            if (BackBut) BackBut.SetActive(false);
            mainDialogue.MainEnd();
        }
    }

    void PanelOff()
    {
        GameObject[] panels = { DotPanel, PlayPanel, SelectionPanel, InputPanel, Checkbox3Panel, Checkbox4Panel, Selection3Panel, Selection4Panel };
        foreach (GameObject panel in panels)
            if (panel) panel.SetActive(false);

        if (MainClick) MainClick.SetActive(false);
    }

    private IEnumerator ShowPanelWithDelay(
        GameObject panel,
        CanvasGroup cg,
        float fadeSeconds,
        IList<Button> focusButtons,               // 🔁 여러 버튼 지원
        System.Action beforeActivate,
        bool waitForVideo)
    {
        if (waitForVideo)
        {
            gameManager.mainVideo.PlayVideo();
            yield return new WaitForSeconds(1f);
        }

        beforeActivate?.Invoke();
        panel.SetActive(true);

        yield return StartCoroutine(FadeIn(cg, fadeSeconds, focusButtons)); // 🔁 리스트 전달

        // selection 류는 Next 등록 안 함
        if (focusButtons != null && panel != SelectionPanel)
        {
            foreach (var b in focusButtons)
                if (b) RegisterNextButton(b);
        }
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
        bool waitVideo = animScene == "1";

        switch (textType)
        {
            case "text":
                if (actor == "Dot")
                {
                    if (MainClick) MainClick.SetActive(true);
                    if (korText.Contains("<nickname>") && pc)
                        korText = korText.Replace("<nickname>", pc.GetNickName());

                    //[디버깅]0.5f -> 0.01f
                    StartCoroutine(ShowPanelWithDelay(
                        DotPanel,
                        DotPanel.GetComponent<CanvasGroup>(),
                        0.01f,
                        new List<Button> { MainClick ? MainClick.GetComponent<Button>() : null }, 
                        () => { DotTextUI.text = korText; },
                        waitVideo
                    ));
                }
                else if (actor == "Player")
                {
                    if (MainClick) MainClick.SetActive(true);
                    StartCoroutine(ShowPanelWithDelay(
                        PlayPanel,
                        PlayPanel.GetComponent<CanvasGroup>(),
                        0.5f,
                        new List<Button> { MainClick ? MainClick.GetComponent<Button>() : null },
                        () => { PlayTextUI.text = korText; },
                        waitVideo
                    ));
                }
                break;

            case "selection":
                StartCoroutine(ShowPanelWithDelay(
                    SelectionPanel,
                    SelectionPanel.GetComponent<CanvasGroup>(),
                    0.5f,
                    new List<Button>(SelectionPanel.GetComponentsInChildren<Button>(true)), // ✅ 두 버튼 모두
                    () => { ShowSelection(korText); },
                    waitVideo
                ));
                break;

            case "textbox":
                StartCoroutine(ShowPanelWithDelay(
                    InputPanel,
                    InputPanel.GetComponent<CanvasGroup>(),
                    0.5f,
                    new List<Button> { InputPanel.transform.GetChild(1).GetComponent<Button>() },
                    () =>
                    {
                        InputTextUI.text = korText;
                        Resetinputfield(InputPanel);
                    },
                    waitVideo
                ));
                break;

            case "checkbox3":
                StartCoroutine(ShowPanelWithDelay(
                    Checkbox3Panel,
                    Checkbox3Panel.GetComponent<CanvasGroup>(),
                    0.5f,
                    new List<Button> { Checkbox3Panel.transform.GetChild(1).GetComponent<Button>() },
                    () => { ShowCheckboxOptions(Checkbox3Panel, korText); },
                    waitVideo
                ));
                break;

            case "checkbox4":
                StartCoroutine(ShowPanelWithDelay(
                    Checkbox4Panel,
                    Checkbox4Panel.GetComponent<CanvasGroup>(),
                    0.5f,
                    new List<Button> { Checkbox4Panel.transform.GetChild(1).GetComponent<Button>() },
                    () => { ShowCheckboxOptions(Checkbox4Panel, korText); },
                    waitVideo
                ));
                break;

            case "selection3":
                StartCoroutine(ShowPanelWithDelay(
                    Selection3Panel,
                    Selection3Panel.GetComponent<CanvasGroup>(),
                    0.5f,
                    new List<Button> { Selection3Panel.transform.GetChild(1).GetComponent<Button>() },
                    () => { ShowSelectionOptions(Selection3Panel, korText); },
                    waitVideo
                ));
                break;

            case "selection4":
                StartCoroutine(ShowPanelWithDelay(
                    Selection4Panel,
                    Selection4Panel.GetComponent<CanvasGroup>(),
                    0.5f,
                    new List<Button> { Selection4Panel.transform.GetChild(1).GetComponent<Button>() },
                    () => { ShowSelectionOptions(Selection4Panel, korText); },
                    waitVideo
                ));
                break;
        }
    }

    IEnumerator FadeIn(CanvasGroup canvasGroup, float duration, IList<Button> buttons)
    {
        float counter = 0f;

        if (buttons != null)
            foreach (var b in buttons) if (b) b.interactable = false;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, counter / duration);
            yield return null;
        }

        canvasGroup.alpha = 1;

        if (buttons != null)
            foreach (var b in buttons) if (b) b.interactable = true;
    }

    void RegisterNextButton(Button button)
    {
        if (!button) return;
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
                if (nextIndex != -1) dialogueIndex = nextIndex;
                else { DialEnd(); return; }
            }
            else { DialEnd(); return; }
        }
        else { DialEnd(); return; }

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
            if (dialogueIndex > backindex)
            {
                dialogueIndex--;
                ShowNextDialogue();
            }
            else if (dialogueIndex == backindex)
            {
                if (!string.IsNullOrEmpty(backtag)) pc.DownArcheType(backtag);
                ShowNextDialogue();
            }
        }
    }
}
