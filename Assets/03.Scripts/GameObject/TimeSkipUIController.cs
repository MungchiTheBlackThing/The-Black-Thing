using System;
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
    GameManager gameManager;

    [SerializeField]
    DotController dotController;
    [SerializeField]
    ObjectManager objectManager;

    // 0 watch
    // A -> watching 흐름 따라간다.  1
    // 1 thinking 2
    // B 3
    // 2 writinng 4
    // play 5
    // 3 spleeping 6

                        // 0watching   1 thinking 2 wrting   3 play   4 sleeping
    float[] timeStamp = { 3600f, -1f, 1800f, -1f, 7200f, -1f, 1800f, -1f };
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

        dotController = GameObject.FindWithTag("DotController").GetComponent<DotController>();

        gameManager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();

        playerController.nextPhaseDelegate += NextPhase;

        timeIdx = playerController.GetAlreadyEndedPhase();

        if(timeIdx < timeStamp.Length)
        {
            time = timeStamp[timeIdx];
        }

        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();

        translator.translatorDel += Translate;
        objectManager.activeSystemUIDelegate += ControllActiveState;
    }

    private void Update()
    {
        if(timeIdx != -1 && time >= 0)
        {
            int hour = (int)time / HOUR; 
            int min = ((int)time % HOUR) / MIN;
            timeText.text = (hour).ToString() + "h " + (min).ToString() + "m";
            time -= Time.deltaTime;

            if(time < 0)
            {
                //다음 챕터로 넘어간다.
                Debug.Log("2");
                playerController.NextPhase();
                time = timeStamp[timeIdx]; //리셋
                return;
            }
        }
    }

    void NextPhase(GamePatternState gameState)
    {
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

        text[1].text = DataManager.Instance.Settings.timeSkip.yes[Idx];
        text[2].text = DataManager.Instance.Settings.timeSkip.no[Idx];

        //for(int i=0; i<text.Length; i++)
        //{
        //    text[i].font = font;
        //}
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

        Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    }

    public void TutoYesClick()
    {
        const string anim = "anim_default";
        popup.SetActive(false);
        gameManager.ScrollManager.MoveCamera(new Vector3((float)5.70, 0, -10), 1f);
        dotController.ChangeState(DotPatternState.Default, anim, 3);
        StartCoroutine(subcontinue(1.2f));
    }
    IEnumerator subcontinue (float delay)
    {
        yield return new WaitForSeconds(delay);
        gameManager.SubContinue();
        this.gameObject.SetActive(false);
    }
}
