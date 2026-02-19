using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Text.RegularExpressions;

public class MoonDeathNote : MonoBehaviour, IDragHandler, IEndDragHandler
{

    [SerializeField] private Transform pagesRoot;   // 페이지만 담는 부모(1~4페이지만)
    [SerializeField] private GameObject exitButton; // X 버튼
    public Transform pagesContainer; // 페이지들을 담고 있는 부모 Transform
    public int pageCount; // 페이지의 총 갯수
    public int currentPageIndex = 0; // 현재 페이지의 인덱스
    private Vector2 dragStartPosition; // 드래그 시작 위치
    private PlayerController playerController;
    private LANGUAGE LANGUAGE;

    [Header("Nav Buttons")]
    [SerializeField] private Button NextPageBut;
    [SerializeField] private Button BackPageBut;

    void OnEnable()
    {
        playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        pageCount = pagesRoot.childCount;
        currentPageIndex = 0;

        // 첫 페이지만 켜기
        for (int i = 0; i < pageCount; i++)
        {
            pagesRoot.GetChild(i).gameObject.SetActive(i == 0);
        }

        UpdateAllText();

        DeathNoteClick.readDeathnote = (playerController.GetPlayerInfo().deathnoteState == 1);

        // X 버튼은 별도 오브젝트로 토글
        if (exitButton != null)
            exitButton.SetActive(DeathNoteClick.readDeathnote);

        // 버튼 리스너
        if (NextPageBut != null)
        {
            NextPageBut.onClick.RemoveAllListeners();
            NextPageBut.onClick.AddListener(NextPage);
        }
        if (BackPageBut != null)
        {
            BackPageBut.onClick.RemoveAllListeners();
            BackPageBut.onClick.AddListener(PrevPage);
        }

        RefreshNavButtons();

    }

    private void UpdateAllText()
    {
        int passive = playerController.GetArcheType().passive;
        int active = playerController.GetArcheType().active;
        int deathnote = playerController.GetArcheType().deathnote;
        int count = playerController.GetRewards().Count;
        string name = playerController.GetNickName();

        // multiple0: passive/active에 따라 로컬라이징 테이블에서 가져오기
        string multiple0Key = (passive > active) ? "multiple0_passive" : "multiple0_active";
        string multiple0 = GetLocalizedString("MoonDeath", multiple0Key);

        // multiple1: count에 따라 로컬라이징 테이블에서 가져오기
        string multiple1Key = (count >= 28) ? "multiple1_high" : "multiple1_low";
        string multiple1 = GetLocalizedString("MoonDeath", multiple1Key);

        // multiple2: deathnote 인덱스에 따라 로컬라이징 테이블에서 가져오기
        string multiple2Key = $"multiple2_{deathnote}";
        string multiple2 = GetLocalizedString("MoonDeath", multiple2Key);

        for (int i = 0; i < pageCount; i++)
        {
            Transform page = pagesRoot.GetChild(i);
            TextMeshProUGUI[] texts = page.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (TextMeshProUGUI textMesh in texts)
            {
                // 먼저 MoonDeath1~4 키로 로컬라이징된 텍스트 가져오기
                string pageKey = $"MoonDeath{i + 1}";
                string localizedText = GetLocalizedString("MoonDeath", pageKey);

                // 로컬라이징 텍스트가 있으면 사용, 없으면 원본 텍스트 사용
                string baseText = string.IsNullOrEmpty(localizedText) ? textMesh.text : localizedText;

                // 조건부 텍스트 패턴 처리: (passive: ...)(active: ...)
                baseText = ProcessConditionalText(baseText, passive > active);

                // 변수 치환
                string modifiedText = baseText.Replace("<multiple0>", multiple0)
                                              .Replace("<multiple1>", multiple1)
                                              .Replace("<multiple2>", multiple2)
                                              .Replace("<name>", name)
                                              .Replace("<nickname>", name); // <nickname>도 지원
                textMesh.text = modifiedText;
            }
        }
    }

    private void RefreshNavButtons()
    {

        bool isFirst = (currentPageIndex <= 0);
        bool isLast  = (currentPageIndex >= pageCount - 1);

        if (BackPageBut != null) BackPageBut.gameObject.SetActive(!isFirst);
        if (NextPageBut != null) NextPageBut.gameObject.SetActive(!isLast);
    }


    private string GetLocalizedString(string tableName, string key)
    {
        try
        {
            string localized = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
            if (string.IsNullOrEmpty(localized))
            {
                Debug.LogWarning($"[MoonDeathNote] 로컬라이징 키를 찾을 수 없음: {tableName}/{key}");
            }
            return localized ?? string.Empty;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MoonDeathNote] 로컬라이징 오류 ({tableName}/{key}): {e.Message}");
            return string.Empty;
        }
    }

    // 조건부 텍스트 패턴 처리: (passive: ...)(active: ...) -> passive > active면 첫 번째, 아니면 두 번째
    private string ProcessConditionalText(string text, bool isPassive)
    {
        // (passive: ...)(active: ...) 패턴 찾기
        Regex regex = new Regex(@"\(passive:\s*([^)]+)\)\(active:\s*([^)]+)\)");
        return regex.Replace(text, match =>
        {
            string passiveText = match.Groups[1].Value.Trim();
            string activeText = match.Groups[2].Value.Trim();
            return isPassive ? passiveText : activeText;
        });
    }

    public void OnDrag(PointerEventData eventData)
    {
 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 start = eventData.pressPosition;
        Vector2 end = eventData.position;       

        float dx = end.x - start.x;
        float dy = end.y - start.y;

        // 1) 각도(대각선/스크롤) 필터 강화: dy가 dx의 0.6배보다 크면 취소
        if (Mathf.Abs(dy) > Mathf.Abs(dx) * 0.6f) return;

        // 2) 거리 기준을 화면 비율로: 화면 너비의 15% 미만이면 취소
        float minSwipe = Screen.width * 0.15f;
        if (Mathf.Abs(dx) < minSwipe) return;

        // 오른쪽으로 스와이프하여 다음 페이지로 넘어갈 때
        if (dx < 0f && currentPageIndex < pageCount - 1)
        {
            SetPage(currentPageIndex + 1);
        }
        // 왼쪽으로 스와이프하여 이전 페이지로 넘어갈 때
        else if (dx > 0f && currentPageIndex > 0)
        {
            SetPage(currentPageIndex - 1);
        }

        if (currentPageIndex == pageCount - 1)
        {
            if (exitButton != null) exitButton.SetActive(true); //exit 버튼 활성화
        }
    }

    public void NextPage()
    {
        if (currentPageIndex >= pageCount - 1) return;
        SetPage(currentPageIndex + 1);
    }

    public void PrevPage()
    {
        if (currentPageIndex <= 0) return;
        SetPage(currentPageIndex - 1);
    }

    private void SetPage(int newIndex)
    {
            pagesRoot.GetChild(currentPageIndex).gameObject.SetActive(false);
            currentPageIndex = newIndex;
            pagesRoot.GetChild(currentPageIndex).gameObject.SetActive(true);

            UpdateAllText();

            bool showExit = DeathNoteClick.readDeathnote || currentPageIndex == pageCount - 1;
            if (exitButton != null) exitButton.SetActive(showExit);

            RefreshNavButtons();
    }

    void OnDisable()
    {
        currentPageIndex = 0;
        if (exitButton != null)
    	    exitButton.SetActive(DeathNoteClick.readDeathnote);
    }
}
