using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class MainVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    [SerializeField] public GameObject Rawimage;

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
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer가 설정되지 않았습니다.");
            return;
        }

        // 경로에 맞춰 VideoClip 로드
        string path = $"StoryAnimation/AnimDay{Day}";
        VideoClip clip = Resources.Load<VideoClip>(path);

        if (clip == null)
        {
            Debug.LogError($"Resources에서 VideoClip '{path}' 을(를) 찾을 수 없습니다.");
            return;
        }

        // 이벤트 중복 방지
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.prepareCompleted += OnPrepared;

        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.loopPointReached += OnVideoEnd;

        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.errorReceived += OnVideoError;

        videoPlayer.clip = clip;
        videoPlayer.gameObject.SetActive(true);

        videoPlayer.SetDirectAudioMute(0, true); // 소리 끄기
        videoPlayer.Prepare(); // 영상 준비
    }

    public void PlayVideo()
    {
        Rawimage.transform.SetAsLastSibling();
        Rawimage.SetActive(true);

        if (videoPlayer.isPrepared)
        {
            videoPlayer.SetDirectAudioMute(0, false);
            videoPlayer.Play();
        }
        else
        {
            StartCoroutine(WaitForVideoToPrepare());
        }
    }

    private IEnumerator WaitForVideoToPrepare()
    {
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.SetDirectAudioMute(0, false);
        videoPlayer.Play();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        Debug.Log("영상 준비 완료됨.");
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        vp.Stop();
        vp.time = 0;

        Rawimage.transform.SetAsFirstSibling();
        Rawimage.SetActive(false);
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;
        }
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("VideoPlayer Error: " + message);
        Rawimage.SetActive(false);
    }
}
