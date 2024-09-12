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
    [SerializeField]
    Collider2D targetCollider;

    Animator animator;

    public void Awake()
    {
        dot = GameObject.FindWithTag("DotController");
    }

    private void OnEnable()
    {
        if (dot == null)
        {
            dot = GameObject.FindWithTag("DotController");
        }

        if (isDoorOpen == false)
        {
            Collider2D[] overlappingColliders = Physics2D.OverlapBoxAll(targetCollider.bounds.center, targetCollider.bounds.size, 0);

            foreach (Collider2D collider in overlappingColliders)
            {
                if (collider != targetCollider && collider.gameObject == dot)
                {
                    dot.SetActive(false);
                }
            }
        }
        else
        {
            if(dot.activeSelf == false)
            {
                dot.SetActive(true);
            }
        }
    }

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

        int OpenIdx = Animator.StringToHash("isOpening");
        animator.SetFloat(Animator.StringToHash("speed"), 1.0f);

        if (isDoorOpen)
        {
            //�������� ���, �ݾƾ���
            animator.SetBool(OpenIdx, false);
        }
        else
        {
            //�ݾ��ִ� ���, �������
            animator.SetBool(OpenIdx, true);
        }
    }
}
