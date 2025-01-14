using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchGuide : MonoBehaviour
{
    [SerializeField]
    private Button myButton;

    [SerializeField] 
    SubPanel subPanel;

    [SerializeField]
    GameObject Touchground;


    private void OnEnable()
    {
        myButton = this.transform.GetChild(3).GetComponent<Button>();
    }

    public void tuto2(GameObject selectedDot, int determine)
    {
        subPanel = GameObject.Find("SubPanel").GetComponent<SubPanel>();
        if (myButton != null)
        {
            // 버튼의 onClick 이벤트에 함수 추가
            myButton.onClick.AddListener(() => tuto2Click(selectedDot, determine));
        }
        else
        {
            Debug.LogError("Button reference is missing!");
        }
    }
    public void tuto3(GameObject selectedDot, int determine)
    {
        subPanel = GameObject.Find("SubPanel").GetComponent<SubPanel>();
        if (myButton != null)
        {
            // 버튼의 onClick 이벤트에 함수 추가
            myButton.onClick.AddListener(() => tuto3Click(selectedDot, determine));
        }
        else
        {
            Debug.LogError("Button reference is missing!");
        }
    }
    public void tuto2Click(GameObject selectedDot, int determine)
    {
        GameObject door = GameObject.Find("fix_door");
        Debug.Log(door);
        door.transform.GetChild(1).GetComponent<DoorController>().open();
        subPanel.clickon();
        if (determine == 0)
        {
            subPanel.dotballoon(selectedDot);
        }
        else
        {
            subPanel.playerballoon(selectedDot);
        }
        Destroy(this.gameObject);
    }
    public void tuto3Click(GameObject selectedDot, int determine)
    {
        GameObject door = GameObject.Find("fix_door");
        Debug.Log(door);
        door.transform.GetChild(1).GetComponent<DoorController>().close();
        subPanel.clickon();
        if (determine == 0)
        {
            subPanel.dotballoon(selectedDot);
        }
        else
        {
            subPanel.playerballoon(selectedDot);
        }
        Destroy(this.gameObject);
    }

    public void skipGuide()
    {
        if (myButton != null)
        {
            // 버튼의 onClick 이벤트에 함수 추가
            myButton.onClick.AddListener(() => skipClick());
            Touchground.SetActive(true);
        }
        else
        {
            Debug.LogError("Button reference is missing!");
        }
    }

    public void skipClick()
    {
        TimeSkipUIController timeSkip = GameObject.Find("TimeSkip").GetComponent<TimeSkipUIController>();
        timeSkip.OnClick();
        Destroy(this.gameObject);
    }
}
