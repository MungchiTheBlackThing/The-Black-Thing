using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public enum EVideoIdx
{
    SkipPhase,
    SkipSleeping,
    NoVideo
}

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

    private EVideoIdx eVideoIdx = EVideoIdx.NoVideo;

    public void ShowVideo(EVideoIdx idx, bool looping = true)
    {
        eVideoIdx = idx;

        if (eVideoIdx == EVideoIdx.NoVideo)
        {
            Debug.Log("No video to play.");
            return;
        }

        video.SetActive(true);
        loading[0].SetActive(true); // ù �ε� ������Ʈ �ѱ�

        videoPlayer.isLooping = looping;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        // ���� �ؽ�ó ����
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 0);
        videoPlayer.targetTexture = renderTexture;
        videoImage.texture = renderTexture;

        // enum �̸� �״�� ��� (e.g., SkipPhase �� SkipPhase.mp4)
        string clipName = eVideoIdx.ToString(); // "SkipPhase" or "SkipSleeping"
        VideoClip clip = Resources.Load<VideoClip>($"SkipAnimation/{clipName}");

        if (clip != null)
        {
            videoPlayer.clip = clip;
            videoPlayer.Play();

            if (eVideoIdx == EVideoIdx.SkipPhase)
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
        if (eVideoIdx == EVideoIdx.SkipPhase)
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
