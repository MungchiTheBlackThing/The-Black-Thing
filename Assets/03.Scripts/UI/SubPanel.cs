using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Assets.Script.DialClass;
using UnityEngine.EventSystems;
using System.Reflection;
using System;
/* "말풍선 위치를 어떻게 해야하나 뭉치 위치를 가져와서 if pos.x < 0 이면 뭉치 기준 오른쪽 상단????? 
pos.x > 0 이면 뭉치 기준 왼쪽 상단
!! Dot은 스프라이트라 캔버스가 아니고 Panel UI는 캔버스 기준이라 위치를 통일시켜줘야 함 !!*/

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
    [SerializeField] public SubDialogue subdialogue;
    // 리스트로 묶은 Dot 게임 오브젝트들
    [SerializeField] private List<GameObject> dotObjects = new List<GameObject>();

    [SerializeField] private List<GameObject> prObjects = new List<GameObject>();

    // 리스트로 묶은 PR_TB 게임 오브젝트들
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

    [SerializeField]
    SubTuto subTuto;
    void OnEnable()
    {
        pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        //subClick = GameObject.Find("SubClick");
    }

    public void InitializePanels()
    {
        Debug.Log("서브 패널 초기화");
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
    public GameObject FindPanelObjectByName(string name)
    {
        List<GameObject>[] allLists = { dotObjects, prTbObjects, Sels, prObjects };

        foreach (var list in allLists)
        {
            foreach (var obj in list)
            {
                if (obj.name == name)
                    return obj;
            }
        }

        Debug.LogWarning($"SubPanel 안에서 이름이 {name} 인 오브젝트를 찾을 수 없습니다.");
        return null;
    }

    void ShowSelection(string options, GameObject Sel)
    {
        SelLocationSet(Sel);
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
                Debug.Log("NextLineKey is not a valid integer. Moving to the next entry by index.");
                dialogueIndex++;
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
        Debug.Log("서브 대화 끝");
        PanelOff();
        sub.currentDialogueList.Clear();
        dialogueIndex = 0;
        if (!string.IsNullOrEmpty(pc.currentReward)) //비어있거나 없는 경우
        {
            pc.successSubDialDelegate(pc.GetAlreadyEndedPhase(), pc.currentReward);
        }
        if (pc.GetAlreadyEndedPhase() != 5)
            dotcontroller.EndSubScriptList(gameManager.Pattern);
        sub.Subexit();
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
            Debug.Log("인덱스가 넘어감");
            DialEnd();
            return;
        }
        sub nextDial = sub.GetData(dialogueIndex);
        string scriptnumber = nextDial.ScriptNumber;
        string textType = nextDial.TextType;
        string actor = nextDial.Actor;
        string korText = nextDial.Text;
        int color = nextDial.Color;
        int determine; //dot인지 player인지 구분 dot:0 , pl:1
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
                    Debug.Log("L인지 R인지: " + dotcontroller.transform.position.x);
                    // if 컬러가 검은 색이면 dotObjects의 B를 가져오고 아니면 각 시간에 맞는 말풍선을
                    // AND if Dotcontroller.transform 으로 x좌표가 음수면 L 를 아니면 R을 플레이어는 그 반대로
                    //각 조건에 맞는 말풍선 켜지게끔
                    if (color == 0) // Black
                    {
                        // "Black"이 포함된 오브젝트만 가져옴
                        List<GameObject> blackDots = dotObjects.FindAll(dot => dot.name.Contains("Black"));

                        // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                        GameObject selectedDot = blackDots.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_L" : "_R"));

                        if (selectedDot != null)
                        {
                            DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            DotTextUI.text = $"{korText}";
                            if (scriptnumber.Contains("tutorial"))
                            {
                                subClick.SetActive(false);
                                TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
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

                            // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_L" : "_R"));

                            if (selectedDot != null)
                            {
                                DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                DotTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                {
                                    subClick.SetActive(false);
                                    TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                                }
                                else
                                    dotballoon(selectedDot);
                            }
                        }
                        if (gameManager.Time == "Morning")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Mor"));

                            // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_L" : "_R"));

                            if (selectedDot != null)
                            {
                                DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                DotTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                {
                                    subClick.SetActive(false);
                                    TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                                }
                                else
                                    dotballoon(selectedDot);
                            }
                        }
                        if (gameManager.Time == "Evening")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Eve"));

                            // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_L" : "_R"));

                            if (selectedDot != null)
                            {
                                DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                DotTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                {
                                    subClick.SetActive(false);
                                    TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                                }
                                else
                                    dotballoon(selectedDot);
                            }
                        }
                        if (gameManager.Time == "Night")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Nig"));

                            // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_L" : "_R"));

                            if (selectedDot != null)
                            {
                                DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                DotTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                {
                                    subClick.SetActive(false);
                                    TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                                }
                                else
                                    dotballoon(selectedDot);
                            }
                        }
                    }
                }
                else if (actor == "Player")
                {
                    subClick.SetActive(true);
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
                        // "Black"이 포함된 오브젝트만 가져옴
                        List<GameObject> blackDots = prObjects.FindAll(dot => dot.name.Contains("Black"));

                        // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                        GameObject selectedDot = blackDots.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            if (scriptnumber.Contains("tutorial"))
                            {
                                subClick.SetActive(false);
                                TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                            }
                            else
                                playerballoon(selectedDot);
                        }
                    }
                    else if (color == 1)
                    {
                        if (gameManager.Time == "Dawn")
                        {
                            List<GameObject> Temp = prObjects.FindAll(dot => dot.name.Contains("Dawn"));

                            // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                            if (selectedDot != null)
                            {
                                PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                PlayTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                {
                                    subClick.SetActive(false);
                                    TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                                }
                                else
                                    playerballoon(selectedDot);
                            }
                        }
                        if (gameManager.Time == "Morning")
                        {
                            List<GameObject> Temp = prObjects.FindAll(dot => dot.name.Contains("Mor"));

                            // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                            if (selectedDot != null)
                            {
                                PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                PlayTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                {
                                    subClick.SetActive(false);
                                    TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                                }
                                else
                                    playerballoon(selectedDot);
                            }
                        }
                        if (gameManager.Time == "Evening")
                        {
                            List<GameObject> Temp = prObjects.FindAll(dot => dot.name.Contains("Eve"));

                            // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                            if (selectedDot != null)
                            {
                                PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                PlayTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                {
                                    subClick.SetActive(false);
                                    TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                                }
                                else
                                    playerballoon(selectedDot);
                            }
                        }
                        if (gameManager.Time == "Night")
                        {
                            List<GameObject> Temp = prObjects.FindAll(dot => dot.name.Contains("Nig"));

                            // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                            if (selectedDot != null)
                            {
                                PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                PlayTextUI.text = $"{korText}";
                                if (scriptnumber.Contains("tutorial"))
                                {
                                    subClick.SetActive(false);
                                    TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                                }
                                else
                                    playerballoon(selectedDot);
                            }
                        }
                    }
                }
                break;

            //=============================================================================================================================================================

            case "textbox": //얘는 플레이어 기준
                determine = 1;
                if (color == 0) //Black
                {
                    // "Black"이 포함된 오브젝트만 가져옴
                    List<GameObject> blackDots = prTbObjects.FindAll(dot => dot.name.Contains("Black"));

                    // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                    GameObject selectedDot = blackDots.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                    if (selectedDot != null)
                    {
                        PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                        PlayTextUI.text = $"{korText}";
                        Resetinputfield(selectedDot);
                        if (scriptnumber.Contains("tutorial"))
                            TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                        else
                            playerballoon(selectedDot);
                    }
                }
                else if (color == 1)
                {
                    if (gameManager.Time == "Dawn")
                    {
                        List<GameObject> Temp = prTbObjects.FindAll(dot => dot.name.Contains("Dawn"));

                        // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            Resetinputfield(selectedDot);
                            if (scriptnumber.Contains("tutorial"))
                                TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                            else
                                playerballoon(selectedDot);
                        }
                    }
                    if (gameManager.Time == "Morning")
                    {
                        List<GameObject> Temp = prTbObjects.FindAll(dot => dot.name.Contains("Mor"));

                        // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            Resetinputfield(selectedDot);
                            if (scriptnumber.Contains("tutorial"))
                                TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                            else
                                playerballoon(selectedDot);
                        }
                    }
                    if (gameManager.Time == "Evening")
                    {
                        List<GameObject> Temp = prTbObjects.FindAll(dot => dot.name.Contains("Eve"));

                        // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            Resetinputfield(selectedDot);
                            if (scriptnumber.Contains("tutorial"))
                                TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                            else
                                playerballoon(selectedDot);
                        }
                    }
                    if (gameManager.Time == "Night")
                    {
                        List<GameObject> Temp = prTbObjects.FindAll(dot => dot.name.Contains("Nig"));

                        // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dotcontroller.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            Resetinputfield(selectedDot);
                            if (scriptnumber.Contains("tutorial"))
                                TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                            else
                                playerballoon(selectedDot);
                        }
                    }
                }
                break;

            //=============================================================================================================================================================

            case "selection": //얘도 플레이어 기준
                if (color == 0) //Black
                {
                    // "Black"이 포함된 오브젝트만 가져옴
                    List<GameObject> blackDots = Sels.FindAll(dot => dot.name.Contains("Black"));

                    // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
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

                        // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
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

                        // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
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

                        // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
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

                        // Dot의 x 좌표가 음수이면 "L" 포함된 오브젝트를, 양수이면 "R" 포함된 오브젝트를 선택
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
        yield return new WaitForSeconds(duration);
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
                    Debug.Log("넥스트 키가 잘못됨");
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
            // 서브 다이얼로그가 온전히 끝났을 경우
            Debug.Log("Current entry is null. Ending dialogue.");
            DialEnd();
           
           
            return;
        }

        ShowNextDialogue();
    }

    public void LocationSet(GameObject dotbub)
    {
        // 1. 말풍선 RectTransform 가져오기
        RectTransform speechBubbleUI = dotbub.GetComponent<RectTransform>();

        // 2. 캔버스의 RectTransform 가져오기
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // 3. dotcontroller의 월드 좌표 가져오기
        Vector3 worldPos = dotcontroller.transform.position;

        // 4. 월드 좌표를 **스크린 좌표로 변환**
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // 5. 스크린 좌표를 **캔버스 로컬 좌표로 변환**
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out anchoredPos);

        if (dotcontroller.transform.position.x < 0)
        {
            Debug.Log("1");
            // 말풍선을 dot의 오른쪽에 배치
            speechBubbleUI.anchoredPosition = anchoredPos;
        }
        else
        {
            Debug.Log("2");
            // 말풍선을 dot의 왼쪽에 배치
            speechBubbleUI.anchoredPosition = anchoredPos;
        }
        Debug.Log("최종 위치: " + speechBubbleUI.anchoredPosition);
        // 6. 변환된 UI 좌표 적용

        // 7. 말풍선 활성화
        speechBubbleUI.gameObject.SetActive(true);

        // Debug 로그로 확인
        Debug.Log($"2D 오브젝트 위치 (월드): {worldPos}");
        Debug.Log($"2D 오브젝트 위치 (스크린): {screenPos}");
        Debug.Log($"변환된 UI 좌표 (캔버스 기준): {anchoredPos}");
    }





    public void PlayerLocationSet(GameObject dotbub)
    {
        RectTransform rectTransform = dotbub.GetComponent<RectTransform>();

        // dotcontroller의 x 좌표 기준으로 말풍선 위치 설정
        if (dotcontroller.transform.position.x < 0)
        {
            // 왼쪽 하단에 배치 (피벗을 왼쪽 하단 기준으로 설정)
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(1, 0); // 왼쪽 하단 기준
            rectTransform.anchoredPosition = new Vector2(900, -400); // 위치 조정
        }
        else
        {
            // 오른쪽 하단에 배치 (피벗을 오른쪽 하단 기준으로 설정)
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0, 0); // 오른쪽 하단 기준
            rectTransform.anchoredPosition = new Vector2(-900, -400); // 위치 조정
        }

        // 말풍선 UI 활성화
        rectTransform.gameObject.SetActive(true);
    }

    public void SelLocationSet(GameObject dotbub)
    {
        RectTransform rectTransform = dotbub.GetComponent<RectTransform>();

        // dotcontroller의 x 좌표 기준으로 말풍선 위치 설정
        if (dotcontroller.transform.position.x < 0)
        {
            // 왼쪽 하단에 배치 (피벗을 왼쪽 하단 기준으로 설정)
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(1, 0); // 왼쪽 하단 기준
            rectTransform.anchoredPosition = new Vector2(300, 0); // 위치 조정
        }
        else
        {
            // 오른쪽 하단에 배치 (피벗을 오른쪽 하단 기준으로 설정)
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0, 0); // 오른쪽 하단 기준
            rectTransform.anchoredPosition = new Vector2(200, 600); // 위치 조정
        }

        // 말풍선 UI 활성화
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
        LocationSet(selectedDot); // 선택한 오브젝트를 활성화
        StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, subClick.GetComponent<Button>()));
        RegisterNextButton(subClick.GetComponent<Button>());
    }

    public void playerballoon(GameObject selectedDot)
    {
        subClick.SetActive(true);
        PlayerLocationSet(selectedDot); // 선택한 오브젝트를 활성화
        StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, subClick.GetComponent<Button>()));
        RegisterNextButton(subClick.GetComponent<Button>());
    }

    public void TutoConditon(GameObject selectedDot, string scriptnumber, int determine , int index)
    {
        // SubTuto 인스턴스 가져오기
        subTuto = gameManager.gameObject.GetComponent<SubTuto>();

        if (subTuto == null)
        {
            Debug.Log("SubTuto 컴포넌트를 찾을 수 없습니다.");
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

        // SubTuto 타입 가져오기
        Type type = subTuto.GetType();

        // 메서드 정보 가져오기
        MethodInfo method = type.GetMethod(scriptnumber, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // 메서드 존재 여부 확인 후 호출
        if (method != null)
        {
            // 매개변수를 전달하여 메서드 실행
            method.Invoke(subTuto, new object[] { selectedDot, determine, index });
            Debug.Log($"{scriptnumber} 메서드가 실행되었습니다.");
        }
        else
        {
            Debug.Log($"'{scriptnumber}' 메서드를 찾을 수 없습니다.");
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
        subTuto = gameManager.gameObject.GetComponent<SubTuto>();

        if(subTuto)
        {
            Debug.Log("서브 튜토");
            subTuto.Subcontinue();
            prePos = dotcontroller.Position;
        }
        else
        {
            ShowNextDialogue();
        }
    }
    public void clear()
    {
        PanelOff();
        sub.currentDialogueList.Clear();
        dialogueIndex = 0;
    }
}