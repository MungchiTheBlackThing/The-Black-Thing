using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class MoonRadioController : MonoBehaviour
{
    // Start is called before the first frame update

    TranslateManager translator;

    [SerializeField]
    TMP_Text[] radioOn;
    [SerializeField]
    TMP_Text[] radioOff;
    [SerializeField]
    TMP_Text[] earth;

    void Start()
    {
        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();
        translator.translatorDel += Translate;
    }

    void Translate(LANGUAGE language, TMP_FontAsset font)
    {
        //번역한다.
        Debug.Log("달나라 송신기를 번역합니다.\n");

        int Idx = (int)language;

        radioOn[0].text = DataManager.Instance.Settings.moonRadioMain.radioOn.text[Idx];
        radioOn[1].text = DataManager.Instance.Settings.moonRadioMain.radioOn.earth[Idx];
        radioOn[2].text = DataManager.Instance.Settings.moonRadioMain.radioOn.moon[Idx];
        radioOn[3].text = DataManager.Instance.Settings.moonRadioMain.radioOn.exit[Idx];

        radioOff[0].text = DataManager.Instance.Settings.moonRadioMain.radioOff.text[Idx];
        radioOff[1].text = DataManager.Instance.Settings.moonRadioMain.radioOff.yes[Idx];
        radioOff[2].text = DataManager.Instance.Settings.moonRadioMain.radioOff.no[Idx];

        earth[0].text = DataManager.Instance.Settings.moonRadioEarth.placeholder[Idx];
        earth[1].text = DataManager.Instance.Settings.moonRadioEarth.alert[Idx];
        earth[2].text = DataManager.Instance.Settings.moonRadioEarth.exit[Idx];
        earth[3].text = DataManager.Instance.Settings.moonRadioEarth.popupExit[Idx];
        earth[4].text = DataManager.Instance.Settings.moonRadioEarth.yes[Idx];
        earth[5].text = DataManager.Instance.Settings.moonRadioEarth.no[Idx];

        //for (int i = 0; i < radioOff.Length; i++)
        //{
        //    radioOff[i].font = font;
        //}

        //for (int i = 0; i < radioOn.Length; i++)
        //{
        //    radioOn[i].font = font;      
        //}

        //for(int i=0;i<earth.Length; i++)
        //{
        //    earth[i].font = font;
        //}
    }
}
