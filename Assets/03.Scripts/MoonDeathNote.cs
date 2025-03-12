using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MoonDeathNote : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Transform pagesContainer; // 페이지들을 담고 있는 부모 Transform
    public int pageCount; // 페이지의 총 갯수
    public int currentPageIndex = 0; // 현재 페이지의 인덱스
    private Vector2 dragStartPosition; // 드래그 시작 위치
    private PlayerController playerController;

    private string activeTexts = "너는 기꺼이 너의 바다로 나를 이끌었고,"; //<multiple(0)>

    private string passiveTexts = "너는 기꺼이 너의 바다에 나를 초대했고,";

    private string lowTexts = "난 그 드넓은 수면에 몸을 띄운 채 잔잔히 파도를 거슬러 보았어."; //<multiple(1)>

    private string highTexts = "난 마음껏 그 수면을 뛰놀며 자잘한 파도를 일렁여 보았어."; //<multiple(1)>

    private List<string> abcd = new List<string> {
        "찰나의 미소띤 잔상","파도 속 낯선 안식처","비좁고 다정한 암흑","미약한 용기의 불씨" //<multiple(2)>
    };
    void OnEnable()
    {
        playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        pageCount = pagesContainer.childCount - 1; // 페이지의 총 갯수 (-1은 닫기 버튼을 제외하기 위함)

        // 초기화: 첫 번째 페이지를 활성화하고, 나머지는 비활성화
        for (int i = 0; i < pageCount; i++)
        {
            pagesContainer.GetChild(i).gameObject.SetActive(i == 0);
        }
        UpdateAllText();
    }

    private void UpdateAllText()
    {
        int passive = playerController.GetArcheType().passive;
        int active = playerController.GetArcheType().active;
        int deathnote = playerController.GetArcheType().deathnote;
        int count = playerController.GetRewards().Count;
        string name = playerController.GetNickName();

        // passive, active에 따라 선택할 텍스트 결정
        string selectedTexts = (passive > active) ? passiveTexts : activeTexts;
        string multiple0 = selectedTexts;
        string multiple1 = (count >= 28) ? highTexts : lowTexts;
        string multiple2 = abcd[deathnote];

        for (int i = 0; i < pageCount; i++)
        {
            Transform page = pagesContainer.GetChild(i);
            TextMeshProUGUI[] texts = page.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (TextMeshProUGUI textMesh in texts)
            {
                string modifiedText = textMesh.text;
                modifiedText = modifiedText.Replace("<multiple0>", multiple0)
                                           .Replace("<multiple1>", multiple1)
                                           .Replace("<multiple2>", multiple2)
                                           .Replace("<name>", name);
                textMesh.text = modifiedText;
            }
        }
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
            pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(false); // 현재 페이지 비활성화
            currentPageIndex++; // 다음 페이지 인덱스로 이동
            pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(true); // 다음 페이지 활성화
        }
        // 왼쪽으로 스와이프하여 이전 페이지로 넘어갈 때
        else if (differenceX > 0 && currentPageIndex > 0)
        {
            pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(false); // 현재 페이지 비활성화
            currentPageIndex--; // 이전 페이지 인덱스로 이동
            pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(true); // 이전 페이지 활성화
        }
        if (currentPageIndex == pageCount - 1)
        {
            pagesContainer.GetChild(pageCount).gameObject.SetActive(true); //exit 버튼 활성화
        }
    }


    void OnDisable()
    {
        currentPageIndex = 0;
        pagesContainer.GetChild(pageCount).gameObject.SetActive(false);
        if (DeathNoteClick.checkdeath)
        {
            pagesContainer.GetChild(pageCount).gameObject.SetActive(true); //exit 버튼 활성화
        }
    }

    public void Onexitclick()
    {
        this.gameObject.SetActive(false);
    }
}