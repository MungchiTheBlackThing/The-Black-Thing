using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoonoteController : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> pages; // ������ GameObject���� ������ ����Ʈ

    private int currentPageIndex = 0; // ���� ������ �ε���

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
        // �ʱ�ȭ: ù ������ Ȱ��ȭ, ������ ��Ȱ��ȭ
        UpdatePageVisibility();
        // ��ư �̺�Ʈ �߰�
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
            Debug.Log("�ۼ����� ���� �ϱ�");
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
            Debug.Log("�ϱ����� ������ ����");
        }
    }
    private void UpdatePageVisibility()
    {
        // ��� �������� ��Ȱ��ȭ�ϰ� ���� �������� Ȱ��ȭ
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
