using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Assets.Script.TimeEnum;

[Serializable]
public class VideoPath
{
    [SerializeField] public ChapterDay Time;
    [SerializeField] public LANGUAGE LANGUAGE;
    [SerializeField] public string path;
}

public class MainVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    [SerializeField] List<VideoPath> videoLinks;
    [SerializeField] public GameObject Rawimage;

    public VideoPath videoPath;

    private void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;         // �ڵ� ��� ����
            videoPlayer.skipOnDrop = true;           // ������ ���� ���
            videoPlayer.isLooping = false;
        }
    }

    public void Setting(int Day, LANGUAGE language)
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer�� �������� �ʾҽ��ϴ�.");
            return;
        }

        videoPath = videoLinks.Find(video => video.Time == (ChapterDay)Day && video.LANGUAGE == language);
        if (videoPath == null)
        {
            Debug.LogError("�ش��ϴ� ������ �����ϴ�.");
            return;
        }

        // �̺�Ʈ �ߺ� ����
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.prepareCompleted += OnPrepared;

        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.loopPointReached += OnVideoEnd;

        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.errorReceived += OnVideoError;

        videoPlayer.url = "https://drive.google.com/uc?export=download&id=" + videoPath.path;
        videoPlayer.gameObject.SetActive(true);

        videoPlayer.SetDirectAudioMute(0, true); // �Ҹ� �ϴ� ����
        videoPlayer.Prepare();                  // ���� + �Ҹ� �غ�
    }

    public void PlayVideo()
    {
        Rawimage.transform.SetAsLastSibling();
        Rawimage.SetActive(true);

        if (videoPlayer.isPrepared)
        {
            videoPlayer.SetDirectAudioMute(0, false); // ��� ������ �Ҹ� �ѱ�
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

        videoPlayer.SetDirectAudioMute(0, false); // �غ� �Ϸ� �� �Ҹ� �ѱ�
        videoPlayer.Play();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        Debug.Log("���� �غ� �Ϸ��.");
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
