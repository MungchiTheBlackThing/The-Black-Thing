using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DoorController : MonoBehaviour
{

    [SerializeField]
    public bool isDoorOpen = true;
    [SerializeField]
    public GameObject dot;
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
        CheckDot();
    }

    public void CheckDot()
    {
        if (isDoorOpen == false)
        {
            Collider2D[] overlappingColliders = Physics2D.OverlapBoxAll(targetCollider.bounds.center, targetCollider.bounds.size, 0);

            foreach (Collider2D collider in overlappingColliders)
            {
                if (collider != targetCollider && collider.gameObject == dot)
                {
                    StartCoroutine(DotvisibleCheck(true));
                }
            }
        }
        else
        {
            if (dot.GetComponent<BoxCollider2D>().enabled == false)
            {
                StartCoroutine(DotvisibleCheck(false));
            }
        }
    }
    private void Start()
    {
        CheckDot();
    }
    //private void FixedUpdate()
    //{
    //    CheckDot();
    //}
    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // ���콺�� UI ���� ���� ���� �� �Լ��� �������� �ʵ��� ��
            return;
        }

        if (!dot.GetComponent<DotController>().tutorial)
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

    }
    public void close()
    {
        int OpenIdx = Animator.StringToHash("isOpening");
        animator = this.transform.parent.GetComponent<Animator>();
        animator.SetFloat(Animator.StringToHash("speed"), 1.0f);
        animator.SetBool(OpenIdx, false);
        AudioManager.instance.PlayOneShot(FMODEvents.instance.door, this.transform.position);
        CheckDot();
    }
    public void open()
    {
        int OpenIdx = Animator.StringToHash("isOpening");
        animator = this.transform.parent.GetComponent<Animator>();
        animator.SetFloat(Animator.StringToHash("speed"), 1.0f);
        animator.SetBool(OpenIdx, true);
        AudioManager.instance.PlayOneShot(FMODEvents.instance.door, this.transform.position);
        CheckDot();
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

    public IEnumerator DotvisibleCheck(bool setoff)
    {
        yield return new WaitForSeconds(0.2f);
        if (setoff)
        {
            dot.GetComponent<DotController>().Invisible();
        }
        else
        {
            dot.GetComponent<DotController>().Visible();
        }
    }
}