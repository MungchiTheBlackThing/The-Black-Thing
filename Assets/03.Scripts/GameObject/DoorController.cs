using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DoorController : MonoBehaviour
{
    static bool _sharedDoorOpen = true;
    public bool isDoorOpen => _sharedDoorOpen;
    [SerializeField]
    public GameObject dot;
    [SerializeField] [HideInInspector]
    Collider2D targetCollider;

    Animator animator;
    BoxCollider2D doorCollider;

    static float _lastDoorClickTime;
    const float DOOR_CLICK_COOLDOWN = 0.2f;

    public void Awake()
    {
        dot = GameObject.FindWithTag("DotController");
        doorCollider = this.GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        if (dot == null)
            dot = GameObject.FindWithTag("DotController");
    }

    //private void FixedUpdate()
    //{
    //    CheckDot();
    //}
    private void OnMouseDown()
    {
        if (Time.unscaledTime - _lastDoorClickTime < DOOR_CLICK_COOLDOWN)
            return;
        _lastDoorClickTime = Time.unscaledTime;

        if (InputGuard.BlockWorldInput()) return;
        if (dot == null) return;

        if (dot.GetComponent<DotController>().tutorial) return;

        if (_sharedDoorOpen) close();
        else open();
    }
    public void close()
    {
        _sharedDoorOpen = false;
        int OpenIdx = Animator.StringToHash("isOpening");
        animator = this.transform.parent.GetComponent<Animator>();
        animator.SetFloat(Animator.StringToHash("speed"), 1.0f);
        animator.SetBool(OpenIdx, false);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.door, this.transform.position);
    }
    public void open()
    {
        _sharedDoorOpen = true;
        int OpenIdx = Animator.StringToHash("isOpening");
        animator = this.transform.parent.GetComponent<Animator>();
        animator.SetFloat(Animator.StringToHash("speed"), 1.0f);
        animator.SetBool(OpenIdx, true);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.door, this.transform.position);
    }

    public void Touch()
    {
        if (_sharedDoorOpen)
        {
            close();
        }
        else
        {
            open();
        }
    }

    public void DisableTouch()
    {
        if (doorCollider != null)
        {
            doorCollider.enabled = false; 
        }
    }

    public void EnableTouch()
    {
        if (doorCollider != null)
        {
            doorCollider.enabled = true;
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
        //if (!enable)
        //{
        //    if (dot != null)
        //        dot.GetComponent<DotController>()?.dotvicheck(false);
        //} 필요 없습니다 이미 뭉치보다 문이 위 레이어이며, 메인 때는 문 렌더링을 끄는 
        // 방식으로 처리해 두어서 굳이 메인 때 문이 뭉치를 건드릴 필요 없음
    }


}