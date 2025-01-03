using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoonoteController : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> pages; // 페이지 GameObject들을 저장할 리스트

    private int currentPageIndex = 0; // 현재 페이지 인덱스

    [SerializeField]
    private Button next;
   

    [SerializeField]
    private Button prev;

    [SerializeField]
    private Button exit;

    public Image nximage;
    public Image pvimage;

    private void Start()
    {
        exit.gameObject.SetActive(false);
        nximage = next.gameObject.GetComponent<Image>();
        pvimage = prev.gameObject.GetComponent<Image>();
        foreach (Transform child in transform)
        {
            pages.Add(child.gameObject);
        }
        // 초기화: 첫 페이지 활성화, 나머지 비활성화
        UpdatePageVisibility();
        // 버튼 이벤트 추가
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
            Color color = next.targetGraphic.color;
            color.a = 130.0f;
            next.targetGraphic.color = color;
            Debug.Log(currentPageIndex);
        }
        else if (currentPageIndex == pages.Count -1)
        {
            Color color = prev.targetGraphic.color;
            color.a = 130.0f;
            prev.targetGraphic.color = color;
            Debug.Log(currentPageIndex);
            exit.gameObject.SetActive(true);
        }
        else
        {
            Color color = next.targetGraphic.color;
            color.a = 255.0f;
            next.targetGraphic.color= color;
            prev.targetGraphic.color = color;
            Debug.Log(currentPageIndex);
        }
        Debug.Log(currentPageIndex + 1);
    }
}
