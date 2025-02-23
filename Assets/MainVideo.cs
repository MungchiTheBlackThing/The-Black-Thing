using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script.TimeEnum;
using System;
using UnityEngine.Video;

[Serializable]
public class VideoPath
{
    [SerializeField]
    public ChapterDay Time;
    [SerializeField]
    public LANGUAGE LANGUAGE;
    [SerializeField]
    public string path;
}
public class MainVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    [SerializeField] List<VideoPath> videoLinks;
    [SerializeField] public GameObject Rawimage;
    public VideoPath videoPath;
    // Start is called before the first frame update
    public void Start()
    {
        
    }
    // Update is called once per frame
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
        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.loopPointReached += OnVideoEnd;

        videoPlayer.url = "https://drive.google.com/uc?export=download&id=" + videoPath.path;
        videoPlayer.gameObject.SetActive(true);

        videoPlayer.Prepare();
    }
    public void PlayVideo()
    {
        Rawimage.transform.SetAsLastSibling();
        Rawimage.SetActive(true); // UI Ȱ��ȭ
        videoPlayer.prepareCompleted += (vp) => vp.Play();
    }
    private void OnVideoEnd(VideoPlayer vp)
    {
        //videoPlayer.gameObject.SetActive(false);
        Rawimage.transform.SetAsFirstSibling();
        Rawimage.SetActive(false);
    }
    private void OnDisable()
    {
        
    }
}
