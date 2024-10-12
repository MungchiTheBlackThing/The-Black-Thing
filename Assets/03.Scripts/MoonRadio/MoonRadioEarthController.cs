using Assets.Script.DialClass;
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
    GameObject sendAlert;
    [SerializeField]
    GameObject closePopup;
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
    public void Write2Moon(TMP_Text text)
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

    public void Send2MoonBut()
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
    void Reset()
    {
        //���ƿ��� �ִϸ����� 
        //animator.ResetTrigger("YourTrigger");
        answerTextBox.SetActive(true); //�ٽ� ���� �ֱ� ������ ���ӿ�����Ʈ�� ���ش�s
    }
    public void WaitAlert()
    {
        StartCoroutine("waitForTransmission");
    }
    //waitForTransmission
    public IEnumerator waitForTransmission()
    {

        yield return new WaitForSeconds(2.0f);
        sendAlert.SetActive(false);
        sendEarth.SetActive(true);
        Reset();
        yield return null;

        //main.SetActive(true);
        //Destroy(this.gameObject);
    }

    public void Send2MoonButEventExit()
    {
        sendEarth.SetActive(false);
        sendAlert.SetActive(true);
        Invoke("WaitAlert", .5f);
    }

    //channel exit but ������.
    public void ExitChannelBut()
    {
        //close_Alter�� ���.
        closePopup.SetActive(true);
    }
    //ä�� ����
    public void YesBut()
    {
        //yes�� ������ send_Alert ��.. ȭ�� Ŭ���� ���� ȭ������ �̵�
        closePopup.SetActive(false);
        this.gameObject.SetActive(false);
    }

    //ä�� ���� ����
    public void NoBut()
    {
        //no�Ͻ�... ��������ҵ� ����..? 
        closePopup.SetActive(false);
    }
}
