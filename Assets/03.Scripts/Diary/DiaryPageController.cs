using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiaryPageController : MonoBehaviour
{
    [SerializeField] DiaryPage page;
    [SerializeField] GameManager gameManger;

    [SerializeField] float turnCooldown = 0.2f; 
    private int currentPageIndex = 0;
    private int maxChapterIdx;
    private bool isTurning = false;

    private void OnEnable()
    {
        maxChapterIdx = gameManger.Chapter - 1;
        currentPageIndex = maxChapterIdx;

        isTurning = false;
        UpdatePageVisibility(); 
    }

 
    public void NextPage()
    {
        if (isTurning) return;                      
        if (currentPageIndex >= maxChapterIdx) return; 
        StartCoroutine(TurnPage(+1));
    }

    public void PrevPage()
    {
        if (isTurning) return;
        if (currentPageIndex <= 0) return;
        StartCoroutine(TurnPage(-1));
    }

    private System.Collections.IEnumerator TurnPage(int dir)
    {
        isTurning = true;
        yield return new WaitForSeconds(turnCooldown);

        currentPageIndex += dir;
        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, maxChapterIdx);

        UpdatePageVisibility(); 

        isTurning = false;
    }

    private void UpdatePageVisibility()
    {
        DiaryEntry entry = DataManager.Instance.DiaryData.DiaryEntry[currentPageIndex];
        int language = (int)gameManger.pc.GetLanguage();

        AudioManager.instance.PlayOneShot(FMODEvents.instance.Pagesound, this.transform.position);

        // 서브 성공 여부 확인을 위한 부울 리스트
        List<bool> sub_success = gameManger.pc.GetSubPhase(gameManger.Chapter);
        page.UpdateDiaryPage(entry.title, entry.leftPage, entry.rightPage, entry.imagePath, language, sub_success);
    }
}
