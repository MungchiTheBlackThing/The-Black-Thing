using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Assets.Script.TimeEnum;
using System.Collections;

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

    private void Start() { }

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

        // URL ����
        videoPlayer.url = "https://drive.google.com/uc?export=download&id=" + videoPath.path;
        videoPlayer.gameObject.SetActive(true);

        // ���� �غ� ����
        videoPlayer.Prepare();
    }

    public void PlayVideo()
    {
        Rawimage.transform.SetAsLastSibling();
        Rawimage.SetActive(true);

        if (videoPlayer.isPrepared)
        {
            // �ٿ�ε尡 �Ϸ�Ǿ����� ���
            videoPlayer.Play();
        }
        else
        {
            // �ٿ�ε尡 �Ϸ�� ������ ��ٸ���
            StartCoroutine(WaitForVideoToPrepare());
        }
    }

    // ������ �غ�� ������ ��ٸ��� �ڷ�ƾ
    private IEnumerator WaitForVideoToPrepare()
    {
        // ���� �غ� �Ϸ�� ������ ��ٸ���
        while (!videoPlayer.isPrepared)
        {
            yield return null; // �� �����Ӿ� ��ٸ���
        }

        // �غ� �Ϸ� �� ���
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
