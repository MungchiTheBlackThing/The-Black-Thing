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
        //누르면, 박스는 사라진다.

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
        //한글자라도 있으면 없애고, 한글자 존재하면 생김.
        //글씨 처리..
        //text.text는 moonbut누를시 전달될 string
    }

    public void OnEndEdit(TMP_Text text)
    {
        Debug.Log(text.text.Length);
    }

    public void Send2MoonBut()
    {
        //textfield가 사라진다.
        //현재 누른 오브젝트 실행 후 애니메이션 끝나면 함수 실행
        //Debug.Log(inputText);
        if (isCheckingWithin500 == false) //500글자 이내
        {
            exceedAlert.SetActive(true);
            //전송 불가능함.
            return;
        }
        exceedAlert.SetActive(false);
        GameObject currObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        currObj.GetComponent<Animator>().SetBool("isGoing", true);
        sendEarth.GetComponent<Animator>().SetBool("isGoing", true);
    }
    void Reset()
    {
        //돌아오는 애니메이터 
        //animator.ResetTrigger("YourTrigger");
        answerTextBox.SetActive(true); //다시 쓸수 있기 때문에 게임오브젝트를 켜준다s
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

    //channel exit but 누른다.
    public void ExitChannelBut()
    {
        //close_Alter이 뜬다.
        closePopup.SetActive(true);
    }
    //채널 종료
    public void YesBut()
    {
        //yes를 누르면 send_Alert 뜸.. 화면 클릭시 메인 화면으로 이동
        closePopup.SetActive(false);
        this.gameObject.SetActive(false);
    }

    //채널 종료 안함
    public void NoBut()
    {
        //no일시... 물어봐야할듯 뭔데..? 
        closePopup.SetActive(false);
    }
}
