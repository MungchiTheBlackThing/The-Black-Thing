using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class MainVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    [SerializeField] public GameObject Rawimage;

    private bool waitingToPlay = false;

    [SerializeField] GameObject text;
    [SerializeField] int chapter;
    [SerializeField] GameObject background;

    private void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.skipOnDrop = true;
            videoPlayer.isLooping = false;
        }
    }

    public void Setting(int Day, LANGUAGE language)
    {
        chapter = Day;
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer가 설정되지 않았습니다.");
            return;
        }

        Rawimage.SetActive(false);

        string path = $"StoryAnimation/AnimDay{Day}";
        VideoClip clip = Resources.Load<VideoClip>(path);

        if (clip == null)
        {
            Debug.LogError($"Resources에서 VideoClip '{path}' 을(를) 찾을 수 없습니다.");
            return;
        }

        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetDirectAudioMute(0, true);
        videoPlayer.SetDirectAudioVolume(0, 1.0f);

        videoPlayer.clip = clip;
        videoPlayer.gameObject.SetActive(true);
        videoPlayer.Prepare();

        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.prepareCompleted += OnPrepared;

        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.loopPointReached += OnVideoEnd;

        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.errorReceived += OnVideoError;
    }

    public void PlayVideo()
    {
        Debug.Log("영상 시작");
        StartCoroutine(FadeInAndPlay());
    }

    private IEnumerator FadeInAndPlay()
    {
        CanvasGroup bgCg = background.GetComponent<CanvasGroup>();
        if (bgCg == null)
        {
            bgCg = background.AddComponent<CanvasGroup>();
        }

        bgCg.alpha = 0f;
        background.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(bgCg, 0f, 1f, 1f)); // 페이드 인

        Rawimage.transform.SetAsLastSibling();
        Rawimage.SetActive(true);
        text.SetActive(true);

        if (videoPlayer.isPrepared)
        {
            AudioManager.instance.StopBGM();
            videoPlayer.SetDirectAudioMute(0, false);
            videoPlayer.Play();
        }
        else
        {
            waitingToPlay = true;
            StartCoroutine(WaitForVideoToPrepare());
        }
    }

    private IEnumerator WaitForVideoToPrepare()
    {
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        if (waitingToPlay)
        {
            waitingToPlay = false;
            AudioManager.instance.StopBGM();
            videoPlayer.SetDirectAudioMute(0, false);
            videoPlayer.Play();
        }
    }

    private void OnPrepared(VideoPlayer vp)
    {
        Debug.Log("영상 준비 완료됨.");
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        StartCoroutine(FadeOutAndEnd(vp));
    }

    private IEnumerator FadeOutAndEnd(VideoPlayer vp)
    {
        vp.Stop();
        vp.time = 0;
        text.SetActive(false);
        Rawimage.transform.SetAsFirstSibling();
        Rawimage.SetActive(false);

        CanvasGroup bgCg = background.GetComponent<CanvasGroup>();
        if (bgCg == null)
        {
            bgCg = background.AddComponent<CanvasGroup>();
        }

        yield return StartCoroutine(FadeCanvasGroup(bgCg, 1f, 0f, 1f)); // 페이드 아웃
        background.SetActive(false);

        AudioManager.instance.UpdateBGMByChapter(chapter);
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;
        }
        text.SetActive(false);
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("VideoPlayer Error: " + message);
        Rawimage.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            cg.alpha = Mathf.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        cg.alpha = end;
    }
}
