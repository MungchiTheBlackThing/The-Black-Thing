using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [SerializeField] AudioSource soundEffect;
    [SerializeField] AudioSource popupButtonSound;

    RecentData data;
    public PlayerInfo playerInfo;
    GameManager gameManager;

    private void Start()
    {
        if (PlayerPrefs.GetInt("MIGRATED_PLAYERDATA_V1", 0) == 0)
        {
            SavePaths.MigrateLegacyPlayerDataIfNeeded();
            PlayerPrefs.SetInt("MIGRATED_PLAYERDATA_V1", 1);
            PlayerPrefs.Save();
        }
        if (PlayerPrefs.GetInt("FORCE_NEW_GAME", 0) == 1)
        {
            PlayerPrefs.DeleteKey("FORCE_NEW_GAME");
            PlayerPrefs.Save();

            InitStart(); // "새로시작하기 누르는 것처럼 InitPlay()" 루트
            return;      // InitStart 안에서 씬 넘기는 흐름이면 여기서 끊기
        }
        AudioManager.Instance.PlayBGM(FMODEvents.Instance.bgm_intro);
        playerInfo = new PlayerInfo("Default", 1, GamePatternState.Watching);
        data = RecentManager.Load();
        //1.스플래시 재생
        //2.디폴트 로딩 재생
        //3.인트로 신
        //4.시작하면 에셋 로딩
        btnContinue.interactable = true;
        btnStart.interactable = true;
        introGroup.gameObject.SetActive(false);
        splashAnimator.gameObject.SetActive(true);
        loadingAnimator.gameObject.SetActive(false);

        soundEffect.Play();
        StartCoroutine(Wait_Animation(splashAnimator, "SplashAnimation", () =>
        {
            splashAnimator.gameObject.SetActive(false);
            loadingAnimator.gameObject.SetActive(true);
            introGroup.SetActive(true);

            StartCoroutine(Wait_Animation(loadingAnimator, "DefaultLoadingAnimation", () =>
            {
                loadingAnimator.gameObject.SetActive(false);
                continueButton.SetActive(data != null && data.isContinue == 1);
            }));
        }));
    }

    public void OnContinue()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, Vector3.zero);
        btnContinue.interactable = false; 
        Play();
    }

    public void OnStart()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, Vector3.zero);
        Debug.Log("OnStart");
        btnStart.interactable = false;
        data = RecentManager.Load();
        if (data != null && data.isContinue == 1)
        {
            StartPopup.SetActive(true);
            btnStart.interactable = true;
        }
        else
        {
            InitStart();
        }
    }
    public void InitStart()
    {
        PlayerPrefs.DeleteKey("PROLOGUE_PLAYED"); //프롤로그 다시 재생하도록 초기화
        // AfterScript 복원용 PlayerPrefs 청소
        PlayerPrefs.DeleteKey("AS_AnimKey");
        PlayerPrefs.DeleteKey("AS_Pos");
        PlayerPrefs.DeleteKey("AS_IsPlaying");
        PlayerPrefs.Save();
        
        RecentManager.ResetFlagOnly();
        playerInfo.Replay();
        
        PlayerController.ClearAllSubTimestampsForNewGame();
        WritePlayerFile();
        data = RecentManager.Load();
        Play();
    }

    public void PlayPopupButtonSound()
    {
        popupButtonSound.Play();
    }

    public void OnSetting()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, Vector3.zero);
        SettingPopup.SetActive(true);
    }

    public void Play()
    {
        string nextScene;
        bool goingTutorial;

        if (data != null && data.tutoend == false)
        {
            nextScene = "Tutorial";
            goingTutorial = true;
            playerInfo.currentPhase = GamePatternState.Watching; // (enum 0)
            //WritePlayerFile();
        }
        else
        {
            nextScene = "MainScene";
            goingTutorial = false;
        }
        
        var p = FindObjectOfType<PrologueVideoManager>(true);
        if (p != null)
        {
            p.TryRunPrologueThenGo(nextSceneIfNotTutorial: "MainScene", goingTutorial: goingTutorial, currentSceneName: "IntroScene");
        }
        else
        {
            LoadSceneManager.Instance.LoadScene("IntroScene", nextScene, 0);
        }
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
        playerInfo.currentPhase =
            playerInfo.currentPhase == GamePatternState.NextChapter
                ? GamePatternState.Watching
                : playerInfo.currentPhase;

        string jsonData = JsonUtility.ToJson(playerInfo);

        // SavePaths로 통일 + atomic write
        SavePaths.WriteAllTextAtomic(SavePaths.PlayerDataPath, jsonData);
    }

}
