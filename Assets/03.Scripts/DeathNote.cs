using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class DeathNote : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Transform pagesContainer; // 페이지들을 담고 있는 부모 Transform
    public int pageCount; // 페이지의 총 갯수
    public int currentPageIndex = 0; // 현재 페이지의 인덱스
    private Vector2 dragStartPosition; // 드래그 시작 위치

    void OnEnable()
    {
        pageCount = pagesContainer.childCount - 1; // 페이지의 총 갯수 (-1은 닫기 버튼을 제외하기 위함)

        // 초기화: 첫 번째 페이지를 활성화하고, 나머지는 비활성화
        for (int i = 0; i < pageCount; i++)
        {
            pagesContainer.GetChild(i).gameObject.SetActive(i == 0);
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