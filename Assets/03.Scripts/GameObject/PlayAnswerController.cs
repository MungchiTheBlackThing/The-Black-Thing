using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

public class PlayAnswerController : MonoBehaviour
{
    [SerializeField]
    GameObject playPlayer;
    [SerializeField]
    GameObject playDot;
    [SerializeField]
    Button yesPoemBubble;
    [SerializeField]
    Button noPoemBubble;
    [SerializeField]
    Button playDotButton;

    [SerializeField]
    GameManager gameManager;
    [SerializeField]
    bool answer;

    [SerializeField]
    GameObject poem;

    DotController dot;
    public TMP_Text dotText;
    Utility u = Utility.Instance;
    bool waitSleepTap = false;

    private void Start()
    {
        dot = playDot.transform.parent.GetComponent<DotController>();

        yesPoemBubble.onClick.AddListener(YesPoemBubbleClicked);
        noPoemBubble.onClick.AddListener(NoPoemBubbleClicked);
        playDotButton.onClick.AddListener(OnPlayDotClicked);
    }

    private void OnEnable()
    {
        yesPoemBubble.onClick.AddListener(YesPoemBubbleClicked);
        noPoemBubble.onClick.AddListener(NoPoemBubbleClicked);
        playDotButton.onClick.AddListener(OnPlayDotClicked);
    }

    public void OnDisable()
    {
        yesPoemBubble.onClick.RemoveListener(YesPoemBubbleClicked);
        noPoemBubble.onClick.RemoveListener(NoPoemBubbleClicked);
        playDotButton.onClick.RemoveListener(OnPlayDotClicked);
        waitSleepTap = false;
    }

    //yes버블 눌렀을 때
    private void YesPoemBubbleClicked()
    {
        if (poem)
        {
            Instantiate(poem, GameObject.Find("Canvas").transform);
        }
        HideBubble();
    }
    //시 읽은 후 처리
    public void AfterReadingPoem()
    {
        //자러가야지 대사 출력
        //응 알겠어 자러갈게 대사 출력
        StringTable stringTable = LocalizationSettings.StringDatabase.GetTable("PoetryUIText");
        dotText.text = stringTable.GetEntry("poem_end").GetLocalizedString();
        playDot.SetActive(true);
        playPlayer.SetActive(false);
        waitSleepTap = true;
    }

    //no버블 눌렀을 때
    private void NoPoemBubbleClicked()
    {
        //응 알겠어 자러갈게 대사 출력
        StringTable stringTable = LocalizationSettings.StringDatabase.GetTable("PoetryUIText");
        dotText.text = stringTable.GetEntry("poem_n").GetLocalizedString();
        playPlayer.SetActive(false);
        waitSleepTap = true;
    }

    private void OnPlayDotClicked()
    {   //닷 말풍선 클릭시 자러 감
        if (!waitSleepTap) return;

        waitSleepTap = false;
        HideBubble();
        gameManager.GoSleep();
    }

    private void HideBubble()
    {
        playDot.SetActive(false);
        playPlayer.SetActive(false);
    }






    
}
