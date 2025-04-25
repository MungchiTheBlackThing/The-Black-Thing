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
            Debug.LogError("VideoPlayer가 설정되지 않았습니다.");
            return;
        }

        videoPath = videoLinks.Find(video => video.Time == (ChapterDay)Day && video.LANGUAGE == language);
        if (videoPath == null)
        {
            Debug.LogError("해당하는 영상이 없습니다.");
            return;
        }

        // 이벤트 중복 방지
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.prepareCompleted += OnPrepared;

        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.loopPointReached += OnVideoEnd;

        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.errorReceived += OnVideoError;

        // URL 설정
        videoPlayer.url = "https://drive.google.com/uc?export=download&id=" + videoPath.path;
        videoPlayer.gameObject.SetActive(true);

        // 영상 준비 시작
        videoPlayer.Prepare();
    }

    public void PlayVideo()
    {
        Rawimage.transform.SetAsLastSibling();
        Rawimage.SetActive(true);

        if (videoPlayer.isPrepared)
        {
            // 다운로드가 완료되었으면 재생
            videoPlayer.Play();
        }
        else
        {
            // 다운로드가 완료될 때까지 기다리기
            StartCoroutine(WaitForVideoToPrepare());
        }
    }

    // 비디오가 준비될 때까지 기다리는 코루틴
    private IEnumerator WaitForVideoToPrepare()
    {
        // 영상 준비가 완료될 때까지 기다리기
        while (!videoPlayer.isPrepared)
        {
            yield return null; // 한 프레임씩 기다리기
        }

        // 준비 완료 후 재생
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
