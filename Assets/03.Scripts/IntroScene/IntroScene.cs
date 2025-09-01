using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;

public class IntroScene : MonoBehaviour
{
    [SerializeField] GameObject continueButton;
    [SerializeField] Button btnContinue;
    [SerializeField] Button btnStart;
    [SerializeField] Animator splashAnimator;
    [SerializeField] Animator loadingAnimator;
    [SerializeField] GameObject introGroup;
    [SerializeField] GameObject SettingPopup;
    [SerializeField] GameObject StartPopup;

    const string playerInfoDataFileName = "PlayerData.json";
    RecentData data;
    public PlayerInfo playerInfo;
    GameManager gameManager;

    private void Start()
    {

        playerInfo = new PlayerInfo("Default", 1, GamePatternState.Watching);
        data = RecentManager.Load();
        //1.스플래시 재생
        //2.디폴트 로딩 재생
        //3.인트로 신
        //4.시작하면 에셋 로딩
        btnContinue.enabled = true;
        btnStart.enabled = true;
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
        btnContinue.enabled = false; 
        Play();
    }

    public void OnStart()
    {
        btnStart.enabled = false;
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
        string nextScene;
        //int chapter = playerInfo.chapter;

        if (data != null && data.tutoend == false)
        {
            nextScene = "Tutorial";
            playerInfo.currentPhase = 0;
            WritePlayerFile();
        }
        else
        {
            nextScene = "MainScene";
        }
        LoadSceneManager.Instance.LoadScene("IntroScene", nextScene, 0);
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

    public void WritePlayerFile()
    {
        //PlayerInfo 클래스 내에 플레이어 정보를 Json 형태로 포멧팅 된 문자열 생성
        //만약 player nextchapter라면, 변경
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
