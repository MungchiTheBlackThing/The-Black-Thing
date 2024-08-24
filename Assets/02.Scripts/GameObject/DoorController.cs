using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
