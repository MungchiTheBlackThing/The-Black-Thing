using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;
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
    
    [SerializeField]
    GameObject video;
    bool isVideoPrepared = false;

    private EVideoIdx eVideoIdx = EVideoIdx.NoVideo; // ���� ��� ���� ���� �ε���
    const string googleURL = "https://drive.google.com/uc?export=download&id=";

    private void Start()
    {
        for(int i=0; i<path.Length; i++)
        {
            StartCoroutine(DownloadPhaseVideo(googleURL + path[i], pathForDocumentsFile(loading[i].name + ".mp4")));
        }
    }

    IEnumerator DownloadPhaseVideo(string url, string path)
    {
        // ������ �̹� �����ϴ��� Ȯ��
        if (File.Exists(path))
        {
            Debug.Log($"Video already exists at: {path}, using existing file.");

            yield break;
        }

        UnityWebRequest www = UnityWebRequest.Get(url);

        Debug.Log("Downloading video...");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Download failed: {www.error}");
        }
        else
        {
            Debug.Log("Download complete, saving to disk...");
            File.WriteAllBytes(path, www.downloadHandler.data);
            Debug.Log($"Video saved to: {path}");
        }
    }

    string pathForDocumentsFile(string filename)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Path.Combine(path, "Documents"), filename);

        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            string path = Application.persistentDataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
        else
        {
            string path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Application.dataPath, filename);
        }
    }


    public void ShowVideo(EVideoIdx Idx, bool looping = true)
    {
        eVideoIdx = Idx;
        video.SetActive(true);
        loading[0].SetActive(true); //�׻� Ų��.

        videoPlayer.isLooping = looping;
        videoImage.texture = videoPlayer.texture;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture; // RenderTexture ���� ����
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 0); // ���ϴ� �ػ󵵷� ����
        videoPlayer.targetTexture = renderTexture;
        videoImage.texture = renderTexture;
        StartCoroutine(PlayVideo(pathForDocumentsFile(loading[(int)eVideoIdx].name + ".mp4")));

        if (eVideoIdx == EVideoIdx.SkipPhase)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
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
        loading[0].SetActive(false);
    }

    // ���� ���
    IEnumerator PlayVideo(string path)
    {

        //if (isVideoPrepared)
        {
            // �̸� �غ�� ������ ������ �ٷ� ���
            videoPlayer.url = path;
            
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            videoPlayer.Play();
            // ���� ��� ���� ���
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }
        }
    }

    // ���� ����� �Ϸ�Ǹ� ȣ��Ǵ� �ݹ�
    private void OnVideoFinished(VideoPlayer vp)
    {
        // ������ ������ �� GameManager�� �޼��� ȣ��
        CloseVideo();
    }
}