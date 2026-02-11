using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public enum SkipVideoIdx
{
    SkipPhase,
    SkipSleeping,
    NoVideo
}

// SkipPhase, SkipSleeping Animation 재생 컨트롤러
public class VideoPlayerController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoImage;
    [SerializeField] private GameObject[] loading;
    [SerializeField] private GameObject video; // 루트 오브젝트 (캔버스 패널)
    [SerializeField] private CanvasGroup videoCanvasGroup; // 없으면 자동 추가
    [SerializeField] private GraphicRaycaster videoGraphicRaycaster;


    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.25f;

    public event Action<SkipVideoIdx> OnClosed;

    private SkipVideoIdx currentIdx = SkipVideoIdx.NoVideo;
    private Coroutine fadeCo;
    private Action onFadeInCommit; // 페이드인 끝난 직후 1회 커밋
    private bool committedThisShow = false;

    private Coroutine showImageCo;

    public SkipVideoIdx CurrentIdx => currentIdx;
    public bool IsShowing => (video != null && video.activeSelf);

    private void Awake()
    {
        if (video != null)
        {
            if (videoCanvasGroup == null) videoCanvasGroup = video.GetComponent<CanvasGroup>();
            if (videoCanvasGroup == null) videoCanvasGroup = video.AddComponent<CanvasGroup>();
            if (videoGraphicRaycaster == null) videoGraphicRaycaster = video.GetComponent<GraphicRaycaster>();
            videoCanvasGroup.alpha = 0f;
            videoCanvasGroup.blocksRaycasts = false;
            if (videoGraphicRaycaster != null) videoGraphicRaycaster.enabled = false;
        }

        videoPlayer.waitForFirstFrame = true;

        videoPlayer.playOnAwake = false;
    }

    /// <summary>
    /// 단순 표시(NextChapter 유지용): 커밋 없음
    /// </summary>
    public void ShowSkipVideo(SkipVideoIdx idx, bool looping = true)
    {
        PlayInternal(idx, looping, commitAfterFadeIn: null, autoCloseOnEnd: false);
    }

    /// <summary>
    /// 스킵 트랜지션용: 페이드인 끝나자마자 commitAfterFadeIn() 1회 호출
    /// SkipPhase는 영상 종료 시 자동 Close
    /// SkipSleeping은 루프라 autoCloseOnEnd = false
    /// </summary>
    public void PlaySkipTransition(SkipVideoIdx idx, bool looping, Action commitAfterFadeIn, bool autoCloseOnEnd)
    {
        PlayInternal(idx, looping, commitAfterFadeIn, autoCloseOnEnd);
    }

    private void PlayInternal(SkipVideoIdx idx, bool looping, Action commitAfterFadeIn, bool autoCloseOnEnd)
    {
        if (idx == SkipVideoIdx.NoVideo)
            return;

        // 이미 같은 영상이 떠 있으면 "커밋만" 업데이트 할지 여부 결정:
        // - NextChapter 유지용(ShowSkipVideo)은 그냥 무시
        // - 트랜지션(PlaySkipTransition)은 커밋이 필요하니 hook만 갱신
        if (IsShowing && currentIdx == idx)
        {
            onFadeInCommit = commitAfterFadeIn;
            return;
        }

        currentIdx = idx;
        onFadeInCommit = commitAfterFadeIn;
        committedThisShow = false;

        if (videoPlayer == null || video == null)
        {
            Debug.LogError("[VideoPlayerController] Missing refs.");
            return;
        }

        // 상태/이벤트 정리
        videoPlayer.loopPointReached -= OnVideoFinished;
        if (autoCloseOnEnd)
            videoPlayer.loopPointReached += OnVideoFinished;

        videoPlayer.isLooping = looping;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        if (loading != null && loading.Length > 0 && loading[0])
            loading[0].SetActive(true);

        video.SetActive(true);
        InputGuard.WorldInputLocked = true;
        if (videoCanvasGroup != null) videoCanvasGroup.blocksRaycasts = true;
        if (videoGraphicRaycaster != null) videoGraphicRaycaster.enabled = true;

        if (videoImage != null) videoImage.enabled = false;

        // clip 로드
        string clipName = idx.ToString();
        VideoClip clip = Resources.Load<VideoClip>($"SkipAnimation/{clipName}");
        if (clip == null)
        {
            Debug.LogError($"[VideoPlayerController] VideoClip not found: Resources/SkipAnimation/{clipName}");
            Close();
            return;
        }

        videoPlayer.clip = clip;
        videoPlayer.Play();

        if (showImageCo != null) StopCoroutine(showImageCo);
        showImageCo = StartCoroutine(ShowVideoImageNextFrame());    

        // 페이드인 시작
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeInThenCommit());
    }

    private IEnumerator ShowVideoImageNextFrame()
    {
        yield return null; // 딱 1프레임 기다림
        if (videoImage != null) videoImage.enabled = true;
        showImageCo = null;
    }

    private IEnumerator FadeInThenCommit()
    {
        if (videoCanvasGroup == null && video != null)
        {
            videoCanvasGroup = video.GetComponent<CanvasGroup>();
            if (videoCanvasGroup == null) videoCanvasGroup = video.AddComponent<CanvasGroup>();
        }

        if (videoCanvasGroup != null)
        {
            float t = 0f;
            videoCanvasGroup.alpha = 0f;
            while (t < fadeInDuration)
            {
                t += Time.unscaledDeltaTime;
                videoCanvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
                yield return null;
            }
            videoCanvasGroup.alpha = 1f;
        }
        else
        {
            yield return null;
        }

        // 페이드인 끝난 직후 1회 커밋
        if (!committedThisShow && onFadeInCommit != null)
        {
            committedThisShow = true;
            onFadeInCommit.Invoke();
        }

        fadeCo = null;
    }

    /// <summary>
    /// NextChapter 루프 영상 등: 외부에서 닫을 때
    /// </summary>
    public void Close()
    {
        if (fadeCo != null)
        {
            StopCoroutine(fadeCo);
            fadeCo = null;
        }

        if (videoPlayer != null) videoPlayer.Stop();
        if (loading != null && loading.Length > 0 && loading[0]) loading[0].SetActive(false);
        if (videoCanvasGroup != null)
        {
            videoCanvasGroup.alpha = 0f;
            videoCanvasGroup.blocksRaycasts = false;
        }
        if (videoGraphicRaycaster != null) videoGraphicRaycaster.enabled = false;
        if (video != null) video.SetActive(false);
        InputGuard.WorldInputLocked = false;

        var closedIdx = currentIdx;
        currentIdx = SkipVideoIdx.NoVideo;
        onFadeInCommit = null;
        committedThisShow = false;

        if (showImageCo != null) { StopCoroutine(showImageCo); showImageCo = null; }
        if (videoImage != null) videoImage.enabled = false;

        OnClosed?.Invoke(closedIdx);
    }

    public void CloseIfShowing(SkipVideoIdx idx)
    {
        if (IsShowing && currentIdx == idx)
            Close();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // SkipPhase만 여기 걸어둘 것(autoCloseOnEnd=true로 호출된 경우)
        Close();
    }
}
