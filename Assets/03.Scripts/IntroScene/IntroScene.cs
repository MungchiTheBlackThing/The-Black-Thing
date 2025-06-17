using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScene : MonoBehaviour
{
    [SerializeField] GameObject continueButton;
    [SerializeField] Animator splashAnimator;
    [SerializeField] Animator loadingAnimator;
    [SerializeField] GameObject introGroup;

    RecentData data;

    private void Start()
    {
        data = RecentManager.Load();
        
        //1.���÷��� ���
        //2.����Ʈ �ε� ���
        //3.��Ʈ�� ��
        //4.�����ϸ� ���� �ε�
        introGroup.gameObject.SetActive(false);
        splashAnimator.gameObject.SetActive(true);
        loadingAnimator.gameObject.SetActive(false);
        StartCoroutine(Wait_Animation(splashAnimator, "SplashAnimation", () =>
        {
            splashAnimator.gameObject.SetActive(false);
            loadingAnimator.gameObject.SetActive(true);

            StartCoroutine(Wait_Animation(loadingAnimator, "DefaultLoadingAnimation", () =>
            {
                loadingAnimator.gameObject.SetActive(false);
                introGroup.SetActive(true);
                continueButton.SetActive(data != null && data.tutoend == true);
            }));
        }));
    }

    public void OnContinue()
    {
        Play();
    }

    public void OnStart()
    {
        RecentManager.ResetFlagOnly();
        data = RecentManager.Load();
        Play();
    }

    public void OnSetting()
    {

    }

    void Play()
    {
        if (data != null && data.tutoend == false)
        {
            SceneManager.LoadScene("Tutorial");
        }
        else
        {
            SceneManager.LoadScene("MainScene");
        }
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
