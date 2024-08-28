using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DoorController : MonoBehaviour
{

    [SerializeField]
    bool isDoorOpen;

    Animator animator;

    private void Start()
    {
        animator = this.transform.parent.GetComponent<Animator>();
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // ���콺�� UI ���� ���� ���� �� �Լ��� �������� �ʵ��� ��
            return;
        }

        animator.SetFloat("speed", 1.0f);

        if (isDoorOpen)
        {
            //�������� ���, �ݾƾ���
            animator.SetBool("isOpening", false);
        }
        else
        {
            //�ݾ��ִ� ���, �������
            animator.SetBool("isOpening", true);
        }
    }
}
