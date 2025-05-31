using System.Collections;
using UnityEngine;
using TMPro;

public class DotTextReview : MonoBehaviour
{
    [SerializeField] PlayerController PlayerController;
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float holdDuration = 3.0f;
    [SerializeField] private int currentChapter = 1;

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
        switch (chapterNumber)
        {
            case 1: return review.Chapter1;
            case 2: return review.Chapter2;
            case 3: return review.Chapter3;
            case 4: return review.Chapter4;
            case 5: return review.Chapter5;
            case 6: return review.Chapter6;
            case 7: return review.Chapter7;
            case 8: return review.Chapter8;
            case 9: return review.Chapter9;
            case 10: return review.Chapter10;
            case 11: return review.Chapter11;
            case 12: return review.Chapter12;
            case 13: return review.Chapter13;
            default: return null;
        }
    }

    private IEnumerator PlayLines(string[] lines)
    {
        foreach (var line in lines)
        {
            displayText.text = line.Trim();
            yield return FadeIn();
            yield return new WaitForSeconds(holdDuration);
            yield return FadeOut();
        }

        displayText.text = "";
    }

    private IEnumerator FadeIn()
    {
        float time = 0f;
        Color color = displayText.color;
        color.a = 0f;
        displayText.color = color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, time / fadeDuration);
            displayText.color = color;
            yield return null;
        }

        color.a = 1f;
        displayText.color = color;
    }

    private IEnumerator FadeOut()
    {
        float time = 0f;
        Color color = displayText.color;
        color.a = 1f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, time / fadeDuration);
            displayText.color = color;
            yield return null;
        }

        color.a = 0f;
        displayText.color = color;
    }
}
