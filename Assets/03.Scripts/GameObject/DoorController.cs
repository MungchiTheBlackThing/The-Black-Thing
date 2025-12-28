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
                    dot.GetComponent<DotController>().dotvicheck(true);
                }
            }
        }
        else
        {
            if (dot.GetComponent<BoxCollider2D>())
            {
                dot.GetComponent<DotController>().dotvicheck(false);
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
        if (InputGuard.BlockWorldInput()) return;


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
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.door, this.transform.position);
        CheckDot();
    }
    public void open()
    {
        int OpenIdx = Animator.StringToHash("isOpening");
        animator = this.transform.parent.GetComponent<Animator>();
        animator.SetFloat(Animator.StringToHash("speed"), 1.0f);
        animator.SetBool(OpenIdx, true);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.door, this.transform.position);
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

    public void SetDoorForDialogue(bool enable)
    {
        // 터치 비활성화
        if (doorCollider != null)
            doorCollider.enabled = enable;

        // 렌더 끄기 (문이 시각적으로만! 사라지게 해야 함)
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = enable;

        // Dot 충돌 체크 강제 해제해서 뭉치가 메인할 때 보이도록 해야 함
        if (!enable)
        {
            if (dot != null)
                dot.GetComponent<DotController>()?.dotvicheck(false);
        }
    }


}