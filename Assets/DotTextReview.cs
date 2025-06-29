using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DotTextReview : MonoBehaviour
{
    [SerializeField] private PlayerController PlayerController;
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Button inputBlockerButton; // 전체화면 버튼
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private int currentChapter = 1;
    [SerializeField] private GameObject speechBubble;
    private CanvasGroup speechBubbleGroup;

    private void Awake()
    {
        speechBubbleGroup = speechBubble.GetComponent<CanvasGroup>();
    }

    private bool userClicked = false;

    public void StartReview()
    {
        PlayerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        currentChapter = PlayerController.GetChapter();
        Translate(PlayerController.GetLanguage());
    }

    public void Translate(LANGUAGE language)
    {
        Debug.Log("리뷰 번역을 시작합니다.");

        DotReview dotReview = DataManager.Instance.DotReview;
        if (dotReview == null)
        {
            Debug.LogError("DotReview 데이터가 없습니다.");
            return;
        }

        Chapter chapter = GetChapter(dotReview, currentChapter);
        if (chapter == null)
        {
            Debug.LogError("챕터 데이터가 없습니다.");
            return;
        }

        string rawText = (language == LANGUAGE.KOREAN) ? chapter.kortext : chapter.engtext;
        if (string.IsNullOrEmpty(rawText))
        {
            Debug.LogError("해당 언어 텍스트가 없습니다.");
            return;
        }

        string[] lines = rawText.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        StopAllCoroutines();
        StartCoroutine(PlayLines(lines));
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

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            speechBubbleGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
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

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            speechBubbleGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);
            yield return null;
        }

        speechBubbleGroup.alpha = 0f;
    }
}
