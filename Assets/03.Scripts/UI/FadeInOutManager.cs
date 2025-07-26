using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInOutManager : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image _image;
    [SerializeField] private float _fadeTime = 3.0f;
    [SerializeField] private AnimationCurve _fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public void StartFadeIn()
    {
        StartCoroutine(Fade(1, 0));
    }

    public void StartFadeOut()
    {
        StartCoroutine(Fade(0, 1));
    }

    public float GetFadeTime()
    {
        return _fadeTime;
    }

    private IEnumerator Fade(float start, float end)
    {
        float currentTime = 0.0f;
        float percent = 0.0f;

        while (percent < 1)
        {
            currentTime += Time.deltaTime;
            percent = currentTime / _fadeTime;

            Color color = _image.color;
            color.a = Mathf.Lerp(start, end, _fadeCurve.Evaluate(percent));
            _image.color = color;

            yield return null;
        }
    }
}
