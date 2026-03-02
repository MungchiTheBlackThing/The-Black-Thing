using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MoonoteController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private List<GameObject> pages; // 페이지 GameObject들을 저장할 리스트

    private int currentPageIndex = 0; // 현재 페이지 인덱스

    [SerializeField] private Button next;
    [SerializeField] private Button prev;

    [SerializeField]
    private Button exit;

    // public Image nximage;
    // public Image pvimage;

    private void Start()
    {
      
        if (transform.childCount > 0)
            foreach (Transform child in transform) pages.Add(child.gameObject);
        else
            for (int i = 0; i < transform.GetSiblingIndex(); i++)
                pages.Add(transform.parent.GetChild(i).gameObject);
        UpdatePageVisibility();
    }

    // 첫 페이지로 리셋되도록
    public void ResetToFirstPage()
    {
        currentPageIndex = 0;
        UpdatePageVisibility();
    }

    public void OnDrag(PointerEventData eventData) { }

    public void OnEndDrag(PointerEventData eventData)
    {
        var ui = GetComponentInParent<MoonnoteUI>();
        TouchUtility.HandleHorizontalSwipe(eventData,
            () => { ButtonUpNext(); ui?.NextPage(); },
            () => { ButtonUpPrev(); ui?.PreviousPage(); },
            verticalThresholdRatio: 0.8f, minSwipeScreenRatio: 0.1f);
    }
    public void ButtonUpNext()
    {
        if (currentPageIndex < pages.Count - 1)
        {
            currentPageIndex++;
            UpdatePageVisibility();
        }
        else
        {
            Debug.Log("작성되지 않은 일기");
        }
    }
    public void ButtonUpPrev()
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
    private void UpdatePageVisibility()
    {
        // 모든 페이지를 비활성화하고 현재 페이지만 활성화
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(i == currentPageIndex);
        }
        if (currentPageIndex == 0)
        {
            if (next != null)
            {
                Color color = next.targetGraphic.color;
                color.a = 130.0f;
                next.targetGraphic.color = color;
            }
        }
        else if (currentPageIndex == pages.Count - 1)
        {
            if (prev != null)
            {
                Color color = prev.targetGraphic.color;
                color.a = 130.0f;
                prev.targetGraphic.color = color;
            }
            if (exit != null) exit.gameObject.SetActive(true);
        }
        else
        {
            if (next != null && prev != null)
            {
                Color color = next.targetGraphic.color;
                color.a = 255.0f;
                next.targetGraphic.color = color;
                prev.targetGraphic.color = color;
            }
        }
    }
}
