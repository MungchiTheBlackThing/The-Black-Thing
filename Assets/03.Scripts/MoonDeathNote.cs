using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MoonDeathNote : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Transform pagesContainer; // ���������� ��� �ִ� �θ� Transform
    public int pageCount; // �������� �� ����
    public int currentPageIndex = 0; // ���� �������� �ε���
    private Vector2 dragStartPosition; // �巡�� ���� ��ġ
    private PlayerController playerController;
    private LANGUAGE LANGUAGE;

    private string activeTexts = "�ʴ� �Ⲩ�� ���� �ٴٷ� ���� �̲�����,"; //<multiple(0)>
    private string passiveTexts = "�ʴ� �Ⲩ�� ���� �ٴٿ� ���� �ʴ��߰�,";
    private string lowTexts = "�� �� ����� ���鿡 ���� ��� ä ������ �ĵ��� �Ž��� ���Ҿ�."; //<multiple(1)>
    private string highTexts = "�� ������ �� ������ �ٳ�� ������ �ĵ��� �Ϸ��� ���Ҿ�."; //<multiple(1)>

    private List<string> abcd = new List<string> {
        "������ �̼Ҷ� �ܻ�","�ĵ� �� ���� �Ƚ�ó","������ ������ ����","�̾��� ����� �Ҿ�" //<multiple(2)>
    };

    //--EN---------
    private string activeTextsen = "pulled me into your sea,"; //<multiple(0)>
    private string passiveTextsen = "called me into your sea,";
    private string lowTextsen = "and I would run about its vast surface and create small ripples all around."; //<multiple(1)>
    private string highTextsen = "and I would drift upon its vast surface and gently paddle against the waves."; //<multiple(1)>

    private List<string> abcden = new List<string> {
        "an afterimage of a pleasant daydream","a glimpse of an unlit haven","a moment of loving darkness","an ember of faint courage" //<multiple(2)>
    };

    void OnEnable()
    {
        playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        pageCount = pagesContainer.childCount - 1; 
        LANGUAGE = playerController.GetLanguage();

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

        bool isEnglish = LANGUAGE == LANGUAGE.ENGLISH;

        // passive, active�� ���� ������ �ؽ�Ʈ ���� (��� �ݿ�)
        string multiple0;
        if (passive > active)
        {
            multiple0 = isEnglish ? passiveTextsen : passiveTexts;
        }
        else
        {
            multiple0 = isEnglish ? activeTextsen : activeTexts;
        }

        string multiple1;
        if (count >= 28)
        {
            multiple1 = isEnglish ? highTextsen : highTexts;
        }
        else
        {
            multiple1 = isEnglish ? lowTextsen : lowTexts;
        }

        string multiple2 = (isEnglish ? abcden : abcd)[deathnote];

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
                                           .Replace("<nickname>", name);
                textMesh.text = modifiedText;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // �巡�� ���� ���� ����
        dragStartPosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // �巡�װ� ������ ���� ��ġ ����
        Vector2 dragEndPosition = eventData.position;
        float differenceX = dragEndPosition.x - dragStartPosition.x;

        // ���������� ���������Ͽ� ���� �������� �Ѿ ��
        if (differenceX < 0 && currentPageIndex < pageCount - 1)
        {
            SetPage(currentPageIndex + 1);
        }
        // �������� ���������Ͽ� ���� �������� �Ѿ ��
        else if (differenceX > 0 && currentPageIndex > 0)
        {
            SetPage(currentPageIndex - 1);
        }
        if (currentPageIndex == pageCount - 1)
        {
            pagesContainer.GetChild(pageCount).gameObject.SetActive(true); //exit ��ư Ȱ��ȭ
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
            pagesContainer.GetChild(pageCount).gameObject.SetActive(true); //exit ��ư Ȱ��ȭ
        }
    }

}
