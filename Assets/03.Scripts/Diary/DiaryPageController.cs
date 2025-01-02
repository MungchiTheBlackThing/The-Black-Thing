using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiaryPageController : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> pages; // ������ GameObject���� ������ ����Ʈ

    private int currentPageIndex = 0; // ���� ������ �ε���

    [SerializeField]
    private float minClickTime = 0.3f; // �ٿ� Ŭ�� ���� �ð�

    private bool isClick = false; // ��ư Ŭ�� ����
    private float clickTime = 0.0f; // Ŭ�� �ð�

    [SerializeField]
    private GameManager gameManger; // ���� ������ ��ư

    public int chapter;
    private void Start()
    {
        foreach (Transform child in transform)
        {
            pages.Add(child.gameObject);
        }

        chapter = gameManger.Chapter - 1;
        // �ʱ�ȭ: ù ������ Ȱ��ȭ, ������ ��Ȱ��ȭ
        UpdatePageVisibility();
        // ��ư �̺�Ʈ �߰�
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
                Debug.Log("�ۼ����� ���� �ϱ�");
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
                Debug.Log("�ϱ����� ������ ����");
            }
        }
    }
    private void UpdatePageVisibility()
    {
        // ��� �������� ��Ȱ��ȭ�ϰ� ���� �������� Ȱ��ȭ
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(i == currentPageIndex);
            return;
        }
        Debug.Log(currentPageIndex + 1);
    }
}
