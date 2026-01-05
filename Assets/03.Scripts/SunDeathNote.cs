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
	private LANGUAGE LANGUAGE;

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

	//------------En--------------

	private List<string> activeTextsen = new List<string> {
		"held me in your palm",   // <multiple(0)>
		"No, you dove into my pitch black" // <multiple(1)>
	};

	private List<string> passiveTextsen = new List<string> {
		"fondly called my name",   // <multiple(0)>
		"No, you strode into my pitch black" // <multiple(1)>
	};

	private List<string> abcden = new List<string> {
		"an afterimage of a pleasant daydream","a glimpse of an unlit haven"," a moment of loving darkness","an ember of faint courage" //<multiple(3)>
	};

	private string highTextsen = "and relaxed in it, picturing the dark side of the moon."; //<multiple(2)>
	private string lowTextsen = "and fell into step with me, humming melodies from bygone days."; //<multiple(2)>

	void OnEnable()
	{
		playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
		pageCount = pagesContainer.childCount - 1; // �������� �� ���� (-1�� �ݱ� ��ư�� �����ϱ� ����)
		LANGUAGE = playerController.GetLanguage();

		// �ʱ�ȭ: ù ��° �������� Ȱ��ȭ�ϰ�, �������� ��Ȱ��ȭ
		for (int i = 0; i < pageCount; i++)
		{
			pagesContainer.GetChild(i).gameObject.SetActive(i == 0);
		}

		UpdateAllText();
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
		List<string> selectedTexts;
		if (passive > active)
		{
			selectedTexts = isEnglish ? passiveTextsen : passiveTexts;
		}
		else
		{
			selectedTexts = isEnglish ? activeTextsen : activeTexts;
		}

		string multiple0 = selectedTexts[0];
		string multiple1 = selectedTexts[1];

		string multiple2;
		if (count >= 28)
		{
			multiple2 = isEnglish ? highTextsen : highTexts;
		}
		else
		{
			multiple2 = isEnglish ? lowTextsen : lowTexts;
		}

		string multiple3 = (isEnglish ? abcden : abcd)[deathnote];

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
