using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiaryPageController : MonoBehaviour
{
    // Start is called before the first frame update

    private bool isClick;
    float clickTime = 0.0f;
    [SerializeField]
    float minClickTime;
    public void OnEnable()
    {
        isClick = false;
    }

    public void ButtonUp()
    {
        isClick = true;
    }

    public void ButtonDown()
    {
        isClick = false;
        Debug.Log("Click �ٿ�");

        if (clickTime >= minClickTime)
        {
            //Ư�� ���� ����
            Debug.Log("Click �ٿ�");
        }
    }

    public void Update()
    {
        if(isClick)
        {
            clickTime += Time.deltaTime;
        }
        else 
        {
            clickTime = 0.0f;
        }
    }
}
