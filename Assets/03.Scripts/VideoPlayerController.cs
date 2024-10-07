using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
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
    string[] path;

    [SerializeField]
    VideoPlayer videoPlayer;
    [SerializeField]
    RawImage videoImage;
    [SerializeField]
    GameObject[] loading;

    GameObject video;
    bool isVideoPrepared = false;

    private EVideoIdx eVideoIdx = EVideoIdx.NoVideo; // ���� ��� ���� ���� �ε���
    const string googleURL = "https://drive.google.com/uc?export=download&id=";

    private void Start()
    {
        video = videoPlayer.transform.parent.gameObject;
    }

    public void ShowVideo()
    {
        video.SetActive(true);
        loading[(int)eVideoIdx].SetActive(true);

        if (videoPlayer.isPrepared)
        {
            // ������ �غ�� ��� �ٷ� ��� ����
            StartCoroutine(PlayVideo());
        }
        else
        {
            // ���� �غ� �Ϸ� �� �ݹ� ���
            if(eVideoIdx == EVideoIdx.SkipPhase)
            {
                videoPlayer.loopPointReached += OnVideoFinished;
            }
        }
    }

    public async void CloseVideo(EVideoIdx Idx, bool looping = true)
    {

        await PreloadVideoAsync(Idx, looping);

        if (eVideoIdx == EVideoIdx.NoVideo) return;

        video.SetActive(false);
        loading[(int)eVideoIdx].SetActive(false);

    }
    // ���� �̸� �ε�
    public async Task PreloadVideoAsync(EVideoIdx Idx, bool looping = true)
    {
        if (eVideoIdx != Idx)
        {
            eVideoIdx = Idx;
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = googleURL + path[(int)Idx];
            videoPlayer.isLooping = looping;

            // �񵿱� �غ�
            await PreloadVideoAsync((int)Idx);
        }
    }

    // ���� �̸� �ε� �񵿱� �Լ�
    private async Task PreloadVideoAsync(int Idx)
    {
        // ����� ����
        var audioSource = videoPlayer.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

        // ���� �غ� (�񵿱�)
        await PrepareVideoAsync();
    }

    // ���� �غ� �ϷḦ �񵿱�� ����ϴ� �Լ�
    private Task PrepareVideoAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        videoPlayer.Prepare(); // ���� �غ� ����
        StartCoroutine(CheckPrepared(tcs)); // �ڷ�ƾ���� �غ� ���� üũ

        return tcs.Task;
    }

    // ���� �غ� ���¸� üũ�ϴ� �ڷ�ƾ
    private IEnumerator CheckPrepared(TaskCompletionSource<bool> tcs)
    {
        while (!videoPlayer.isPrepared)
        {
            yield return null; // ������ �غ�� ������ ���
        }

        // �غ� �Ϸ� �� Task�� ���� ���·� ����
        tcs.SetResult(true);

        isVideoPrepared = true;
    }
    // ���� ���
    IEnumerator PlayVideo()
    {
        if (isVideoPrepared)
        {
            // �̸� �غ�� ������ ������ �ٷ� ���
            videoImage.texture = videoPlayer.texture;
            videoPlayer.Play();
            // ���� ��� ���� ���
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }

            isVideoPrepared = false;
        }


    }


    // ���� ����� �Ϸ�Ǹ� ȣ��Ǵ� �ݹ�
    private void OnVideoFinished(VideoPlayer vp)
    {
        // ������ ������ �� GameManager�� �޼��� ȣ��
        gameManager.OnVideoCompleted();

        // �ݹ� ��� ����
        if(eVideoIdx == EVideoIdx.SkipPhase)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}