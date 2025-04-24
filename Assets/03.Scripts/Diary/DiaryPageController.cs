using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DiaryPageController : MonoBehaviour
{
  
    int currentPageIndex = 0; // ���� ������ �ε���
    bool isLeftClick = false; // ��ư Ŭ�� ����
    bool isRightClick = false; // ��ư Ŭ�� ����

    float leftClickTime = 0.0f; // Ŭ�� �ð�
    float rightClickTime = 0.0f;
    private int maxChapterIdx;

    [SerializeField]
    DiaryPage page;

    [SerializeField]
    float minClickTime = 0.3f; // �ٿ� Ŭ�� ���� �ð�

    [SerializeField]
    GameManager gameManger; // ���� ������ ��ư

    [SerializeField]
    Animator leftDiaryAnim;

    [SerializeField]
    Animator rightDiaryAnim;


    private void OnEnable()
    {
        maxChapterIdx = gameManger.Chapter - 1;
        currentPageIndex = maxChapterIdx;
        isRightClick = false;
        isLeftClick = false;
        leftClickTime = 0.0f;
        rightClickTime = 0.0f;
        UpdatePageVisibility();
    }

    private void Update()
    {
        if (isLeftClick)
        {
            leftClickTime += Time.deltaTime;
        }
        else
        {
            leftClickTime = 0.0f;
            isLeftClick = false;
        }

        if(isRightClick)
        {
            rightClickTime += Time.deltaTime;
        }
        else
        {
            rightClickTime = 0.0f;
            isRightClick = false;
        }
    }

    public void LeftButtonDown()
    {
        isLeftClick = true;
        leftDiaryAnim.SetBool("isPressed", isLeftClick);
    }

    public void RightButtonDown()
    {
        isRightClick = true;
        rightDiaryAnim.SetBool("isPressed", isRightClick);
    }

    public void ButtonUpNext()
    {
        isRightClick = false;

        if (rightClickTime >= minClickTime)
        {
            if (currentPageIndex < maxChapterIdx)
            {
                currentPageIndex++;
                rightDiaryAnim.SetTrigger("Relased");
                rightDiaryAnim.SetBool("isPressed", isRightClick);
                UpdatePageVisibility();
            }
            else
            {
                rightDiaryAnim.SetBool("isPressed", isRightClick);
            }
        }
        else
        {
            rightDiaryAnim.SetBool("isPressed", isRightClick);
        }
    }

    public void ButtonUpPrev()
    {
        isLeftClick = false;

        if (leftClickTime >= minClickTime)
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                leftDiaryAnim.SetTrigger("Relased");
                UpdatePageVisibility();
            }
            else
            {
                leftDiaryAnim.SetBool("isPressed", isLeftClick);
            }
        }
        else
        {
            leftDiaryAnim.SetBool("isPressed", isLeftClick);
        }

    }
    private void UpdatePageVisibility()
    {
        DiaryEntry entry = DataManager.Instance.DiaryData.DiaryEntry[currentPageIndex];
        int language = (int)gameManger.pc.GetLanguage();
        AudioManager.instance.PlayOneShot(FMODEvents.instance.Pagesound, this.transform.position);
        //서브 성공 여부 확인을 위한 부울 리스트
        List<bool> sub_success = gameManger.pc.GetSubPhase(gameManger.Chapter);
        page.UpdateDiaryPage(entry.title, entry.leftPage, entry.rightPage, entry.imagePath, language, sub_success);
    }
}
