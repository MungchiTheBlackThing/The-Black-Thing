using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Script.DialClass;

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

    // 말풍선/선택지 프리팹 컨테이너
    [SerializeField] private List<GameObject> dotObjects = new();
    [SerializeField] private List<GameObject> prObjects = new();
    [SerializeField] private List<GameObject> prTbObjects = new();
    [SerializeField] private List<GameObject> Sels = new();

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject subClick;
    [SerializeField] private GameObject subPanelObj;

    [SerializeField] private SubTuto subTuto;

    public float prePos;
    public int dialogueIndex = 0;
    public int Day = 0;

    private const string PH_KO = "당신의 생각을 입력해주세요";
    private const string PH_EN = "Please enter your thoughts";

    void OnEnable()
    {
        pc = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    // ============================= 초기화 =============================

    public void InitializePanels()
    {
        subPanelObj = this.gameObject;
        Transform parent = transform;

        dotObjects = InstantiateList(dotObjects, parent);
        prObjects = InstantiateList(prObjects, parent);
        prTbObjects = InstantiateList(prTbObjects, parent);
        Sels = InstantiateList(Sels, parent);

        // 모든 루트에 CanvasGroup 보장 (페이드/차단 제어용)
        EnsureCanvasGroup(dotObjects);
        EnsureCanvasGroup(prObjects);
        EnsureCanvasGroup(prTbObjects);
        EnsureCanvasGroup(Sels);

        SetActiveAll(dotObjects, false);
        SetActiveAll(prObjects, false);
        SetActiveAll(prTbObjects, false);
        SetActiveAll(Sels, false);

        int LANGUAGE = (int)pc.GetLanguage();
        List<GameObject> targetsList = new List<GameObject>(32);
        foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
        {
            if (t.gameObject.CompareTag("Placeholder"))
                targetsList.Add(t.gameObject);
        }
        GameObject[] targets = targetsList.ToArray();
        string placeholderText = (LANGUAGE == 0) ? PH_KO : PH_EN;
        Debug.Log("찾은 Placeholder: " + targets.Length);
        for (int i = 0; i < targets.Length; i++)
        {
            var go = targets[i];

            var tmpText = go.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = placeholderText;
            }
        }
    }

    private static List<GameObject> InstantiateList(List<GameObject> src, Transform parent)
    {
        var list = new List<GameObject>(src.Count);
        foreach (var go in src)
        {
            var inst = GameObject.Instantiate(go, parent);
            inst.SetActive(false);
            list.Add(inst);
        }
        return list;
    }

    private static void EnsureCanvasGroup(List<GameObject> list)
    {
        foreach (var go in list)
            if (!go.GetComponent<CanvasGroup>()) go.AddComponent<CanvasGroup>();
    }

    private static void SetActiveAll(List<GameObject> list, bool on)
    {
        foreach (var go in list) go.SetActive(on);
    }

    // ============================= 외부에서 필요로 하는 메서드 (복원) =============================

    public void clickon()
    {
        if (subClick) subClick.SetActive(true);
    }

    public GameObject FindPanelObjectByName(string name)
    {
        List<GameObject>[] allLists = { dotObjects, prTbObjects, Sels, prObjects };
        foreach (var list in allLists)
            foreach (var obj in list)
                if (obj && obj.name == name)
                    return obj;

        Debug.LogWarning($"SubPanel: 이름이 '{name}' 인 오브젝트를 찾을 수 없습니다.");
        return null;
    }

    public void clear()
    {
        PanelOff();
        if (sub != null && sub.currentDialogueList != null)
            sub.currentDialogueList.Clear();
        dialogueIndex = 0;
    }

    // ============================= 선택지 & 페이드 유틸 =============================

    // 선택지: 버튼 바인딩 (자식 인덱스 가정 X, 실제 Button만)
    void ShowSelection(string options, GameObject selRoot)
    {
        SelLocationSet(selRoot);

        var selections = options.Split('|');
        var buttons = selRoot.GetComponentsInChildren<Button>(true);

        int count = Mathf.Min(selections.Length, buttons.Length);
        for (int i = 0; i < buttons.Length; i++)
        {
            var btn = buttons[i];
            bool use = i < count;

            btn.gameObject.SetActive(use);
            btn.onClick.RemoveAllListeners();
            if (!use) continue;

            var txt = btn.GetComponentInChildren<TextMeshProUGUI>(true);
            if (txt) txt.text = selections[i];

            int index = i;
            btn.onClick.AddListener(() => OnSelectionClicked(index));
        }

        // 첫 버튼 포커스 (옵션)
        if (count > 0)
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);

        // 선택지 페이드-인: 루트 CanvasGroup만 제어 (페이드 중엔 전면 차단)
        var cg = selRoot.GetComponent<CanvasGroup>();
        StartCoroutine(FadeCanvasGroup(cg, 0.5f, fadeIn: true));
    }

    // 공용: 캔버스 그룹 페이드 (페이드 중 클릭/레이캐스트 완전 차단)
    IEnumerator FadeCanvasGroup(CanvasGroup cg, float duration, bool fadeIn, Action onDone = null)
    {
        cg.blocksRaycasts = true;  // 뒤 클릭 차단
        cg.interactable = false;   // 내부 버튼 비활성

        float start = fadeIn ? 0f : 1f;
        float end = fadeIn ? 1f : 0f;
        float t = 0f;

        if (!cg.gameObject.activeSelf) cg.gameObject.SetActive(true);

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }
        cg.alpha = end;

        if (fadeIn)
        {
            // 페이드-인 완료 후에만 버튼 허용
            cg.interactable = true;
            cg.blocksRaycasts = true; // 선택지 떠있는 동안 뒤 클릭 계속 차단
        }
        else
        {
            // 페이드-아웃 완료 시 비활성
            cg.blocksRaycasts = false;
            cg.gameObject.SetActive(false);
        }

        onDone?.Invoke();
    }

    // (단일 버튼용) 필요 시 유지. button이 null이면 건너뜀.
    IEnumerator FadeIn(CanvasGroup canvasGroup, float duration, Button buttonOrNull)
    {
        float counter = 0f;
        if (buttonOrNull) buttonOrNull.interactable = false;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, counter / duration);
            yield return null;
        }
        canvasGroup.alpha = 1;
        if (buttonOrNull)
        {
            yield return new WaitForSeconds(duration);
            buttonOrNull.interactable = true;
        }
    }

    // ============================= 선택 콜백 =============================

    public void OnSelectionClicked(int index)
    {
        Debug.Log($"선택 클릭됨: {index}");
        var currentEntry = sub.GetData(dialogueIndex);

        if (currentEntry.NextLineKey != null)
        {
            string[] nextKeys = currentEntry.NextLineKey.Split('|');
            if (index < nextKeys.Length && int.TryParse(nextKeys[index], out int nextLineKey))
            {
                int nextIndex = sub.currentDialogueList.FindIndex(
                    entry => (entry as SubDialogueEntry)?.LineKey == nextLineKey);

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

    // ============================= 공통 흐름 =============================

    public void DialEnd()
    {
        Debug.Log("서브 대화 끝");
        ScreenShield.Off();
        PanelOff();
        sub.currentDialogueList.Clear();
        dialogueIndex = 0;

        // currentReward가 있고, 실제로 현재 subseq에 대한 reward인지 확인
        int currentSubseq = pc.GetSubseq();
        if (!string.IsNullOrEmpty(pc.currentReward))
        {
            Debug.Log($"[DialEnd] currentReward 발견: {pc.currentReward}, 현재 subseq: {currentSubseq}");
            pc.successSubDialDelegate(pc.GetCurrentPhase(), pc.currentReward);
            //pc.currentReward = "";
        }

        if (pc.GetCurrentPhase() != 5)
            dotcontroller.EndSubScriptList(gameManager.Pattern);

        sub.Subexit(); // AfterScript 재생 로직이 Subexit 내부에 포함되어 있음
    }

    void PanelOff()
    {
        SetActiveAll(dotObjects, false);
        SetActiveAll(prObjects, false);
        SetActiveAll(prTbObjects, false);
        SetActiveAll(Sels, false);
        if (subClick) subClick.SetActive(false);
    }

    public void ShowNextDialogue()
    {
        Debug.Log("현재 대사 index :" + dialogueIndex);
        prePos = dotcontroller.Position;
        PanelOff();

        if (dialogueIndex >= sub.currentDialogueList.Count)
        {
            Debug.Log("인덱스가 넘어감");
            DialEnd();
            return;
        }
        clickon();
        ScreenShield.On();
        var nextDial = sub.GetData(dialogueIndex);
        string scriptnumber = nextDial.ScriptNumber;
        string textType = nextDial.TextType;
        string actor = nextDial.Actor;
        string korText = nextDial.Text;
        int color = nextDial.Color;
        int determine; // dot:0, player:1

        var currentEntry = sub.GetData(dialogueIndex);
        dotcontroller.ChangeState(DotPatternState.Sub, currentEntry.DotAnim, prePos);

        switch (textType)
        {
            case "text":
                if (actor == "Dot")
                {
                    determine = 0;
                    if (korText.Contains("<nickname>") && pc)
                        korText = korText.Replace("<nickname>", pc.GetNickName());

                    GameObject selectedDot = PickBubble(dotObjects, color, dotcontroller.transform.position.x < mainCamera.transform.position.x ? "_L" : "_R");
                    if (selectedDot)
                    {
                        DotTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                        DotTextUI.text = korText;
                        if (scriptnumber.Contains("tutorial"))
                        {
                            if (subClick) subClick.SetActive(false);
                            TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                        }
                        else
                            dotballoon(selectedDot);
                    }
                }
                else // Player
                {
                    determine = 1;
                    if (korText.Contains("<nickname>") && pc)
                        korText = korText.Replace("<nickname>", pc.GetNickName());

                    GameObject selectedDot = PickBubble(prObjects, color, dotcontroller.transform.position.x < mainCamera.transform.position.x ? "_R" : "_L");
                    if (selectedDot)
                    {
                        PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                        PlayTextUI.text = korText;
                        if (scriptnumber.Contains("tutorial"))
                        {
                            if (subClick) subClick.SetActive(false);
                            TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                        }
                        else
                            playerballoon(selectedDot);
                    }
                }
                break;

            case "textbox": // Player 기준
                {
                    determine = 1;
                    GameObject selectedDot = PickBubble(prTbObjects, color, dotcontroller.transform.position.x < mainCamera.transform.position.x ? "_R" : "_L");
                    if (selectedDot)
                    {
                        PlayTextUI = selectedDot.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                        PlayTextUI.text = korText;
                        Resetinputfield(selectedDot);
                        if (subClick) subClick.SetActive(false);
                        if (scriptnumber.Contains("tutorial"))
                            TutoConditon(selectedDot, scriptnumber, determine, dialogueIndex);
                        else
                            playerballoon(selectedDot, true);
                    }
                    break;
                }

            case "selection": // Player 기준 (여러 버튼)
                {
                    if (subClick) subClick.SetActive(false); // 선택지는 오버레이가 가리지 않게

                    GameObject selectedDot = PickBubble(Sels, color, dotcontroller.transform.position.x < mainCamera.transform.position.x ? "_R" : "_L");
                    if (selectedDot)
                    {
                        selectedDot.SetActive(true);
                        // 버튼을 FadeIn에 넘기지 않음 (CanvasGroup만 제어해서 페이드 중 클릭 차단)
                        var cg = selectedDot.GetComponent<CanvasGroup>();
                        cg.alpha = 0f;
                        StartCoroutine(FadeCanvasGroup(cg, 0.5f, fadeIn: true));
                        ShowSelection(korText, selectedDot);
                    }
                    break;
                }
        }
    }

    // 말풍선 선택 헬퍼
    GameObject PickBubble(List<GameObject> list, int color, string sideSuffix)
    {
        // color: 0=Black, 1=Time(Dawn/Mor/Eve/Nig)
        if (color == 0)
        {
            var arr = list.FindAll(go => go.name.Contains("Black"));
            return arr.Find(go => go.name.Contains(sideSuffix));
        }
        else
        {
            string timeKey = gameManager.Time switch
            {
                "Dawn" => "Dawn",
                "Morning" => "Mor",
                "Evening" => "Eve",
                "Night" => "Nig",
                _ => "Mor"
            };
            var arr = list.FindAll(go => go.name.Contains(timeKey));
            return arr.Find(go => go.name.Contains(sideSuffix));
        }
    }

    // ============================= 위치/표시 =============================

    public void LocationSet(GameObject dotbub)
    {
        RectTransform speech = dotbub.GetComponent<RectTransform>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Vector3 worldPos = dotcontroller.transform.position;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out var anchoredPos);

        string currentAnim = dotcontroller.AnimKey;
        Vector2 offset = AnimBubbleDB.GetOffset(currentAnim);

        // 오프셋 적용
        if (dotcontroller.transform.position.x < mainCamera.transform.position.x) {
            anchoredPos.x += Mathf.Abs(offset.x);
            anchoredPos.y += offset.y;
        } 
        else
        {
            anchoredPos.x -= Mathf.Abs(offset.x);
            anchoredPos.y += offset.y;
        }
            

        speech.anchoredPosition = anchoredPos;
        dotbub.SetActive(true);

        Debug.Log($"Bubble pos: base={anchoredPos - offset}, offset={offset}, final={anchoredPos}, anim={currentAnim}");
    }



    public void PlayerLocationSet(GameObject dotbub)
    {
        RectTransform rect = dotbub.GetComponent<RectTransform>();
        bool left = dotcontroller.transform.position.x < mainCamera.transform.position.x;

        if (left)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(900, -400);
        }
        else
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            rect.anchoredPosition = new Vector2(-900, -400);
        }
        rect.gameObject.SetActive(true);
    }

    public void SelLocationSet(GameObject dotbub)
    {
        RectTransform rect = dotbub.GetComponent<RectTransform>();
        bool left = dotcontroller.transform.position.x < mainCamera.transform.position.x;

        if (left)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(300, 0);
        }
        else
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0, 0);
            rect.anchoredPosition = new Vector2(-900, 0);
        }
        rect.gameObject.SetActive(true);
    }

    // ============================= 진행/튜토 =============================

    public void dotballoon(GameObject selectedDot)
    {
        if (subClick) subClick.SetActive(true);
        if (subPanelObj) subPanelObj.SetActive(true);
        LocationSet(selectedDot);
        // 단일 버튼(넘김 버튼)만 페이드할 때는 button 전달
        var btn = subClick ? subClick.GetComponent<Button>() : null;
        //[DEBUG] 0.5f -> 0.01f
        StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.01f, btn));
        if (btn) RegisterNextButton(btn);
    }

    public void playerballoon(GameObject selectedDot, bool textbox = false)
    {
        if (subClick) subClick.SetActive(true);
        PlayerLocationSet(selectedDot);
        var btn = subClick ? subClick.GetComponent<Button>() : null;
        if (textbox)
        {
            subClick.SetActive(false);
            btn = selectedDot.transform.GetChild(0).GetChild(1).GetComponent<Button>();
        }
        //[DEBUG] 0.5f -> 0.01f
        StartCoroutine(FadeIn(selectedDot.GetComponent<CanvasGroup>(), 0.01f, btn));
        if (btn) RegisterNextButton(btn);
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
            if (!string.IsNullOrEmpty(currentEntry.DotAnim))
            {
                prePos = dotcontroller.Position;
                dotcontroller.ChangeState(DotPatternState.Sub, currentEntry.DotAnim, prePos);
            }

            if (int.TryParse(currentEntry.NextLineKey, out int nextLineKey))
            {
                int nextIndex = sub.currentDialogueList.FindIndex(
                    entry => (entry as SubDialogueEntry)?.LineKey == nextLineKey);

                if (nextIndex != -1) dialogueIndex = nextIndex;
                else { Debug.Log("넥스트 키가 잘못됨"); DialEnd(); return; }
            }
            else
            {
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

    public void TutoConditon(GameObject selectedDot, string scriptnumber, int determine, int index)
    {
        subTuto = gameManager.gameObject.GetComponent<SubTuto>();

        if (subTuto == null)
        {
            Debug.Log("SubTuto 컴포넌트를 찾을 수 없습니다.");
            if (determine == 0) dotballoon(selectedDot);
            else playerballoon(selectedDot);
            return;
        }

        Type type = subTuto.GetType();
        MethodInfo method = type.GetMethod(scriptnumber, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (method != null)
        {
            method.Invoke(subTuto, new object[] { selectedDot, determine, index });
            Debug.Log($"{scriptnumber} 메서드가 실행되었습니다.");
        }
        else
        {
            Debug.Log($"'{scriptnumber}' 메서드를 찾을 수 없습니다.");
            if (determine == 0) dotballoon(selectedDot);
            else playerballoon(selectedDot);
        }
    }

    public void Subcontinue()
    {
        subTuto = gameManager.gameObject.GetComponent<SubTuto>();
        if (subTuto)
        {
            subTuto.Subcontinue(this.dialogueIndex);
            prePos = dotcontroller.Position;
        }
        else
        {
            ShowNextDialogue();
        }
    }

    public void Resetinputfield(GameObject field)
    {
        TextMeshProUGUI inputfield = field.transform.GetChild(3).GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        Textinput = field.transform.GetChild(3).GetComponent<TMP_InputField>();
        Textinput.text = "";
        inputfield.text = "";
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnDisable() => PanelOff();
}
