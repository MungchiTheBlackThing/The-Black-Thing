using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DiarySubEntry
{
    [SerializeField]
    public string text;
    [SerializeField]
    public Sprite image;
}
[Serializable]
public class DiaryEntry
{
    [SerializeField]
    public string title;
    [SerializeField]
    public string text;
    [SerializeField]
    public string subTitle;
   
    [SerializeField] 
    public DiarySubEntry[] diarySub;
}

public class DiaryPageController : MonoBehaviour
{
  
    private int currentPageIndex = 0; // 현재 페이지 인덱스
    private bool isClick = false; // 버튼 클릭 여부
    private float clickTime = 0.0f; // 클릭 시간
    private int maxChapterIdx;

    [SerializeField]
    List<DiaryEntry> pages; // 페이지 GameObject들을 저장할 리스트
    [SerializeField]
    DiaryPage page;

    [SerializeField]
    float minClickTime = 0.3f; // 꾸욱 클릭 기준 시간

    [SerializeField]
    GameManager gameManger; // 다음 페이지 버튼

    private void OnEnable()
    {
        maxChapterIdx = gameManger.Chapter - 1;
        currentPageIndex = maxChapterIdx;
        isClick = false;
        clickTime = 0.0f;

        UpdatePageVisibility();
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
            if (currentPageIndex < maxChapterIdx)
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
        DiaryEntry entry = pages[currentPageIndex];

        page.UpdateDiaryPage(entry.title, entry.text, entry.subTitle, entry.diarySub);
        
    }
}
