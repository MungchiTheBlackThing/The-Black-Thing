using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Assets.Script.DialClass;

/* "��ǳ�� ��ġ�� ��� �ؾ��ϳ� ��ġ ��ġ�� �����ͼ� if pos.x < 0 �̸� ��ġ ���� ������ ���????? 
 pos.x > 0 �̸� ��ġ ���� ���� ���
 !! Dot�� ��������Ʈ�� ĵ������ �ƴϰ� Panel UI�� ĵ���� �����̶� ��ġ�� ���Ͻ������ �� !!*/

public class SubPanel : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerController pc;
    [SerializeField] private DotController dot;
    [SerializeField] private SubDialogue sub;

    [SerializeField] private TextMeshProUGUI DotTextUI;
    [SerializeField] private TextMeshProUGUI PlayTextUI;
    [SerializeField] private TextMeshProUGUI InputTextUI;

    // ����Ʈ�� ���� Dot ���� ������Ʈ��
    [SerializeField] private List<GameObject> dotObjects = new List<GameObject>();

    // ����Ʈ�� ���� PR_TB ���� ������Ʈ��
    [SerializeField] private List<GameObject> prTbObjects = new List<GameObject>();

    [SerializeField] private GameObject SubTwoSelection;

    [SerializeField] private Camera mainCamera;

    [SerializeField] private Canvas canvas;

    public int dialogueIndex = 0;  // Current dialogue index
    public int Day = 0;  // Current day

    void OnEnable()
    {
        pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
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

        GameObject instantiatedSubTwoSelection = Instantiate(SubTwoSelection, parentTransform);
        instantiatedSubTwoSelection.SetActive(false);
        //instantiatedSubTwoSelection.AddComponent<CanvasGroup>();
        SubTwoSelection = instantiatedSubTwoSelection;
    }


    void ShowSelection(string options)
    {
        string[] selections = options.Split('|');
        for (int i = 0; i < selections.Length; i++)
        {
            Button button = SubTwoSelection.transform.GetChild(i).GetComponent<Button>();
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
        PanelOff();
        sub.currentDialogueList.Clear();
        dialogueIndex = 0;
    }
    void PanelOff()
    {
        List<GameObject>[] panels = { dotObjects, prTbObjects };
        foreach (List<GameObject> panel in panels)
        {
            foreach (GameObject go in panel)
            {
                go.SetActive(false);
            }
        }
        SubTwoSelection.SetActive(false);
    }

    public void ShowNextDialogue()
    {
        PanelOff();
        if (dialogueIndex >= sub.currentDialogueList.Count)
        {
            DialEnd();
            return;
        }
        string textType = sub.GetData(dialogueIndex).TextType;
        string actor = sub.GetData(dialogueIndex).Actor;
        string korText = sub.GetData(dialogueIndex).Text;
        int color = sub.GetData(dialogueIndex).Color;

        switch (textType)
        {
            case "text":
                if (actor == "Dot")
                {
                    if (korText.Contains("<nickname>"))
                    {
                        if (pc)
                        {
                            korText = korText.Replace("<nickname>", pc.GetNickName());
                        }
                    }

                    // if �÷��� ���� ���̸� dotObjects�� B�� �������� �ƴϸ� �� �ð��� �´� ��ǳ����
                    // AND if Dotcontroller.transform ���� x��ǥ�� ������ L �� �ƴϸ� R�� �÷��̾�� �� �ݴ��
                    //�� ���ǿ� �´� ��ǳ�� �����Բ�
                    if (color == 0) // Black
                    {
                        // "Black"�� ���Ե� ������Ʈ�� ������
                        List<GameObject> blackDots = dotObjects.FindAll(dot => dot.name.Contains("Black"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = blackDots.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_L" : "_R"));

                        if (selectedDot != null)
                        {
                            Debug.Log("���� �г�" + selectedDot);
                            LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                            DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            DotTextUI.text = $"{korText}";
                            StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                            RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                        }
                    }
                    else if (color == 1)
                    {
                        if (gameManager.Time == "Dawn")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Dawn"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_L" : "_R"));

                            if (selectedDot != null)
                            {
                                Debug.Log("���� �г�" + selectedDot);
                                LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                                DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                DotTextUI.text = $"{korText}";
                                StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                                RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                            }
                        }
                        if (gameManager.Time == "Morning")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Mor"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_L" : "_R"));

                            if (selectedDot != null)
                            {
                                Debug.Log("���� �г�" + selectedDot);
                                LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                                DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                DotTextUI.text = $"{korText}";
                                StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                                RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                            }
                        }
                        if (gameManager.Time == "Evening")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Eve"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_L" : "_R"));

                            if (selectedDot != null)
                            {
                                Debug.Log("���� �г�" + selectedDot);
                                LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                                DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                DotTextUI.text = $"{korText}";
                                StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                                RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                            }
                        }
                        if (gameManager.Time == "Night")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Nig"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_L" : "_R"));

                            if (selectedDot != null)
                            {
                                Debug.Log("���� �г�" + selectedDot);
                                LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                                DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                DotTextUI.text = $"{korText}";
                                StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                                RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                            }
                        }
                    }
                }
                else if (actor == "Player")
                {
                    if (color == 0) //Black
                    {
                        // "Black"�� ���Ե� ������Ʈ�� ������
                        List<GameObject> blackDots = dotObjects.FindAll(dot => dot.name.Contains("Black"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = blackDots.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            Debug.Log("���� �г�" + selectedDot);
                            LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                            RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                        }
                    }
                    else if (color == 1)
                    {
                        if (gameManager.Time == "Dawn")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Dawn"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_R" : "_L"));

                            if (selectedDot != null)
                            {
                                Debug.Log("���� �г�" + selectedDot);
                                LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                                PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                PlayTextUI.text = $"{korText}";
                                StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                                RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                            }
                        }
                        if (gameManager.Time == "Morning")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Mor"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_R" : "_L"));

                            if (selectedDot != null)
                            {
                                Debug.Log("���� �г�" + selectedDot);
                                LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                                PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                PlayTextUI.text = $"{korText}";
                                StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                                RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                            }
                        }
                        if (gameManager.Time == "Evening")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Eve"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_R" : "_L"));

                            if (selectedDot != null)
                            {
                                Debug.Log("���� �г�" + selectedDot);
                                LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                                PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                PlayTextUI.text = $"{korText}";
                                StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                                RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                            }
                        }
                        if (gameManager.Time == "Night")
                        {
                            List<GameObject> Temp = dotObjects.FindAll(dot => dot.name.Contains("Nig"));

                            // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                            GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_R" : "_L"));

                            if (selectedDot != null)
                            {
                                Debug.Log("���� �г�" + selectedDot);
                                LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                                PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                                PlayTextUI.text = $"{korText}";
                                StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                                RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                            }
                        }
                    }
                }
                break;
         //=============================================================================================================================================================
           
            case "textbox":
                if (color == 0) //Black
                {
                    // "Black"�� ���Ե� ������Ʈ�� ������
                    List<GameObject> blackDots = prTbObjects.FindAll(dot => dot.name.Contains("Black"));

                    // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                    GameObject selectedDot = blackDots.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_R" : "_L"));

                    if (selectedDot != null)
                    {
                        LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                        PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                        PlayTextUI.text = $"{korText}";
                        StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                        RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                    }
                }
                else if (color == 1)
                {
                    if (gameManager.Time == "Dawn")
                    {
                        List<GameObject> Temp = prTbObjects.FindAll(dot => dot.name.Contains("Dawn"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                            RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                        }
                    }
                    if (gameManager.Time == "Morning")
                    {
                        List<GameObject> Temp = prTbObjects.FindAll(dot => dot.name.Contains("Mor"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                            RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                        }
                    }
                    if (gameManager.Time == "Evening")
                    {
                        List<GameObject> Temp = prTbObjects.FindAll(dot => dot.name.Contains("Eve"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                            RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                        }
                    }
                    if (gameManager.Time == "Night")
                    {
                        List<GameObject> Temp = prTbObjects.FindAll(dot => dot.name.Contains("Nig"));

                        // Dot�� x ��ǥ�� �����̸� "L" ���Ե� ������Ʈ��, ����̸� "R" ���Ե� ������Ʈ�� ����
                        GameObject selectedDot = Temp.Find(dot => dot.name.Contains(dot.transform.position.x < 0 ? "_R" : "_L"));

                        if (selectedDot != null)
                        {
                            LocationSet(selectedDot); // ������ ������Ʈ�� Ȱ��ȭ
                            PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                            PlayTextUI.text = $"{korText}";
                            StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.5f, selectedDot.transform.GetComponent<Button>()));
                            RegisterNextButton(selectedDot.transform.GetComponent<Button>());
                        }
                    }
                }
                break;

         //=============================================================================================================================================================
            
            case "selection":
                SubTwoSelection.SetActive(true);
                StartCoroutine(FadeIn(SubTwoSelection.GetComponent<CanvasGroup>(), 0.5f, SubTwoSelection.transform.GetComponentInChildren<Button>()));
                ShowSelection(korText);
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
            Debug.Log("Current entry is null. Ending dialogue.");
            DialEnd();
            return;
        }

        ShowNextDialogue();
    }

    public void LocationSet(GameObject dotbub)
    {
        Debug.Log("���� ��ǳ�� ��ġ ����");
        Debug.Log(dot.transform.position);
        Vector2 screenPos = Camera.main.WorldToScreenPoint(dot.transform.position);
        RectTransform speechBubbleUI = dotbub.GetComponent<RectTransform>();
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //    canvas.transform as RectTransform,
        //    screenPos,
        //    canvas.worldCamera,
        //    out canvasPos);
       
        // 3. dot�� x ��ǥ�� ���� ��ǳ���� ���� �Ǵ� �����ʿ� ��ġ
        if (dot.transform.position.x < 0)
        {
            // ��ǳ���� dot�� �����ʿ� ��ġ
            speechBubbleUI.transform.position = screenPos;
        }
        else
        {
            // ��ǳ���� dot�� ���ʿ� ��ġ
            speechBubbleUI.transform.position = screenPos - new Vector2(1100, -210);
        }

        // 4. ��ǳ���� Ȱ��ȭ�Ͽ� ǥ��
        speechBubbleUI.gameObject.SetActive(true);
    }

}