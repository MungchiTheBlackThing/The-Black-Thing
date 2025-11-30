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

//SkipPhase, SkipSleeping Animation 재생 컨트롤러
public class VideoPlayerController : MonoBehaviour
{
    [SerializeField]
    GameManager gameManager;

    [SerializeField]
    VideoPlayer videoPlayer;

    [SerializeField]
    RawImage videoImage;

    [SerializeField]
    GameObject[] loading;

    [SerializeField]
    GameObject video;

    private SkipVideoIdx skipVideoIdx = SkipVideoIdx.NoVideo;

    //비디오 재생
    public void ShowSkipVideo(SkipVideoIdx idx, bool looping = true)
    {
        skipVideoIdx = idx;

        if (skipVideoIdx == SkipVideoIdx.NoVideo)
        {
            Debug.Log("No video to play.");
            return;
        }

        video.SetActive(true);
        loading[0].SetActive(true); // 첫 로딩 오브젝트 켜기

        videoPlayer.isLooping = looping;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        // 렌더 텍스처 연결
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 0); // 해상도 설정
        videoPlayer.targetTexture = renderTexture; 
        videoImage.texture = renderTexture; 

        // enum 이름 그대로 사용 (e.g., SkipPhase → SkipPhase.mp4)
        string clipName = skipVideoIdx.ToString(); // "SkipPhase" or "SkipSleeping"
        VideoClip clip = Resources.Load<VideoClip>($"SkipAnimation/{clipName}"); // Resources/SkipAnimation 폴더에서 비디오 클립 로드

        if (clip != null)
        {
            videoPlayer.clip = clip; // 비디오 클립 설정
            videoPlayer.Play(); // 비디오 재생

            if (skipVideoIdx == SkipVideoIdx.SkipPhase) // SkipPhase 비디오의 경우 재생이 끝나면 자동으로 창을 닫음.
            {
                videoPlayer.loopPointReached += OnVideoFinished; 
            }
        }
        else
        {
            Debug.LogError($"VideoClip not found: Resources/SkipAnimation/{clipName}");
        }
    }

    public void CloseVideo()
    {
        if (skipVideoIdx == SkipVideoIdx.SkipPhase)
        {
            Close();
        }
    }

    public void Close()
    {
        videoPlayer.Stop();
        loading[0].SetActive(false);
        video.SetActive(false);
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        CloseVideo();
    }
}
