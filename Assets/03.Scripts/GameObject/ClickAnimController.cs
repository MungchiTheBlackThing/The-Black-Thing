using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ClickType
{
    Bread,
    Radio,
    Hourglass
}
public class ClickAnimController : BaseObject
{
    // Start is called before the first frame update

    Animator animator;

    [SerializeField]
    [Tooltip("������ ���� ClickObject Type")]
    ClickType Type;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnMouseDown()
    {

        if (EventSystem.current.IsPointerOverGameObject())
        {
            // ���콺�� UI ���� ���� ���� �� �Լ��� �������� �ʵ��� ��
            return;
        }
        switch ((int)Type)
        {
            case 0:
                AudioManager.instance.PlayOneShot(FMODEvents.instance.mold, this.transform.position);
                break;
            case 1:
                break;
            case 2:
                AudioManager.instance.PlayOneShot(FMODEvents.instance.hourglass, this.transform.position);
                break;
        }
            
        
        animator.SetTrigger("IsTrigger");
    }

}
