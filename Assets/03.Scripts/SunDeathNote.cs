using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Text.RegularExpressions;

public class SunDeathNote : MonoBehaviour, IDragHandler, IEndDragHandler
{
	public Transform pagesContainer; // 페이지들을 담고 있는 부모 Transform
	public int pageCount; // 페이지의 총 갯수
	public int currentPageIndex = 0; // 현재 페이지의 인덱스
	private Vector2 dragStartPosition; // 드래그 시작 위치
	private PlayerController playerController;
	private LANGUAGE LANGUAGE;

	// private List<string> activeTexts = new List<string> {
	// 	"네가 내 검은 몸을 다정하게 쓰다듬어 주는 순간들이 있었어.",   // <multiple(0)>
	// 	"너는 나의 검정 안으로 성큼 걸어들어왔고" // <multiple(1)>
	// };

	// private List<string> passiveTexts = new List<string> {
	// 	"네가 내 이름을 다정한 어조로 불러 주는 순간들이 있었어.",   // <multiple(0)>
	// 	"너는 나의 검정에 어느새 스며들어 있었고" // <multiple(1)>
	// };

	// private string highTexts = "그 안에서 달의 뒤편을 상상하며 느긋하게 휴식하기도 했어."; //<multiple(2)>
	// private string lowTexts = "오래된 노랫말을 들려 주며 내 옆에서 노닐기도 했어."; //<multiple(2)>

	// private List<string> abcd = new List<string> {
	// 	"찰나의 미소띤 잔상","찰나의 낯선 안식처","찰나의 다정한 암흑","미약한 용기의 불씨" //<multiple(3)>
	// };

	// //------------En--------------

	// private List<string> activeTextsen = new List<string> {
	// 	"held me in your palm",   // <multiple(0)>
	// 	"No, you dove into my pitch black" // <multiple(1)>
	// };

	// private List<string> passiveTextsen = new List<string> {
	// 	"fondly called my name",   // <multiple(0)>
	// 	"No, you strode into my pitch black" // <multiple(1)>
	// };

	// private List<string> abcden = new List<string> {
	// 	"an afterimage of a pleasant daydream","a glimpse of an unlit haven"," a moment of loving darkness","an ember of faint courage" //<multiple(3)>
	// };

	// private string highTextsen = "and relaxed in it, picturing the dark side of the moon."; //<multiple(2)>
	// private string lowTextsen = "and fell into step with me, humming melodies from bygone days."; //<multiple(2)>


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
		string multiple0 = GetLocalizedString("SunDeath", multiple0Key);

		// multiple1: passive/active에 따라 로컬라이징 테이블에서 가져오기
		string multiple1Key = (passive > active) ? "multiple1_passive" : "multiple1_active";
		string multiple1 = GetLocalizedString("SunDeath", multiple1Key);

		// multiple2: count에 따라 로컬라이징 테이블에서 가져오기
		string multiple2Key = (count >= 28) ? "multiple2_high" : "multiple2_low";
		string multiple2 = GetLocalizedString("SunDeath", multiple2Key);

		// multiple3: deathnote 인덱스에 따라 로컬라이징 테이블에서 가져오기
		string multiple3Key = $"multiple3_{deathnote}";
		string multiple3 = GetLocalizedString("SunDeath", multiple3Key);

		for (int i = 0; i < pageCount; i++)
		{
			Transform page = pagesContainer.GetChild(i);
			TextMeshProUGUI[] texts = page.GetComponentsInChildren<TextMeshProUGUI>(true);

			foreach (TextMeshProUGUI textMesh in texts)
			{
				// 먼저 SunDeath_1~4 키로 로컬라이징된 텍스트 가져오기
				string pageKey = $"SunDeath_{i + 1}";
				string localizedText = GetLocalizedString("SunDeath", pageKey);
				
				// 로컬라이징 텍스트가 있으면 사용, 없으면 원본 텍스트 사용
				string baseText = string.IsNullOrEmpty(localizedText) ? textMesh.text : localizedText;
				
				// 조건부 텍스트 패턴 처리: (passive: ...)(active: ...)
				baseText = ProcessConditionalText(baseText, passive > active);
				
				// 변수 치환
				string modifiedText = baseText.Replace("<multiple0>", multiple0)
											 .Replace("<multiple1>", multiple1)
											 .Replace("<multiple2>", multiple2)
											 .Replace("<multiple3>", multiple3)
											 .Replace("<name>", name)
											 .Replace("<nickname>", name);
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
				Debug.LogWarning($"[SunDeathNote] 로컬라이징 키를 찾을 수 없음: {tableName}/{key}");
			}
			return localized ?? string.Empty;
		}
		catch (System.Exception e)
		{
			Debug.LogError($"[SunDeathNote] 로컬라이징 오류 ({tableName}/{key}): {e.Message}");
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

		/// 오른쪽으로 스와이프하여 다음 페이지로 넘어갈 때
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

        // 페이지 전환 시 텍스트 업데이트
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
