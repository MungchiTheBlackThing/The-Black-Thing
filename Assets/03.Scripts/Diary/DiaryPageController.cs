using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiaryPageController : MonoBehaviour
{
  
    private int currentPageIndex = 0; // ���� ������ �ε���
    private bool isClick = false; // ��ư Ŭ�� ����
    private float clickTime = 0.0f; // Ŭ�� �ð�
    private int maxChapterIdx;

    [SerializeField]
    List<DiaryEntry> pages; // ������ GameObject���� ������ ����Ʈ
    [SerializeField]
    DiaryPage page;

    [SerializeField]
    float minClickTime = 0.3f; // �ٿ� Ŭ�� ���� �ð�

    [SerializeField]
    GameManager gameManger; // ���� ������ ��ư

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
                Debug.Log("�ۼ����� ���� �ϱ�");
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
                Debug.Log("�ϱ����� ������ ����");
            }
        }
    }
    private void UpdatePageVisibility()
    {
        // ��� �������� ��Ȱ��ȭ�ϰ� ���� �������� Ȱ��ȭ
        DiaryEntry entry = pages[currentPageIndex];

        page.UpdateDiaryPage(entry.title, entry.text, entry.subTitle, entry.diarySub);
        
    }
}
