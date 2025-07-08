using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class IntroScene : MonoBehaviour
{
    [SerializeField] GameObject continueButton;
    [SerializeField] Animator splashAnimator;
    [SerializeField] Animator loadingAnimator;
    [SerializeField] GameObject introGroup;
    [SerializeField] GameObject SettingPopup;
    [SerializeField] GameObject StartPopup;

    const string playerInfoDataFileName = "PlayerData.json";
    RecentData data;
    public PlayerInfo playerInfo;

    private void Start()
    {
        playerInfo = new PlayerInfo("Default", 1, GamePatternState.Watching);
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
                continueButton.SetActive(data != null && data.isContinue == 1);
            }));
        }));
    }

    public void OnContinue()
    {
        Play();
    }

    public void OnStart()
    {
        data = RecentManager.Load();
        if (data != null && data.isContinue == 1)
        {
            StartPopup.SetActive(true);
        }
        else
        {
            InitStart();
        }
    }
    public void InitStart()
    {
        RecentManager.ResetFlagOnly();
        playerInfo.Replay();
        WritePlayerFile();
        data = RecentManager.Load();
        Play();
    }

    public void OnSetting()
    {
        SettingPopup.SetActive(true);
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

    public void WritePlayerFile()
    {
        //PlayerInfo Ŭ���� ���� �÷��̾� ������ Json ���·� ������ �� ���ڿ� ����
        //���� player nextchapter���, ����
        playerInfo.currentPhase = playerInfo.currentPhase == GamePatternState.NextChapter ? GamePatternState.Watching : playerInfo.currentPhase;
        string jsonData = JsonUtility.ToJson(playerInfo);
        string path = pathForDocumentsFile(playerInfoDataFileName);
        File.WriteAllText(path, jsonData);
    }

    string pathForDocumentsFile(string filename)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Path.Combine(path, "Documents"), filename);

        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            string path = Application.persistentDataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
        else
        {
            string path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Application.dataPath, filename);
        }
    }
}
