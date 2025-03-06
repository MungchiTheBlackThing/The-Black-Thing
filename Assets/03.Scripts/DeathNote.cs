using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class DeathNote : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Transform pagesContainer; // ���������� ��� �ִ� �θ� Transform
    public int pageCount; // �������� �� ����
    public int currentPageIndex = 0; // ���� �������� �ε���
    private Vector2 dragStartPosition; // �巡�� ���� ��ġ

    void OnEnable()
    {
        pageCount = pagesContainer.childCount - 1; // �������� �� ���� (-1�� �ݱ� ��ư�� �����ϱ� ����)

        // �ʱ�ȭ: ù ��° �������� Ȱ��ȭ�ϰ�, �������� ��Ȱ��ȭ
        for (int i = 0; i < pageCount; i++)
        {
            pagesContainer.GetChild(i).gameObject.SetActive(i == 0);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // �巡�� ���� ���� ����
        dragStartPosition = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // �巡�װ� ������ ���� ��ġ ����
        Vector2 dragEndPosition = eventData.position;
        float differenceX = dragEndPosition.x - dragStartPosition.x;

        // ���������� ���������Ͽ� ���� �������� �Ѿ ��
        if (differenceX < 0 && currentPageIndex < pageCount - 1)
        {
            pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(false); // ���� ������ ��Ȱ��ȭ
            currentPageIndex++; // ���� ������ �ε����� �̵�
            pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(true); // ���� ������ Ȱ��ȭ
        }
        // �������� ���������Ͽ� ���� �������� �Ѿ ��
        else if (differenceX > 0 && currentPageIndex > 0)
        {
            pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(false); // ���� ������ ��Ȱ��ȭ
            currentPageIndex--; // ���� ������ �ε����� �̵�
            pagesContainer.GetChild(currentPageIndex).gameObject.SetActive(true); // ���� ������ Ȱ��ȭ
        }
        if (currentPageIndex == pageCount - 1)
        {
            pagesContainer.GetChild(pageCount).gameObject.SetActive(true); //exit ��ư Ȱ��ȭ
        }
    }


    void OnDisable()
    {
        currentPageIndex = 0;
        pagesContainer.GetChild(pageCount).gameObject.SetActive(false);
        if (DeathNoteClick.checkdeath)
        {
            pagesContainer.GetChild(pageCount).gameObject.SetActive(true); //exit ��ư Ȱ��ȭ
        }
    }

    public void Onexitclick()
    {
        this.gameObject.SetActive(false);
    }
}