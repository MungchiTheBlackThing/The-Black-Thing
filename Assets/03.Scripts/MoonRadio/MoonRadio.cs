using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoonRadio : MonoBehaviour
{
    [SerializeField]
    GameObject moonRadioController;
    [SerializeField]
    GameObject alert;
    [SerializeField]
    TMP_Text text;

    TranslateManager translator;
    Animator blinkMoonRadioAnim;

    private void Start()
    {
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
            // ���콺�� UI ���� ���� ���� �� �Լ��� �������� �ʵ��� ��
            return;
        }
        
        if(alert != null)
        {
            OpenAlert();
            //alert �˶��߱�
            return;
        }

        //�ð��밡 ���� �ƴ� ��쿡�� �Ʒ��� �۵����� �ʴ´�.
        if (moonRadioController.activeSelf == false)
        {
            //���� �� speed 1, ���� �ƴϸ� 0
            blinkMoonRadioAnim.SetFloat("speed", 0f);
            moonRadioController.SetActive(true);
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
