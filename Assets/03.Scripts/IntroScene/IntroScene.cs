using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScene : MonoBehaviour
{
    [SerializeField] Animator SplashAnimator;
    [SerializeField] Animator LoadingAnimator;

    private void Start()
    {
        RecentData data = RecentManager.Load();
        LoadingAnimator.gameObject.SetActive(false);
        StartCoroutine(Wait_Animation(SplashAnimator, "SplashAnimation", () =>
        {
            LoadingAnimator.gameObject.SetActive(true);
            StopCoroutine("Wait_Animation");
            StartCoroutine(Wait_Animation(LoadingAnimator, "DefaultLoadingAnimation", () =>
            {
                if (data != null && data.tutoend == false)
                {
                    SceneManager.LoadScene("Tutorial");
                }
                else
                {
                    SceneManager.LoadScene("MainScene");
                }
            }));
        }));
    }

    IEnumerator Wait_Animation(Animator animator, string animationName, Action callBack)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        yield return null;

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            yield return null;

        // 애니메이션이 끝날 때까지 대기
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        callBack?.Invoke();
    }
}
