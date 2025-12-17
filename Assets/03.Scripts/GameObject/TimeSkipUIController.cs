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
    public bool IsSkipButtonClicked = false; //GameManager에서 사용 (스킵 버튼 클릭 시에만 스킵 영상 나오도록)


    private void Start()
    {
        IsSkipButtonClicked = false;

        if(playerController == null)
        {
            playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        }

        dotController = GameObject.FindWithTag("DotController").GetComponent<DotController>();

        gameManager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();

<<<<<<< Updated upstream
        playerController.nextPhaseDelegate += NextPhase;

        timeIdx = playerController.GetCurrentPhase();

        if(timeIdx < timeStamp.Length)
        {
            time = timeStamp[timeIdx];
        }
=======
        // playerController.nextPhaseDelegate += NextPhase; // 타이머를 GameManager에서 관리하므로 더 이상 필요 없음
>>>>>>> Stashed changes

        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();

        translator.translatorDel += Translate;
        if (objectManager != null) objectManager.activeSystemUIDelegate += ControllActiveState;
    }

    private void Update()
    {
        // 타이머 로직을 GameManager로 이전했으므로 Update 내용은 제거합니다.
    }

    void NextPhase(GamePatternState gameState)
    {
        // 타이머 로직을 GameManager에서 관리하므로 이 함수는 더 이상 필요하지 않습니다.
    }

    /// <summary>
    /// GameManager에서 호출하여 남은 시간을 UI에 표시합니다.
    /// </summary>
    /// <param name="remainingSeconds">남은 시간(초)</param>
    public void SetTime(float remainingSeconds)
    {
        if (remainingSeconds < 0) remainingSeconds = 0;
        int hour = (int)remainingSeconds / HOUR;
        int min = ((int)remainingSeconds % HOUR) / MIN;
        if (timeText != null)
            timeText.text = (hour).ToString() + "h " + (min).ToString() + "m";
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
        IsSkipButtonClicked = true;
        popup.SetActive(false);
        playerController.NextPhase();
        Debug.Log("스킵 클릭");
    }
    public void TutoNoClick()
    {
        Destroy(GameObject.FindWithTag("TouchGuide"));
        gameManager.ScrollManager.scrollable();
        popup.SetActive(false);
    }
    public void TutoYesClick()
    {
        const string anim = "anim_default";
        popup.SetActive(false);
        gameManager.ScrollManager.MoveCamera(new Vector3((float)5.70, 0, -10), 1f);
        dotController.Visible();
        dotController.ChangeState(DotPatternState.Default, anim, 3);
        Destroy(GameObject.FindWithTag("TouchGuide"));
        StartCoroutine(subcontinue(0.1f));
    }
    IEnumerator subcontinue (float delay)
    {
        yield return new WaitForSeconds(delay);
        gameManager.SubContinue();
        destroy();
    }

    public void destroy()
    {
        Destroy(this.gameObject);
    }

}
