using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MoonRadioEarthController : MonoBehaviour
{
    [SerializeField]
    GameObject sendEarth;
    [SerializeField]
    GameObject closeAlert;
    [SerializeField]
    GameObject sendAlert;

    [SerializeField]
    GameObject exceedAlert;

    [SerializeField]
    GameObject textLength;

    [SerializeField]
    GameObject answerTextBox;

    bool isCheckingWithin500;
    int textlineCnt;
    private void OnEnable()
    {
        textlineCnt = 0; 
        isCheckingWithin500 = true;
    }
    public void write2Moon(TMP_Text text)
    {
        //������, �ڽ��� �������.

        if (text.text.Length >= 1)
        {
            answerTextBox.SetActive(false);
        }
        else
        {
            answerTextBox.SetActive(true);
        }
        
        textlineCnt = text.text.Length;
        textLength.GetComponent<TMP_Text>().text = textlineCnt.ToString() + "/500";

        if (textlineCnt > 500)
        {
            isCheckingWithin500 = false;
            exceedAlert.SetActive(true);
        }
        else
        {
            exceedAlert.SetActive(false);
            isCheckingWithin500 = true;
        }
        //�ѱ��ڶ� ������ ���ְ�, �ѱ��� �����ϸ� ����.
        //�۾� ó��..
        //text.text�� moonbut������ ���޵� string
    }

    public void OnEndEdit(TMP_Text text)
    {
        Debug.Log(text.text.Length);
    }

    public void send2MoonBut()
    {
        //textfield�� �������.
        //���� ���� ������Ʈ ���� �� �ִϸ��̼� ������ �Լ� ����
        //Debug.Log(inputText);
        if (isCheckingWithin500 == false) //500���� �̳�
        {
            exceedAlert.SetActive(true);
            //���� �Ұ�����.
            return;
        }
        exceedAlert.SetActive(false);
        GameObject currObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        currObj.GetComponent<Animator>().SetBool("isGoing", true);
        sendEarth.GetComponent<Animator>().SetBool("isGoing", true);
    }
}
