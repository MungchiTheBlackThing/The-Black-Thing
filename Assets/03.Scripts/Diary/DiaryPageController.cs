using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DiaryPageController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] DiaryPage page;
    [SerializeField] GameManager gameManager;

    [SerializeField] float turnCooldown = 0.2f; 
    private int currentPageIndex = 0;
    private int maxChapterIdx;
    private bool isTurning = false;

    
    public Image _dragRaycastImage;

    private void Awake()
    {
        _dragRaycastImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (_dragRaycastImage != null)
            _dragRaycastImage.raycastTarget = true;

        Debug.Log($"[DiaryPageController] OnEnable. GM Chapter: {gameManager.Chapter}, Pattern: {gameManager.Pattern}");
        maxChapterIdx = gameManager.Chapter - 1;
        if (gameManager.Pattern <= GamePatternState.Writing)
        {
            maxChapterIdx--;
        }
        Debug.Log($"[DiaryPageController] Calculated maxChapterIdx (before clamp): {maxChapterIdx}");

        if (maxChapterIdx < 0) maxChapterIdx = 0;
        currentPageIndex = maxChapterIdx;

        Debug.Log($"[DiaryPageController] Final maxChapterIdx: {maxChapterIdx}, currentPageIndex: {currentPageIndex}");

        isTurning = false;
        UpdatePageVisibility(); 
    }

    private void OnDisable()
    {
        if (_dragRaycastImage != null)
            _dragRaycastImage.raycastTarget = false;
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

    public void OnDrag(PointerEventData eventData) { }

    [SerializeField] float minSwipeRatio = 0.1f;

    public void OnEndDrag(PointerEventData eventData)
    {
        TouchUtility.HandleHorizontalSwipe(eventData, NextPage, PrevPage,
            verticalThresholdRatio: 0.8f, minSwipeScreenRatio: minSwipeRatio);
    }

    private IEnumerator TurnPage(int dir)
    {
        isTurning = true;
        yield return new WaitForSeconds(turnCooldown);

        currentPageIndex += dir;
        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, maxChapterIdx);
        Debug.Log($"[DiaryPageController] TurnPage. New Index: {currentPageIndex}");

        UpdatePageVisibility(); 

        isTurning = false;
    }

    private void UpdatePageVisibility()
    {
        if (DataManager.Instance.DiaryData == null || DataManager.Instance.DiaryData.DiaryEntry == null)
        {
            Debug.LogError("[DiaryPageController] DiaryData is null or empty.");
            return;
        }

        if (currentPageIndex >= DataManager.Instance.DiaryData.DiaryEntry.Count)
        {
            Debug.LogError($"[DiaryPageController] currentPageIndex {currentPageIndex} exceeds DiaryEntry count {DataManager.Instance.DiaryData.DiaryEntry.Count}");
            return;
        }

        DiaryEntry entry = DataManager.Instance.DiaryData.DiaryEntry[currentPageIndex];
        Debug.Log($"[DiaryPageController] UpdatePageVisibility. Entry ID: {entry.id}, TitleKey: {entry.titleKey}");
        //int language = (int)gameManager.pc.GetLanguage(); 패키지 교체

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.Pagesound, this.transform.position);

        // 현재 보고 있는 페이지(entry.id)의 챕터에 맞는 성공 여부를 가져오도록 수정
        List<bool> sub_success = gameManager.pc.GetSubPhase(entry.id);
        Debug.Log($"[DiaryPageController] SubPhase success list count: {(sub_success != null ? sub_success.Count : 0)}");

        // 변경된 UpdateDiaryPage 메서드 호출
        page.UpdateDiaryPage(entry.titleKey, entry.leftPageKey, entry.rightPage, entry.imagePath, sub_success);
    }
}
