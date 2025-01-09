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
    TMP_Text timeText;

    [SerializeField]
    ObjectManager objectManager;

    float[] timeStamp = { 3600f, 1800f, 7200f, 1800f, 1800f };
    int timeIdx = -1;
    float time = 0;
    const int HOUR = 3600;
    const int MIN = 60;


    private void Start()
    {
        if(playerController == null)
        {
            playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        }

        playerController.nextPhaseDelegate += NextPhase;

        timeIdx = playerController.GetAlreadyEndedPhase();
        time = timeStamp[timeIdx];

        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();

        translator.translatorDel += Translate;

        objectManager.activeSystemUIDelegate += ControllActiveState;
    }

    private void Update()
    {
        if(timeIdx != -1)
        {
            int hour = (int)time / HOUR; //3600초 나눔
            int min = ((int)time % HOUR) / MIN;
            timeText.text = (hour).ToString() + "h " + (min).ToString() + "m";
            time -= Time.deltaTime;
        }
    }

    void NextPhase(GamePatternState gameState)
    {
        Debug.Log("NextPhase");
        timeIdx = (int)gameState;
        if(timeIdx < timeStamp.Length)
        {
            time = timeStamp[timeIdx];
        }
    }

    public void ControllActiveState(bool InActive)
    {
        this.gameObject.SetActive(InActive);
    }
    public void Translate(LANGUAGE language, TMP_FontAsset font)
    {
        int Idx = (int)language;

        text[0].text = DataManager.Instance.Settings.timeSkip.title[Idx];

        //아이콘
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
