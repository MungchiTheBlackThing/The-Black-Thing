using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;

public class DotTextReview : MonoBehaviour
{
    [SerializeField] private PlayerController PlayerController;
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Button inputBlockerButton; // 전체화면 버튼
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0f;
    [SerializeField] private int currentChapter = 1;
    [SerializeField] private GameObject speechBubble;
    public System.Action onReviewFinished;
    private CanvasGroup speechBubbleGroup;
    private bool userClicked = false;

    private const string reviewTableName = "PoemReview";

    private void Start()
    {
        
    }
    private void Awake()
    {
        speechBubbleGroup = speechBubble.GetComponent<CanvasGroup>();
    }

    public void StartReview()
    {
        PlayerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        currentChapter = PlayerController.GetChapter();
        StartCoroutine(LoadAndPlayReview());
    }

    public IEnumerator LoadAndPlayReview()
    {
        StringTable table = LocalizationSettings.StringDatabase.GetTable(reviewTableName);
        List<string> lines = new List<string>();
        int lineIndex = 1;

        while (true)
        {
            string key = $"PR{currentChapter}_L{lineIndex:0000}";
            var entry = table.GetEntry(key);

            if (entry == null)
            {
                // 더 이상 키가 없으면 종료
                break;
            }

            string lineText = entry.GetLocalizedString();
            if (!string.IsNullOrWhiteSpace(lineText))
            {
                lines.Add(lineText.Trim());
            }

            lineIndex++;
        }

        if (lines.Count == 0)
        {
            Debug.LogError($"[DotTextReview] 챕터 {currentChapter} 에 해당하는 리뷰 텍스트가 없습니다.");
            yield break;
        }

        // 혹시 기존 코루틴이 돌고 있다면 정지 후 새로 시작
        StopAllCoroutines();
        StartCoroutine(PlayLines(lines.ToArray()));
    }

    private Chapter GetChapter(DotReview review, int chapterNumber)
    {
        return chapterNumber switch
        {
            1 => review.Chapter1,
            2 => review.Chapter2,
            3 => review.Chapter3,
            4 => review.Chapter4,
            5 => review.Chapter5,
            6 => review.Chapter6,
            7 => review.Chapter7,
            8 => review.Chapter8,
            9 => review.Chapter9,
            10 => review.Chapter10,
            11 => review.Chapter11,
            12 => review.Chapter12,
            13 => review.Chapter13,
            _ => null
        };
    }

    private IEnumerator PlayLines(string[] lines)
    {
        inputBlockerButton.gameObject.SetActive(true);
        inputBlockerButton.onClick.AddListener(OnUserClick);

        for (int i = 0; i < lines.Length; i++)
        {
            displayText.text = lines[i].Trim();
            if (displayText.text.Contains("<nickname>"))
            {
                string playerName = PlayerController.GetNickName();
                displayText.text = displayText.text.Replace("<nickname>", playerName);
            }
            yield return FadeIn();

            yield return WaitForUserInput();

            if (i < lines.Length - 1)
            {
                yield return FadeOut();
            }
            else
            {
                // 마지막 줄이면 유지하고 버튼 비활성화
                inputBlockerButton.onClick.RemoveListener(OnUserClick);
                inputBlockerButton.gameObject.SetActive(false);

                onReviewFinished?.Invoke();

                yield break;
            }
        }
    }

    private IEnumerator WaitForUserInput()
    {
        userClicked = false;

        while (!userClicked)
        {
            yield return null;
        }

        userClicked = false;
    }

    public void OnScreenClick()
    {
        userClicked = true;
    }


    private void OnUserClick()
    {
        userClicked = true;
    }

    private IEnumerator FadeIn()
    {
        float time = 0f;

        // 시작 설정
        speechBubbleGroup.alpha = 0f;
        speechBubbleGroup.blocksRaycasts = true;
        speechBubbleGroup.interactable = true;

        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            speechBubbleGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeInDuration);
            yield return null;
        }

        speechBubbleGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float time = 0f;

        speechBubbleGroup.alpha = 1f;
        speechBubbleGroup.blocksRaycasts = false;
        speechBubbleGroup.interactable = false;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            speechBubbleGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeOutDuration);
            yield return null;
        }

        speechBubbleGroup.alpha = 0f;
    }
}
