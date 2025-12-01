using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class PrologueVideoManager : MonoBehaviour
{
    public MainVideo mainVideo;                  
    public LoadSceneManager loadSceneManager;   
    public FadeInOutManager fadeInOut;          

    public int PROLOGUE_DAY = 100;               //프롤로그 Day 인덱스: 100
    public LANGUAGE language = LANGUAGE.KOREAN; 
    public float firstOverlaySeconds = 2.0f;     //프롤로그 전 로딩 시간

    private const string ProloguePlayedKey = "PROLOGUE_PLAYED";
    private bool _skipTriggered = false;

    // 프롤로그 영상 재생, 튜토리얼로 이동
    public void TryRunPrologueThenGo(string nextSceneIfNotTutorial, bool goingTutorial, string currentSceneName = "IntroScene")
    {
        //PlayerPrefs.DeleteKey("PROLOGUE_PLAYED"); //[디버깅] 매번 프롤로그 재생하도록
        //PlayerPrefs.Save();

        int flag = PlayerPrefs.GetInt(ProloguePlayedKey, 0);
        Debug.Log($"[Prologue] goingTutorial={goingTutorial}, playedFlag={flag}, current={currentSceneName}, nextIfNotTutorial={nextSceneIfNotTutorial}");

        if (goingTutorial && flag == 0)
        {
            Debug.Log("Starting Prologue Video");
            StartCoroutine(RunPrologueFlow(currentSceneName, "Tutorial"));
        }
        else
        {
            Debug.Log("Skipping Prologue Video");
            loadSceneManager.LoadScene(currentSceneName, goingTutorial ? "Tutorial" : nextSceneIfNotTutorial, 0);
        }
    }

    private IEnumerator RunPrologueFlow(string currentSceneName, string targetSceneName)
    {
        yield return loadSceneManager.StartCoroutine(loadSceneManager.ShowDefaultOverlayOnce(firstOverlaySeconds));

        mainVideo.OnUserSkipRequested += HandleUserSkip;

        mainVideo.Setting(PROLOGUE_DAY, language);

        bool done = false;

        void OnVideoEnd(VideoPlayer vp)
        {
            mainVideo.videoPlayer.loopPointReached -= OnVideoEnd;
            done = true;
        }
        mainVideo.videoPlayer.loopPointReached += OnVideoEnd;

        mainVideo.PlayVideo();

        yield return new WaitUntil(() => done || _skipTriggered);

        mainVideo.OnUserSkipRequested -= HandleUserSkip;

        if (_skipTriggered)
        {
            mainVideo.EndGameNow();
        }

        loadSceneManager.LoadScene(currentSceneName, targetSceneName, 0);
    }

    private void HandleUserSkip()
    {
        if (_skipTriggered) return; 
        _skipTriggered = true;
    }
}