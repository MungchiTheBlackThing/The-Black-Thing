using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class TimeSkipUIController : MonoBehaviour
{
    [SerializeField]
    GameObject popup;

    [SerializeField]
    PlayerController playerController;
    TranslateManager translator;

    [SerializeField]
    TMP_Text[] text;

    [SerializeField]
    ObjectManager objectManager;

    private void Start()
    {
        if(playerController == null)
        {
            playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        }
        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();

        translator.translatorDel += Translate;

        objectManager.activeSystemUIDelegate += ControllActiveState;
    }

    public void ControllActiveState(bool InActive)
    {
        this.gameObject.SetActive(InActive);
    }
    public void Translate(LANGUAGE language, TMP_FontAsset font)
    {
        Debug.Log("TimeSkip �����մϴ�.\n");
        int Idx = (int)language;

        text[0].text = DataManager.Instance.Settings.timeSkip.title[Idx];

        //������
        text[1].text = DataManager.Instance.Settings.timeSkip.yes[Idx];
        text[2].text = DataManager.Instance.Settings.timeSkip.no[Idx];

        for(int i=0; i<text.Length; i++)
        {
            text[i].font = font;
        }
    }
    public void OnClick()
    {
        if (popup.activeSelf == false)
        {
            popup.SetActive(true);
        }
    }

    public void NoClick()
    {
        popup.SetActive(false);
    }

    public void YesClick()
    {
        popup.SetActive(false);
        playerController.NextPhase();
    }
}
