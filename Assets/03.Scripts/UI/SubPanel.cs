using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Assets.Script.DialClass;
using UnityEngine.EventSystems;
using System.Reflection;
using System;
/* "��ǳ�� ��ġ�� ��� �ؾ��ϳ� ��ġ ��ġ�� �����ͼ� if pos.x < 0 �̸� ��ġ ���� ������ ���????? 
pos.x > 0 �̸� ��ġ ���� ���� ���
!! Dot�� ��������Ʈ�� ĵ������ �ƴϰ� Panel UI�� ĵ���� �����̶� ��ġ�� ���Ͻ������ �� !!*/

public class SubPanel : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerController pc;
    [SerializeField] private DotController dotcontroller;
    [SerializeField] private SubDialogue sub;
    [SerializeField] private TextMeshProUGUI DotTextUI;
    [SerializeField] private TextMeshProUGUI PlayTextUI;
    [SerializeField] private TextMeshProUGUI InputTextUI;
    [SerializeField] private TMP_InputField Textinput;
    // ����Ʈ�� ���� Dot ���� ������Ʈ��
    [SerializeField] private List<GameObject> dotObjects = new List<GameObject>();

    [SerializeField] private List<GameObject> prObjects = new List<GameObject>();

    // ����Ʈ�� ���� PR_TB ���� ������Ʈ��
    [SerializeField] private List<GameObject> prTbObjects = new List<GameObject>();

    [SerializeField] private List<GameObject> Sels = new List<GameObject>();

    [SerializeField] private Camera mainCamera;

    [SerializeField] private Canvas canvas;

    [SerializeField]
    private GameObject subClick;

    [SerializeField]
    public float prePos;
    public int dialogueIndex = 0;  // Current dialogue index
    public int Day = 0;  // Current day

    void OnEnable()
    {
        pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        //subClick = GameObject.Find("SubClick");
    }

    private void Start()
    {
        subClick = GameObject.Find("SubClick");
        Debug.Log("����: " + subClick);
    }

    public void InitializePanels()
    {
        Debug.Log("���� �г� �ʱ�ȭ");
        Transform parentTransform = this.transform;
        for (int i = 0; i < dotObjects.Count; i++)
        {
            GameObject instantiatedDot = Instantiate(dotObjects[i], parentTransform);
            instantiatedDot.SetActive(false);
            //instantiatedDot.AddComponent<CanvasGroup>();
            dotObjects[i] = instantiatedDot;
        }

        for (int i = 0; i < prTbObjects.Count; i++)
        {
            GameObject instantiatedPrTb = Instantiate(prTbObjects[i], parentTransform);
            instantiatedPrTb.SetActive(false);
            //instantiatedPrTb.AddComponent<CanvasGroup>();
            prTbObjects[i] = instantiatedPrTb;
        }

        for (int i = 0; i < Sels.Count; i++)
        {
            GameObject instantiatedSels = Instantiate(Sels[i], parentTransform);
            instantiatedSels.SetActive(false);
            //instantiatedSels.AddComponent<CanvasGroup>();
            Sels[i] = instantiatedSels;
        }

        for (int i = 0; i < prObjects.Count; i++)
        {
            GameObject instantiatedDot = Instantiate(prObjects[i], parentTransform);
            instantiatedDot.SetActive(false);
            //instantiatedDot.AddComponent<CanvasGroup>();
            prObjects[i] = instantiatedDot;
        }
    }


    void ShowSelection(string options, GameObject Sel)
    {
        string[] selections = options.Split('|');
        for (int i = 0; i < selections.Length; i++)
        {
            Button button = Sel.transform.GetChild(i).GetComponent<Button>();
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = selections[i];
            int index = i;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnSelectionClicked(index));
        }
    }

    public void OnSelectionClicked(int index)
    {
        var currentEntry = sub.GetData(dialogueIndex);

        if (currentEntry.NextLineKey != null)
        {
            string[] nextKeys = currentEntry.NextLineKey.Split('|');
            if (index < nextKeys.Length && int.TryParse(nextKeys[index], out int nextLineKey))
            {
                int nextIndex = sub.currentDialogueList.FindIndex(entry => (entry as SubDialogueEntry)?.LineKey == nextLineKey);

                if (nextIndex != -1)
                {
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

        
        ShowNextDialogue();
    }

    public void DialEnd()
    {
        Debug.Log("���� ��ȭ ��");
        PanelOff();
        sub.currentDialogueList.Clear();
        dialogueIndex = 0;
        sub.Subexit();
        dotcontroller.EndSubScriptList(gameManager.Pattern);
        pc.successSubDialDelegate(pc.GetAlreadyEndedPhase(), pc.currentReward);
    }
    void PanelOff()
    {
        List<GameObject>[] panels = { dotObjects, prTbObjects, Sels, prObjects };
        foreach (List<GameObject> panel in panels)
        {
            foreach (GameObject go in panel)
            {
                go.SetActive(false);
            }
        }
        if (subClick)
            subClick.SetActive(false);
    }

    public void ShowNextDialogue()
    {
        prePos = dotcontroller.Position;
        
        PanelOff();
        if (dialogueIndex >= sub.currentDialogueList.Count)
        {
            DialEnd();
            return;
        }
        sub nextDial = sub.GetData(dialogueIndex);
        string scriptnumber = nextDial.ScriptNumber;
        string textType = nextDial.TextType;
        string actor = nextDial.Actor;
        string korText = nextDial.Text;
        int color = nextDial.Color;
        int determine; //dot���� player���� ���� dot:0 , pl:1
        var currentEntry = sub.GetData(dialogueIndex);
        dotcontroller.ChangeState(DotPatternState.Sub, currentEntry.DotAnim, prePos);

        switch (textType)
        {
            case "text":

                if (actor == "Dot")
                {
                    determine = 0;
                    subClick.SetActive(true);
                    if (korText.Contains("<nickname>"))
                    {
                        if (pc)
                        {
                            korText = korText.Replace("<nickname>", pc.GetNickName());
                        }
                    }
                    Debug.Log("L���� R����: " + dotcontroller.transform.position.x);
                    // if �÷��� ���� ���̸� dotObjects�� B�� �������� �ƴϸ� �� �ð��� �´� ��ǳ����
                    // AND if Dotcontroller.transform ���� x��ǥ�� ������ L �� �ƴϸ� R�� �÷��̾�� �� �ݴ��
                    //�� ���ǿ� �´� ��ǳ�� �����Բ�
                    if (color == 0) // Black
                    {
                        // "Black"�� ���Ե� ������Ʈ�� ������
                        List<GameObject> blackDots = dotObjects.FindAll(dot => dot.name.Contains("Black"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = blackDots.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_L" : "_R"));

                        if (selectedDot != null)
                        {
                            DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            DotTextUI.text = $"{korText}";
                            if (scriptnumber.Contains("tutorial"))
                            {
                                subClick.SetActive(false);
                                TutoConditon(selectedDot, scriptnumber, determine);
                            } 
                            else
                                dotballoon(selectedDot);
                        }
                    }
                    else if (color == 1)
                    {
                        if (gameManager.Time == "Dawn")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Dawn"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_L" : "_R"));

                            if (selectedDot != null)
                            {
                                DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                DotTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                {
                                    subClick.SetActive(false);
                                    TutoConditon(selectedDot, scriptnumber, determine);
                                }
                                else
                                    dotballoon(selectedDot);
                            }
                        }
                        if (gameManager.Time == "Morning")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Mor"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_L" : "_R"));

                            if (selectedDot != null)
                            {
                                DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                DotTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                {
                                    subClick.SetActive(false);
                                    TutoConditon(selectedDot, scriptnumber, determine);
                                }
                                else
                                    dotballoon(selectedDot);
                            }
                        }
                        if (gameManager.Time == "Evening")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Eve"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_L" : "_R"));

                            if (selectedDot != null)
                            {
                                DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                DotTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                {
                                    subClick.SetActive(false);
                                    TutoConditon(selectedDot, scriptnumber, determine);
                                }
                                else
                                    dotballoon(selectedDot);
                            }
                        }
                        if (gameManager.Time == "Night")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Nig"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_L" : "_R"));

                            if (selectedDot != null)
                            {
                                DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                DotTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                {
                                    subClick.SetActive(false);
                                    TutoConditon(selectedDot, scriptnumber, determine);
                                }
                                else
                                    dotballoon(selectedDot);
                            }
                        }
                    }
                }
                else if (actor == "Player")
                {
                    if (korText.Contains("<nickname>"))
                    {
                        if (pc)
                        {
                            korText = korText.Replace("<nickname>", pc.GetNickName());
                        }
                    }
                    determine = 1;
                    if (color == 0) //Black
                    {
                        // "Black"�� ���Ե� ������Ʈ�� ������
                        List<GameObject> blackDots = prObjects.FindAll(dot => dot.name.Contains("Black"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = blackDots.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            if (scriptnumber.Contains("tutorial"))
                                TutoConditon(selectedDot, scriptnumber, determine);
                            else
                                playerballoon(selectedDot);
                        }
                    }
                    else if (color == 1)
                    {
                        if (gameManager.Time == "Dawn")
                        {
                            List<GameObject> Temp = prObjects.FindAll(dot => dot.name.Contains("Dawn"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                            if (selectedDot != null)
                            {
                                PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                PlayTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                    TutoConditon(selectedDot, scriptnumber, determine);
                                else
                                    playerballoon(selectedDot);
                            }
                        }
                        if (gameManager.Time == "Morning")
                        {
                            List<GameObject> Temp = prObjects.FindAll(dot => dot.name.Contains("Mor"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                            if (selectedDot != null)
                            {
                                PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                PlayTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                    TutoConditon(selectedDot, scriptnumber, determine);
                                else
                                    playerballoon(selectedDot);
                            }
                        }
                        if (gameManager.Time == "Evening")
                        {
                            List<GameObject> Temp = prObjects.FindAll(dot => dot.name.Contains("Eve"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                            if (selectedDot != null)
                            {
                                PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                PlayTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                    TutoConditon(selectedDot, scriptnumber, determine);
                                else
                                    playerballoon(selectedDot);
                            }
                        }
                        if (gameManager.Time == "Night")
                        {
                            List<GameObject> Temp = prObjects.FindAll(dot => dot.name.Contains("Nig"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                            if (selectedDot != null)
                            {
                                PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                PlayTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                    TutoConditon(selectedDot, scriptnumber, determine);
                                else
                                    playerballoon(selectedDot);
                            }
                        }
                    }
                }
                break;

            //=============================================================================================================================================================

            case "textbox": //��� �÷��̾� ����
                determine = 1;
                if (color == 0) //Black
                {
                    // "Black"�� ���Ե� ������Ʈ�� ������
                    List<GameObject> blackDots = prTbObjects.FindAll(dot => dot.name.Contains("Black"));

                    // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                    GameObject selectedDot = blackDots.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                    if (selectedDot != null)
                    {
                        PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                        PlayTextUI.text = $"{korText}";
                        Resetinputfield(selectedDot);
                        if (scriptnumber.Contains("tutorial"))
                            TutoConditon(selectedDot, scriptnumber, determine);
                        else
                            playerballoon(selectedDot);
                    }
                }
                else if (color == 1)
                {
                    if (gameManager.Time == "Dawn")
                    {
                        List<GameObject> Temp = prTbObjects.FindAll(dot => dot.name.Contains("Dawn"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            Resetinputfield(selectedDot);
                            if (scriptnumber.Contains("tutorial"))
                                TutoConditon(selectedDot, scriptnumber, determine);
                            else
                                playerballoon(selectedDot);
                        }
                    }
                    if (gameManager.Time == "Morning")
                    {
                        List<GameObject> Temp = prTbObjects.FindAll(dot => dot.name.Contains("Mor"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            Resetinputfield(selectedDot);
                            if (scriptnumber.Contains("tutorial"))
                                TutoConditon(selectedDot, scriptnumber, determine);
                            else
                                playerballoon(selectedDot);
                        }
                    }
                    if (gameManager.Time == "Evening")
                    {
                        List<GameObject> Temp = prTbObjects.FindAll(dot => dot.name.Contains("Eve"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            Resetinputfield(selectedDot);
                            if (scriptnumber.Contains("tutorial"))
                                TutoConditon(selectedDot, scriptnumber, determine);
                            else
                                playerballoon(selectedDot);
                        }
                    }
                    if (gameManager.Time == "Night")
                    {
                        List<GameObject> Temp = prTbObjects.FindAll(dot => dot.name.Contains("Nig"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            Resetinputfield(selectedDot);
                            if (scriptnumber.Contains("tutorial"))
                                TutoConditon(selectedDot, scriptnumber, determine);
                            else
                                playerballoon(selectedDot);
                        }
                    }
                }
                break;

            //=============================================================================================================================================================

            case "selection": //�굵 �÷��̾� ����
                if (color == 0) //Black
                {
                    // "Black"�� ���Ե� ������Ʈ�� ������
                    List<GameObject> blackDots = Sels.FindAll(dot => dot.name.Contains("Black"));

                    // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                    GameObject selectedDot = blackDots.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                    if (selectedDot != null)
                    {
                        selectedDot.SetActive(true);
                        StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponentInChildren<Button>()));
                        ShowSelection(korText, selectedDot);
                    }
                }
                else if (color == 1)
                {
                    if (gameManager.Time == "Dawn")
                    {
                        List<GameObject> Temp = Sels.FindAll(dot => dot.name.Contains("Dawn"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            selectedDot.SetActive(true);
                            StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponentInChildren<Button>()));
                            ShowSelection(korText, selectedDot);
                        }
                    }
                    if (gameManager.Time == "Morning")
                    {
                        List<GameObject> Temp = Sels.FindAll(dot => dot.name.Contains("Mor"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            selectedDot.SetActive(true);
                            StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponentInChildren<Button>()));
                            ShowSelection(korText, selectedDot);
                        }
                    }
                    if (gameManager.Time == "Evening")
                    {
                        List<GameObject> Temp = Sels.FindAll(dot => dot.name.Contains("Eve"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            selectedDot.SetActive(true);
                            StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponentInChildren<Button>()));
                            ShowSelection(korText, selectedDot);
                        }
                    }
                    if (gameManager.Time == "Night")
                    {
                        List<GameObject> Temp = Sels.FindAll(dot => dot.name.Contains("Nig"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            selectedDot.SetActive(true);
                            StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponentInChildren<Button>()));
                            ShowSelection(korText, selectedDot);
                        }
                    }
                }
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
        var currentEntry = sub.GetData(dialogueIndex);
        if (currentEntry.NextLineKey != null)
        {
            dotcontroller.ChangeState(DotPatternState.Sub, currentEntry.DotAnim, prePos);
            if (int.TryParse(currentEntry.NextLineKey, out int nextLineKey))
            {
                int nextIndex = sub.currentDialogueList.FindIndex(entry => (entry as SubDialogueEntry)?.LineKey == nextLineKey);

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
                dialogueIndex++;
            }
        }
        else
        {
            // ���� ���̾�αװ� ������ ������ ���
            Debug.Log("Current entry is null. Ending dialogue.");
            DialEnd();
           
           
            return;
        }

        ShowNextDialogue();
    }

    public void LocationSet(GameObject dotbub)
    {
        Debug.Log("���� ��ǳ�� ��ġ ����");
        Debug.Log(dotcontroller.transform.position);

        Vector2 screenPos = Camera.main.WorldToScreenPoint(dotcontroller.transform.position);
        Vector2 dotacnchorPos;
        RectTransform canvasrect = canvas.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasrect,
            new Vector2(dotcontroller.transform.position.x * Screen.width, dotcontroller.transform.position.y * Screen.height),
            Camera.main,
            out dotacnchorPos
        );
        RectTransform speechBubbleUI = dotbub.GetComponent<RectTransform>();
        Debug.Log(screenPos);
        Debug.Log("��Ŀ�� ������: " + dotacnchorPos);

        if (dotcontroller.transform.position.x < 0)
        {
            Debug.Log("1");
            // ��ǳ���� dot�� �����ʿ� ��ġ
            speechBubbleUI.transform.position = screenPos - new Vector2(-100, -210);
        }
        else
        {
            Debug.Log("2");
            // ��ǳ���� dot�� ���ʿ� ��ġ
            speechBubbleUI.transform.position = screenPos - new Vector2(1100, -210);
        }

        // 4. ��ǳ���� Ȱ��ȭ�Ͽ� ǥ��
        speechBubbleUI.gameObject.SetActive(true);
    }

    public void PlayerLocationSet(GameObject dotbub)
    {
        RectTransform rectTransform = dotbub.GetComponent<RectTransform>();

        if (dotcontroller.transform.position.x < 0)
        {

            // ���� �ϴܿ� ��ġ
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0, 0);
            rectTransform.anchoredPosition = new Vector2(50, -400);
        }

        else
        {
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(1, 0);
            rectTransform.anchoredPosition = new Vector2(-50, -400);
        }

        rectTransform.gameObject.SetActive(true);
    }



    private void OnDisable()
    {
        PanelOff();
    }

    public void Resetinputfield(GameObject field)
    {
        TextMeshProUGUI inputfield = field.transform.GetChild(3).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        Textinput = field.transform.GetChild(3).GetComponent<TMP_InputField>();
        Textinput.text = "";
        inputfield.text = "";
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void dotballoon(GameObject selectedDot)
    {
        subClick.SetActive(true);
        LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
        StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, subClick.GetComponent<Button>()));
        RegisterNextButton(subClick.GetComponent<Button>());
    }

    public void playerballoon(GameObject selectedDot)
    {
        PlayerLocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
        StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
        RegisterNextButton(selectedDot.transform.GetComponent<Button>());
    }

    public void TutoConditon(GameObject selectedDot, string scriptnumber, int determine)
    {
        // SubTuto �ν��Ͻ� ��������
        SubTuto subTuto = gameManager.gameObject.GetComponent<SubTuto>();

        if (subTuto == null)
        {
            Debug.Log("SubTuto ������Ʈ�� ã�� �� �����ϴ�.");
            subClick.SetActive(true);
            if (determine == 0)
            {
                dotballoon(selectedDot);
            }
            else
            {
                playerballoon(selectedDot);
            }
        }

        // SubTuto Ÿ�� ��������
        Type type = subTuto.GetType();

        // �޼��� ���� ��������
        MethodInfo method = type.GetMethod(scriptnumber, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // �޼��� ���� ���� Ȯ�� �� ȣ��
        if (method != null)
        {
            // �Ű������� �����Ͽ� �޼��� ����
            method.Invoke(subTuto, new object[] { selectedDot, determine });
            Debug.Log($"{scriptnumber} �޼��尡 ����Ǿ����ϴ�.");
        }
        else
        {
            Debug.Log($"'{scriptnumber}' �޼��带 ã�� �� �����ϴ�.");
            subClick.SetActive(true);
            if (determine == 0)
            {
                dotballoon(selectedDot);
            }
            else
            {
                playerballoon(selectedDot);
            }
        }
    }
    public void clickon()
    {
        subClick.SetActive (true);
    }

    public void Subcontinue()
    {
        SubTuto subTuto = gameManager.gameObject.GetComponent<SubTuto>();

        if(subTuto)
        {
            subTuto.Subcontinue();
            prePos = dotcontroller.Position;
        }
        else
        {
            ShowNextDialogue();
        }
    }
}