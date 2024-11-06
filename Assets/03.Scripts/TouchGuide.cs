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


    private void OnEnable()
    {
        myButton = this.GetComponent<Button>();
    }

    public void tuto2(GameObject selectedDot, int determine)
    {
        subPanel = GameObject.Find("SubPanel").GetComponent<SubPanel>();
        if (myButton != null)
        {
            // ��ư�� onClick �̺�Ʈ�� �Լ� �߰�
            myButton.onClick.AddListener(() => tuto2Click(selectedDot, determine));
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
        Destroy(myButton.gameObject);
    }
}
