using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Text.RegularExpressions;

public class MoonDeathNote : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Transform pagesContainer; // 페이지들을 담고 있는 부모 Transform
    public int pageCount; // 페이지의 총 갯수
    public int currentPageIndex = 0; // 현재 페이지의 인덱스
    private Vector2 dragStartPosition; // 드래그 시작 위치
    private PlayerController playerController;
    private LANGUAGE LANGUAGE;

    // private string activeTexts = "너는 기꺼이 너의 바다로 나를 이끌었고,"; //<multiple(0)>
    // private string passiveTexts = "너는 기꺼이 너의 바다에 나를 초대했고,";
    // private string lowTexts = "난 그 드넓은 수면에 몸을 띄운 채 잔잔히 파도를 거슬러 보았어."; //<multiple(1)>
    // private string highTexts = "난 마음껏 그 수면을 뛰놀며 자잘한 파도를 일렁여 보았어."; //<multiple(1)>

    // private List<string> abcd = new List<string> {
    //     "찰나의 미소띤 잔상","파도 속 낯선 안식처","비좁고 다정한 암흑","미약한 용기의 불씨" //<multiple(2)>
    // };

    // //--EN---------
    // private string activeTextsen = "pulled me into your sea,"; //<multiple(0)>
    // private string passiveTextsen = "called me into your sea,";
    // private string lowTextsen = "and I would run about its vast surface and create small ripples all around."; //<multiple(1)>
    // private string highTextsen = "and I would drift upon its vast surface and gently paddle against the waves."; //<multiple(1)>

    // private List<string> abcden = new List<string> {
    //     "an afterimage of a pleasant daydream","a glimpse of an unlit haven","a moment of loving darkness","an ember of faint courage" //<multiple(2)>
    // };

    void OnEnable()
    {
        playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        pageCount = pagesContainer.childCount - 1;

        // 초기화: 첫 페이지만 켜기
        for (int i = 0; i < pageCount; i++)
        {
            pagesContainer.GetChild(i).gameObject.SetActive(i == 0);
        }

        UpdateAllText();

        // 저장된 유서 읽음 상태를 런타임 플래그에 동기화
        DeathNoteClick.readDeathnote = (playerController.GetPlayerInfo().deathnoteState == 1);

        // 두 번째(이후) 읽기면 X(닫기) 버튼을 즉시 보여주기
        pagesContainer.GetChild(pageCount).gameObject.SetActive(DeathNoteClick.readDeathnote);
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
            Transform page = pagesContainer.GetChild(i);
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
        // 드래그 시작 지점 저장
        dragStartPosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그가 끝났을 때의 위치 저장
        Vector2 dragEndPosition = eventData.position;
        float differenceX = dragEndPosition.x - dragStartPosition.x;

        // 오른쪽으로 스와이프하여 다음 페이지로 넘어갈 때
        if (differenceX < 0 && currentPageIndex < pageCount - 1)
        {
            SetPage(currentPageIndex + 1);
        }
        // 왼쪽으로 스와이프하여 이전 페이지로 넘어갈 때
        else if (differenceX > 0 && currentPageIndex > 0)
        {
            SetPage(currentPageIndex - 1);
        }
        if (currentPageIndex == pageCount - 1)
        {
            pagesContainer.GetChild(pageCount).gameObject.SetActive(true); //exit 버튼 활성화
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
        pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(false);
        currentPageIndex = newIndex;
        pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(true);

        UpdateAllText();

        // exit 버튼 토글 규칙
        bool showExit = DeathNoteClick.readDeathnote || currentPageIndex == pageCount - 1;
        pagesContainer.GetChild(pageCount).gameObject.SetActive(showExit);
    }


    void OnDisable()
    {
        currentPageIndex = 0;
        pagesContainer.GetChild(pageCount).gameObject.SetActive(false);
        if (DeathNoteClick.readDeathnote)
        {
            pagesContainer.GetChild(pageCount).gameObject.SetActive(true); //exit 버튼 활성화
        }
    }

}
