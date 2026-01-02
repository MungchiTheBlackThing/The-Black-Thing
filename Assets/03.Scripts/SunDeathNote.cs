using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SunDeathNote : MonoBehaviour, IDragHandler, IEndDragHandler
{
	public Transform pagesContainer; // 페이지들을 담고 있는 부모 Transform
	public int pageCount; // 페이지의 총 갯수
	public int currentPageIndex = 0; // 현재 페이지의 인덱스
	private Vector2 dragStartPosition; // 드래그 시작 위치
	private PlayerController playerController;
	private LANGUAGE LANGUAGE;

	private List<string> activeTexts = new List<string> {
		"네가 내 검은 몸을 다정하게 쓰다듬어 주는 순간들이 있었어.",   // <multiple(0)>
		"너는 나의 검정 안으로 성큼 걸어들어왔고" // <multiple(1)>
	};

	private List<string> passiveTexts = new List<string> {
		"네가 내 이름을 다정한 어조로 불러 주는 순간들이 있었어.",   // <multiple(0)>
		"너는 나의 검정에 어느새 스며들어 있었고" // <multiple(1)>
	};

	private string highTexts = "그 안에서 달의 뒤편을 상상하며 느긋하게 휴식하기도 했어."; //<multiple(2)>
	private string lowTexts = "오래된 노랫말을 들려 주며 내 옆에서 노닐기도 했어."; //<multiple(2)>

	private List<string> abcd = new List<string> {
		"찰나의 미소띤 잔상","찰나의 낯선 안식처","찰나의 다정한 암흑","미약한 용기의 불씨" //<multiple(3)>
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
		pageCount = pagesContainer.childCount - 1; // 페이지의 총 갯수 (-1은 닫기 버튼을 제외하기 위함)
		LANGUAGE = playerController.GetLanguage();

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

		bool isEnglish = LANGUAGE == LANGUAGE.ENGLISH;

		// passive, active에 따라 선택할 텍스트 결정 (언어 반영)
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
