using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [SerializeField] Image skipIconImage;
    [SerializeField] Sprite iconOn;
    [SerializeField] Sprite iconOff;

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


        timeIdx = playerController.GetCurrentPhase();

        if(timeIdx < timeStamp.Length)
        {
            time = timeStamp[timeIdx];
        }
        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();

        translator.translatorDel += Translate;
        if (objectManager != null) objectManager.activeSystemUIDelegate += SetSkipButtonActiveState;
        RefreshSkipIcon();
    }

    private void OnEnable()
    {
        RefreshSkipIcon();
    }

    void RefreshSkipIcon()
    {
        if (!skipIconImage) return;
        bool enabled = playerController != null && playerController.GetSkipModeEnabled();
        skipIconImage.sprite = enabled ? iconOn : iconOff;
    }


    /// <summary>
    /// GameManager에서 호출하여 남은 시간을 UI에 표시합니다.
    /// </summary>
    /// <param name="remainingSeconds">남은 시간(초)</param>
    public void SetTime(float remainingSeconds)
    {
        if (remainingSeconds < 0) remainingSeconds = 0;
        remainingSeconds += 60f; // 표시용 버퍼(0초도 1분처럼)

        int hour = (int)remainingSeconds / HOUR;
        int min = ((int)remainingSeconds % HOUR) / MIN;

        if (timeText != null)
        {
            if (hour <= 0) timeText.text = min.ToString() + "m";          // 59m
            else           timeText.text = hour.ToString() + "h " + min.ToString("00") + "m"; // 1h 00m
        }
    }

    public void SetSkipButtonActiveState(bool InActive)
    {
        if (GameManager.isend)
        {
            // 엔딩에서는 스킵 버튼 자체를 절대 노출하지 않음
            if (popup) popup.SetActive(false);
            gameObject.SetActive(false);
            return;
        }
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
        var sceneName = SceneManager.GetActiveScene().name;
        // Tutorial 씬이 아니고 + 스킵모드 OFF면 막기
        if (sceneName != "Tutorial" && !playerController.GetSkipModeEnabled())
            return;
        if (GameManager.isend) return;
        if (popup.activeSelf == false)
        {
            popup.SetActive(true);
        }
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.iconClick, this.transform.position);
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

        // 잠 스킵일 때만 NextChapter landing용 앵커/플래그 저장
        var current = (GamePatternState)playerController.GetCurrentPhase();
        var next = (GamePatternState)((int)current + 1);

        if (current == GamePatternState.Sleeping && next == GamePatternState.NextChapter)
        {
            string anchorKey = $"NextChapterAnchor_{gameManager.Chapter}";
            string skipKey   = $"NextChapterEnteredBySkip_{gameManager.Chapter}";

            PlayerPrefs.SetString("NextChapterAnchor", DateTime.Now.ToBinary().ToString());
            PlayerPrefs.SetInt("NextChapterEnteredBySkip", 1);
            PlayerPrefs.Save();
        }

        gameManager.BeginSkipPhaseTransition(); // 스킵 영상 후 페이즈 넘김
    }
    public void TutoYesClick()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        const string anim = "anim_default";
        popup.SetActive(false);
        
        // 콜백 기반: 카메라 이동 완료 후 후속 작업
        gameManager.ScrollManager.MoveCamera(
            new Vector3((float)5.70, 0, -10), 
            1f,
            onComplete: () => {
                dotController.Visible();
                dotController.ChangeState(DotPatternState.Default, anim, 3);
                Destroy(GameObject.FindWithTag("TouchGuide"));
                StartCoroutine(subcontinue(0.1f));
            }
        );
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
