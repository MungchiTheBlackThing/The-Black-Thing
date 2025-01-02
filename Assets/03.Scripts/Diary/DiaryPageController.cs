using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiaryPageController : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> pages; // 페이지 GameObject들을 저장할 리스트

    private int currentPageIndex = 0; // 현재 페이지 인덱스

    [SerializeField]
    private float minClickTime = 0.3f; // 꾸욱 클릭 기준 시간

    private bool isClick = false; // 버튼 클릭 여부
    private float clickTime = 0.0f; // 클릭 시간

    [SerializeField]
    private GameManager gameManger; // 다음 페이지 버튼

    public int chapter;
    private void Start()
    {
        foreach (Transform child in transform)
        {
            pages.Add(child.gameObject);
        }

        chapter = gameManger.Chapter - 1;
        // 초기화: 첫 페이지 활성화, 나머지 비활성화
        UpdatePageVisibility();
        // 버튼 이벤트 추가
    }

    private void Update()
    {
        if (isClick)
        {
            clickTime += Time.deltaTime;
        }
        else
        {
            clickTime = 0.0f;
        }
    }

    public void ButtonDown()
    {
        isClick = true;
    }

    public void ButtonUpNext()
    {
        isClick = false;

        if (clickTime >= minClickTime)
        {
            if (currentPageIndex < chapter)
            {
                currentPageIndex++;
                UpdatePageVisibility();
            }
            else
            {
                Debug.Log("작성되지 않은 일기");
            }
        }
    }
    public void ButtonUpPrev()
    {
        isClick = false;

        if (clickTime >= minClickTime)
        {
            Debug.Log("Long Click");
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                UpdatePageVisibility();
            }
            else
            {
                Debug.Log("일기장은 음수가 없다");
            }
        }
    }
    private void UpdatePageVisibility()
    {
        // 모든 페이지를 비활성화하고 현재 페이지만 활성화
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(i == currentPageIndex);
            return;
        }
        Debug.Log(currentPageIndex + 1);
    }
}
