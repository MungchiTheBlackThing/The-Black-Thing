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
    [SerializeField] GameObject questionBubble;
    [SerializeField] GameObject afterYesBubble;
    [SerializeField] GameObject afterNoBubble;
    [SerializeField] TMP_Text questionText; // Txt


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
    Utility u = Utility.Instance;
    bool waitSleepTap = false;

    private void Start()
    {
        dot = playDot.transform.parent.GetComponent<DotController>();

        if (afterYesBubble != null) afterYesBubble.SetActive(false);
        if (afterNoBubble != null) afterNoBubble.SetActive(false);

    }


    private void OnEnable()
    {
        yesPoemBubble.onClick.AddListener(YesPoemBubbleClicked);
        noPoemBubble.onClick.AddListener(NoPoemBubbleClicked);
        playDotButton.onClick.AddListener(OnPlayDotClicked);

        // 1일차 다이어리 잠금 해제
        if (gameManager != null && gameManager.Chapter == 1 && !gameManager.pc.IsDiaryUnlockedForChapter1())
        {
            Debug.Log("1일차 다이어리 잠금 해제");
            gameManager.pc.UnlockDiaryForChapter1();
        }
    }

    public void EnterPoemQuestion()
    {
        if (questionBubble != null) questionBubble.SetActive(true);
        if (afterYesBubble != null) afterYesBubble.SetActive(false);
        if (afterNoBubble != null) afterNoBubble.SetActive(false);

        playDot.SetActive(true);
        playPlayer.SetActive(true);
        waitSleepTap = false;
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
        var poemObj = Instantiate(poem, GameObject.Find("Canvas").transform);

        
        var poemController = poemObj.GetComponentInChildren<PoemController>(true);
        if (poemController != null)
            poemController.playAnswerController = this;
        HideBubble();
    }
    //시 읽은 후 처리
    public void AfterReadingPoem()
    {
        //자러가야지 대사 출력
        SetAfterYesState();
    }

    private void SetAfterYesState()
    {
        if (questionBubble != null) questionBubble.SetActive(false);
        if (afterYesBubble != null) afterYesBubble.SetActive(true);
        if (afterNoBubble != null) afterNoBubble.SetActive(false);

        playDot.SetActive(true);
        playPlayer.SetActive(false);
        waitSleepTap = true;
    }

    //no버블 눌렀을 때
    private void NoPoemBubbleClicked()
    {
        SetAfterNoState();
    }

    private void SetAfterNoState()
    {
        if (questionBubble != null) questionBubble.SetActive(false);
        if (afterYesBubble != null) afterYesBubble.SetActive(false);
        if (afterNoBubble != null) afterNoBubble.SetActive(true);

        playDot.SetActive(true);
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
