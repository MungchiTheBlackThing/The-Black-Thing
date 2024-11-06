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
    BoxCollider2D doorCollider;

    public void Awake()
    {
        dot = GameObject.FindWithTag("DotController");
        doorCollider = this.GetComponent<BoxCollider2D>();
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
                    SpriteRenderer dotRenderer = dot.GetComponent<SpriteRenderer>();
                    Color color = dotRenderer.color;
                    color.a = 0f; 
                    dotRenderer.color = color;
                    dot.GetComponent<BoxCollider2D>().enabled = false;
                }
            }
        }
        else
        {
            if (dot.GetComponent<BoxCollider2D>().enabled == false)
            {
                SpriteRenderer dotRenderer = dot.GetComponent<SpriteRenderer>();
                Color color = dotRenderer.color;
                color.a = 255f;
                dotRenderer.color = color;
                dot.GetComponent<BoxCollider2D>().enabled = true;
            }
        }
    }

    private void Start()
    {
       
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // ���콺�� UI ���� ���� ���� �� �Լ��� �������� �ʵ��� ��
            return;
        }


        if (isDoorOpen)
        {
            //�������� ���, �ݾƾ���
            close();
        }
        else
        {
            //�ݾ��ִ� ���, �������
            open();
        }
    }
    public void close()
    {
        int OpenIdx = Animator.StringToHash("isOpening");
        animator = this.transform.parent.GetComponent<Animator>();
        animator.SetFloat(Animator.StringToHash("speed"), 1.0f);
        animator.SetBool(OpenIdx, false);
    }
    public void open()
    {
        int OpenIdx = Animator.StringToHash("isOpening");
        animator = this.transform.parent.GetComponent<Animator>();
        animator.SetFloat(Animator.StringToHash("speed"), 1.0f);
        animator.SetBool(OpenIdx, true);
    }

    public void Touch()
    {
        if (isDoorOpen)
        {
            //�������� ���, �ݾƾ���
            close();
        }
        else
        {
            //�ݾ��ִ� ���, �������
            open();
        }
    }

    public void DisableTouch()
    {
        if (doorCollider != null)
        {
            doorCollider.enabled = false; // ���� ���� ��ġ/Ŭ�� ��Ȱ��ȭ
        }
    }

    public void EnableTouch()
    {
        if (doorCollider != null)
        {
            doorCollider.enabled = true; // ���� ���� ��ġ/Ŭ�� Ȱ��ȭ
        }
    }
}
