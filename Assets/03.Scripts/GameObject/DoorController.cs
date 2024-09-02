using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DoorController : MonoBehaviour
{

    [SerializeField]
    bool isDoorOpen = true;
    [SerializeField]
    GameObject dot;

    Animator animator;

    private void Start()
    {
        animator = this.transform.parent.GetComponent<Animator>();
        dot = GameObject.FindWithTag("DotController").gameObject;   
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
            dot.SetActive(false);
            //�������� ���, �ݾƾ���
            animator.SetBool("isOpening", false);
        }
        else
        {
            dot.SetActive(true);
            //�ݾ��ִ� ���, �������
            animator.SetBool("isOpening", true);
        }
    }
}
