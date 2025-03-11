using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SunDeathNote : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Transform pagesContainer; // ���������� ��� �ִ� �θ� Transform
    public int pageCount; // �������� �� ����
    public int currentPageIndex = 0; // ���� �������� �ε���
    private Vector2 dragStartPosition; // �巡�� ���� ��ġ
    private PlayerController playerController;

    private List<string> activeTexts = new List<string> {
        "�װ� �� ���� ���� �����ϰ� ���ٵ�� �ִ� �������� �־���.",   // <multiple(0)>
        "�ʴ� ���� ���� ������ ��ŭ �ɾ���԰�" // <multiple(1)>
    };

    private List<string> passiveTexts = new List<string> {
       "�װ� �� �̸��� ������ ������ �ҷ� �ִ� �������� �־���.",   // <multiple(0)>
       "�ʴ� ���� ������ ����� ������ �־���" // <multiple(1)>
    };
    private string highTexts = "�� �ȿ��� ���� ������ ����ϸ� �����ϰ� �޽��ϱ⵵ �߾�."; //<multiple(2)>

    private string lowTexts = "������ �뷧���� ��� �ָ� �� ������ ��ұ⵵ �߾�."; //<multiple(2)>

    private List<string> abcd = new List<string> {
        "������ �̼Ҷ� �ܻ�","������ ���� �Ƚ�ó","������ ������ ����","�̾��� ����� �Ҿ�" //<multiple(3)>
    };
    void OnEnable()
    {
        playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        pageCount = pagesContainer.childCount - 1; // �������� �� ���� (-1�� �ݱ� ��ư�� �����ϱ� ����)

        // �ʱ�ȭ: ù ��° �������� Ȱ��ȭ�ϰ�, �������� ��Ȱ��ȭ
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

        // passive, active�� ���� ������ �ؽ�Ʈ ����
        List<string> selectedTexts = (passive > active) ? passiveTexts : activeTexts;
        string multiple0 = selectedTexts[0];
        string multiple1 = selectedTexts[1];
        string multiple2 = (count >= 28) ? highTexts : lowTexts; 
        string multiple3 = abcd[deathnote]; 

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
                                           .Replace("<multiple3>", multiple3)
                                           .Replace("<name>", name);
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
            pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(false); // ���� ������ ��Ȱ��ȭ
            currentPageIndex++; // ���� ������ �ε����� �̵�
            pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(true); // ���� ������ Ȱ��ȭ
        }
        // �������� ���������Ͽ� ���� �������� �Ѿ ��
        else if (differenceX > 0 && currentPageIndex > 0)
        {
            pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(false); // ���� ������ ��Ȱ��ȭ
            currentPageIndex--; // ���� ������ �ε����� �̵�
            pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(true); // ���� ������ Ȱ��ȭ
        }
        if (currentPageIndex == pageCount - 1)
        {
            pagesContainer.GetChild(pageCount).gameObject.SetActive(true); //exit ��ư Ȱ��ȭ
        }
    }


    void OnDisable()
    {
        currentPageIndex = 0;
        pagesContainer.GetChild(pageCount).gameObject.SetActive(false);
        if (DeathNoteClick.checkdeath)
        {
            pagesContainer.GetChild(pageCount).gameObject.SetActive(true); //exit ��ư Ȱ��ȭ
        }
    }

    public void Onexitclick()
    {
        this.gameObject.SetActive(false);
    }
}