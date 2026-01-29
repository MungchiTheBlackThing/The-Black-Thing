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

        if (InputGuard.BlockWorldInput()) return;
        switch ((int)Type)
        {
            case 0:
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.mold, this.transform.position);
                break;
            case 1:
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.mapradio, transform.position);
                AudioManager.Instance.ToggleBGMMute();
                break;
            case 2:
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.hourglass, this.transform.position);
                break;
        }
            
        
        animator.SetTrigger("IsTrigger");
    }

}
