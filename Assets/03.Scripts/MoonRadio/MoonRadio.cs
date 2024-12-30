using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.EventSystems;

public class MoonRadio : MonoBehaviour
{
    [SerializeField]
    GameObject moonRadioController;
    [SerializeField]
    GameObject alert;
    [SerializeField]
    TMP_Text text;
    [SerializeField]
    GameObject screen;
    [SerializeField]
    DotController Dot;

    TranslateManager translator;
    Animator blinkMoonRadioAnim;

    private void Start()
    {
        screen = GameObject.Find("Screen");
        Dot = GameObject.FindWithTag("DotController").GetComponent<DotController>();
        blinkMoonRadioAnim = GetComponent<Animator>();
        moonRadioController = GameObject.Find("MoonRadio").transform.GetChild(0).gameObject;
        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();
        translator.translatorDel += Translate;
    }
    void Translate(LANGUAGE language, TMP_FontAsset font)
    {
        if (alert != null)
        {
            int Idx = (int)language;
            text.text = DataManager.Instance.Settings.alert.diary[Idx];
            text.font = font;
        }
    }
    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // 마우스가 UI 위에 있을 때는 이 함수가 동작하지 않도록 함
            return;
        }
        
        if(alert != null)
        {
            OpenAlert();
            //alert 알람뜨기
            return;
        }

        //시간대가 밤이 아닐 경우에는 아래는 작동하지 않는다.
        if (moonRadioController.activeSelf == false && !Dot.tutorial)
        {
            //밤일 때 speed 1, 밤이 아니면 0
            blinkMoonRadioAnim.SetFloat("speed", 0f);
            moonRadioController.SetActive(true);
            screen.SetActive(false);
        }
    }

    public void OpenAlert()
    {
        if (alert.activeSelf == false)
        {
            alert.SetActive(true);
            StartCoroutine(CloseAlter(alert));
        }
    }

    IEnumerator CloseAlter(GameObject alert)
    {
        yield return new WaitForSeconds(1.5f);
        alert.SetActive(false);
    }
}
