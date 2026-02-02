using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Replay : MonoBehaviour
{
    [SerializeField]
    GameObject popup;

    [SerializeField]
    PlayerController playerController;
    TranslateManager translator;

    [SerializeField]
    TMP_Text[] text;

    [SerializeField]
    GameManager gameManager;

    [SerializeField]
    DotController dotController;


    private void Start()
    {
        if(playerController == null)
        {
            playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        }

        gameManager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();

        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();

        translator.translatorDel += Translate;
    }

    public void Translate(LANGUAGE language, TMP_FontAsset font)
    {
        int Idx = (int)language;

        text[0].text = DataManager.Instance.Settings.timeSkip.title[Idx];

        text[1].text = DataManager.Instance.Settings.timeSkip.yes[Idx];
        text[2].text = DataManager.Instance.Settings.timeSkip.no[Idx];

        for(int i=0; i<text.Length; i++)
        {
            text[i].font = font;
        }
    }
    public void OnClick()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.iconClick, this.transform.position);
        if (popup.activeSelf == false)
        {
            popup.SetActive(true);
        }
    }

    public void NoClick()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        popup.SetActive(false);
    }

    public void YesClick()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        popup.SetActive(false);
        playerController.Replay();
    }
}
