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
        LoadingAnimator.gameObject.SetActive(false);
        StartCoroutine(Wait_Animation(SplashAnimator, "SplashAnimation", () =>
        {
            LoadingAnimator.gameObject.SetActive(true);
            StopCoroutine("Wait_Animation");
            StartCoroutine(Wait_Animation(LoadingAnimator, "DefaultLoadingAnimation", () =>
            {
                //���� ������ �ѱ�� �� ó��
                SceneManager.LoadScene("MainScene");
            }));
        }));
    }

    IEnumerator Wait_Animation(Animator animator, string animationName, Action callBack)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        yield return null;

        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
            yield return null;

        // �ִϸ��̼��� ���� ������ ���
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;

        callBack?.Invoke();
    }
}
