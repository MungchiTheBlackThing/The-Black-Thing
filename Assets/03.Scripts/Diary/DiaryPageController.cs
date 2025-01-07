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
    DiaryPage page;

    [SerializeField]
    float minClickTime = 0.3f; // �ٿ� Ŭ�� ���� �ð�

    [SerializeField]
    GameManager gameManger; // ���� ������ ��ư

    [SerializeField]
    Animator diaryAnim;

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
        diaryAnim.SetTrigger("Pressed");
    }

    public void ButtonUpNext()
    {
        diaryAnim.SetInteger("Direction", 1);
        
        if (clickTime >= minClickTime)
        {
            if (currentPageIndex < maxChapterIdx)
            {
                currentPageIndex++;
                diaryAnim.SetTrigger("Relased");
                UpdatePageVisibility();
            }
        }
    }
    public void ButtonUpPrev()
    {
        diaryAnim.SetInteger("Direction",0);

        if (clickTime >= minClickTime)
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                diaryAnim.SetTrigger("Relased");
                UpdatePageVisibility();
            }
        }
    }
    private void UpdatePageVisibility()
    {
        DiaryEntry entry = DataManager.Instance.DiaryData.DiaryEntry[currentPageIndex];
        int language = (int)gameManger.pc.GetLanguage();

        //서브 성공 여부 확인을 위한 부울 리스트
        List<bool> sub_success = gameManger.pc.GetSubPhase(gameManger.Chapter);
        page.UpdateDiaryPage(entry.title, entry.leftPage, entry.rightPage, entry.imagePath, language, sub_success);
    }
}
