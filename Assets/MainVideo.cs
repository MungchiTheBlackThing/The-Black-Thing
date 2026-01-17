using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainVideo : MonoBehaviour
{
    private GameManager gm;
    public VideoPlayer videoPlayer;
    [SerializeField] public GameObject Rawimage;
    [SerializeField] public RawImage videoDisplayImage;

    private bool waitingToPlay = false;

    [SerializeField] GameObject text;
    [SerializeField] int chapter;
    [SerializeField] GameObject background;

    private TMP_Text subtitleText;
    private LANGUAGE currentLanguage = LANGUAGE.KOREAN;
    private List<VideoSubtitleEntry> entries;
    private int lastIndex = -1;
    private double[] startsCache;

    [SerializeField] private bool useLocalization = true;
    private string subtitleTableName;
    private string[] entryKeys;
    private string[] localizedCache;

    [Header("Skip Hint")]
    [SerializeField] private GameObject skipHintRoot;
    [SerializeField] private TMP_Text skipHintText;
    [SerializeField] private float skipHintFadeDuration = 2f;

    [Header("Replay")]
    [SerializeField] Button replayButton;
    [SerializeField] Button nextButton;

    [Header("Cloudinary Settings")]
    [SerializeField] private bool useCloudinary = true;
    [SerializeField] private float videoLoadTimeout = 30f;
    [SerializeField] private GameObject loadingIndicator;
    
    private bool isVideoPlaying = false;
    private bool skipArmed = false;
    private bool allowSkip = false;
    private Coroutine skipHintFadeCo;
    private Coroutine videoLoadCoroutine;
    public event System.Action OnUserSkipRequested;
    Action videoEndEvent = null;

    private readonly Dictionary<int, string> cloudinaryVideoUrls = new Dictionary<int, string>
    {
        {1, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654151/AnimDay1_u5nemw.mp4"},
        {2, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654173/AnimDay2_ds8ep8.mp4"},
        {3, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654141/AnimDay3_gzvoz3.mp4"},
        {4, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654182/AnimDay4_sz8lxm.mp4"},
        {5, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654182/AnimDay5_ur14px.mp4"},
        {6, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654133/AnimDay6_hpnl7n.mp4"},
        {7, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654131/AnimDay7_kn8pzm.mp4"},
        {8, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654170/AnimDay8_kl1rbk.mp4"},
        {9, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654114/AnimDay9_obplkk.mp4"},
        {10, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654119/AnimDay10_uuyc2t.mp4"},
        {11, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654126/AnimDay11_ram7bo.mp4"},
        {12, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654192/AnimDay12_wvopoz.mp4"},
        {13, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654151/AnimDay13_pqxqhe.mp4"},
        {14, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654133/AnimDay14_rcznx2.mp4"},
        {100, "https://res.cloudinary.com/dfcadcwpk/video/upload/v1768654159/AnimDay100_vskqat.mp4"}
    };

    private void Start()
    {
        GameObject gc = GameObject.FindWithTag("GameController");
        if (gc != null)
        {
            gm = gc.GetComponent<GameManager>();
        }

        replayButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.skipOnDrop = true;
            videoPlayer.isLooping = false;
        }
        
        if (Rawimage != null)
            Rawimage.SetActive(false);
            
        if (text != null)
            subtitleText = text.GetComponentInChildren<TMP_Text>(true);

        if (videoDisplayImage == null && Rawimage != null)
            videoDisplayImage = Rawimage.GetComponent<RawImage>();

        HideSkipHintImmediate();
        
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
    }

    public void Setting(int Day, LANGUAGE language)
    {
        chapter = Day;
        currentLanguage = language;

        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer is not set.");
            return;
        }

        subtitleTableName = $"SrcDay{Day}";
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged_Subtitle;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged_Subtitle;

        if (Rawimage != null)
            Rawimage.SetActive(false);

        if (videoLoadCoroutine != null)
        {
            StopCoroutine(videoLoadCoroutine);
            videoLoadCoroutine = null;
        }

        videoPlayer.Stop();
        videoPlayer.clip = null;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = string.Empty;

        ConfigureVideoPlayerForFullScreen();

        entries = MainVideoCsvLoader.Load(chapter, 30.0);
        entryKeys = (entries != null) ? BuildEntryKeys(Day, entries.Count) : null;
        RebuildLocalizedCache();

        lastIndex = -1;
        startsCache = null;
        EnsureSubtitleText();
        if (subtitleText != null) subtitleText.text = "";

        videoPlayer.gameObject.SetActive(true);

        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.prepareCompleted += OnPrepared;

        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.loopPointReached += OnVideoEnd;

        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.errorReceived += OnVideoError;

        videoPlayer.seekCompleted -= OnSeekCompleted;
        videoPlayer.seekCompleted += OnSeekCompleted;
    }

    private void ConfigureVideoPlayerForFullScreen()
    {
        videoPlayer.playOnAwake = false;
        videoPlayer.playbackSpeed = 1f;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetDirectAudioMute(0, true);
        videoPlayer.SetDirectAudioVolume(0, 1.0f);
        
        videoPlayer.aspectRatio = VideoAspectRatio.Stretch;
        
        if (videoDisplayImage != null)
        {
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            if (videoPlayer.targetTexture == null)
            {
                videoPlayer.targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
            }
            videoDisplayImage.texture = videoPlayer.targetTexture;
            videoDisplayImage.color = Color.white;
        }
        else
        {
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            videoPlayer.targetCamera = Camera.main;
            videoPlayer.targetCameraAlpha = 1f;
        }
    }

    public void PlayVideo(Action videoEndEvent = null)
    {
        this.videoEndEvent = videoEndEvent;
        allowSkip = false;
        Debug.Log("Video start");
        replayButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        
        StartCoroutine(LoadAndPlayVideo());
    }

    private IEnumerator LoadAndPlayVideo()
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        CanvasGroup bgCg = background.GetComponent<CanvasGroup>() ?? background.AddComponent<CanvasGroup>();
        bgCg.alpha = 0f;
        background.SetActive(true);
        
        yield return StartCoroutine(FadeCanvasGroup(bgCg, 0f, 1f, 1f));

        string videoUrl = GetVideoUrlForChapter(chapter);
        
        if (string.IsNullOrEmpty(videoUrl))
        {
            Debug.LogError($"No video URL found for chapter {chapter}");
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
            yield break;
        }

        Debug.Log($"Loading video from: {videoUrl}");
        
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoUrl;
        
        videoPlayer.Prepare();
        
        float prepareStartTime = Time.time;
        while (!videoPlayer.isPrepared)
        {
            if (Time.time - prepareStartTime > videoLoadTimeout)
            {
                Debug.LogError($"Video preparation timeout after {videoLoadTimeout} seconds");
                if (loadingIndicator != null)
                    loadingIndicator.SetActive(false);
                yield break;
            }
            yield return null;
        }
        
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
        
        if (Rawimage != null)
            Rawimage.SetActive(true);
            
        if (text != null)
            text.SetActive(true);
            
        HideSkipHintImmediate();

        if (videoDisplayImage != null)
        {
            videoDisplayImage.texture = videoPlayer.targetTexture;
        }

        if (AudioManager.Instance != null) 
            AudioManager.Instance.StopBGM();
        else 
            Debug.LogWarning("[MainVideo] AudioManager.instance is null");
            
        videoPlayer.SetDirectAudioMute(0, false);
        isVideoPlaying = true;
        videoPlayer.Play();
    }

    private string GetVideoUrlForChapter(int chapter)
    {
        if (useCloudinary && cloudinaryVideoUrls.ContainsKey(chapter))
        {
            return cloudinaryVideoUrls[chapter];
        }
        else
        {
            Debug.LogWarning($"Using local resource fallback for chapter {chapter}");
            string path = $"StoryAnimation/AnimDay{chapter}";
            VideoClip clip = Resources.Load<VideoClip>(path);
            if (clip != null)
            {
                videoPlayer.source = VideoSource.VideoClip;
                videoPlayer.clip = clip;
                return null;
            }
            else
            {
                Debug.LogError($"No video found for chapter {chapter} in resources or Cloudinary");
                return null;
            }
        }
    }

    private void OnPrepared(VideoPlayer vp)
    {
        Debug.Log("Video prepared successfully.");
        lastIndex = -1;
        if (subtitleText != null) subtitleText.text = "";
        waitingToPlay = false;
    }

    private void OnSeekCompleted(VideoPlayer vp)
    {
        lastIndex = -1;
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        isVideoPlaying = false;
        replayButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);
    }

    public void OnReplay()
    {
        Debug.Log("[MainVideo] Replay requested");

        StopAllCoroutines();

        isVideoPlaying = false;
        waitingToPlay = false;
        HideSkipHintImmediate();

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;
            videoPlayer.frame = 0;
            videoPlayer.SetDirectAudioMute(0, true);
        }

        PlayerPrefs.SetInt("PROLOGUE_SKIP_ENABLED", 1);
        PlayerPrefs.Save();

        PlayVideo(videoEndEvent);
        allowSkip = true;
    }

    public void OnNext()
    {
        PlayerPrefs.SetInt("PROLOGUE_PLAYED", 1);
        PlayerPrefs.Save();
        
        isVideoPlaying = false;
        HideSkipHintImmediate();
        StartCoroutine(FadeOutAndEnd(videoPlayer));
        replayButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        videoEndEvent?.Invoke();
    }

    private IEnumerator FadeOutAndEnd(VideoPlayer vp)
    {
        vp.Stop();
        vp.time = 0;
        
        if (text != null)
            text.SetActive(false);
            
        if (Rawimage != null)
            Rawimage.SetActive(false);

        var bgCg = background.GetComponent<CanvasGroup>() ?? background.AddComponent<CanvasGroup>();
        yield return StartCoroutine(FadeCanvasGroup(bgCg, 1f, 0f, 1f));
        background.SetActive(false);

        if (AudioManager.Instance != null && gm != null)
        {
            AudioManager.Instance.UpdateBGMByChapter(gm.Chapter, gm.Pattern);
        }
        else 
        {
            Debug.LogWarning("[MainVideo] AudioManager.instance is null or GameManager not found");
        }
        
        vp.url = string.Empty;
        vp.clip = null;
    }

    private void OnDisable()
    {
        isVideoPlaying = false;
        HideSkipHintImmediate();
        
        if (videoLoadCoroutine != null)
        {
            StopCoroutine(videoLoadCoroutine);
            videoLoadCoroutine = null;
        }
        
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;
        }

        if (subtitleText != null) subtitleText.text = "";
        
        if (text != null)
            text.SetActive(false);
        
        if (gm != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateBGMByChapter(gm.Chapter, gm.Pattern);
        }
        
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged_Subtitle;
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("VideoPlayer Error: " + message);
        
        if (Rawimage != null)
            Rawimage.SetActive(false);
            
        if (subtitleText != null) subtitleText.text = "";
        
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            cg.alpha = Mathf.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        cg.alpha = end;
    }

    private void Update()
    {
        if (videoPlayer == null || entries == null || entries.Count == 0 || !videoPlayer.isPlaying) return;
        if (subtitleText == null) return;

        double t = videoPlayer.time;
        int idx = FindIndexAtTime(t);

        if (idx != lastIndex)
        {
            lastIndex = idx;
            if (idx >= 0)
            {
                var e = entries[idx];
                string subtitleContent = "";
                string loc = (useLocalization) ? TryGetLocalized(idx) : null;
                subtitleContent = !string.IsNullOrEmpty(loc)
                    ? loc
                    : ((currentLanguage == LANGUAGE.ENGLISH) ? e.EngText : e.KorText);

                subtitleText.text = subtitleContent;

                Debug.Log($"[Subtitle] Time: {t:F3}s, Index: {idx}, Start: {e.Start:F3}s, End: {e.End:F3}s, Text: {subtitleContent}");
            }
            else
            {
                subtitleText.text = "";
            }
        }
        
        if (isVideoPlaying && allowSkip)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!skipArmed)
                {
                    ShowSkipHint();
                }
                else
                {
                    OnUserSkipRequested?.Invoke();
                    OnNext();
                }
            }
        }
    }

    private int FindIndexAtTime(double t)
    {
        if (lastIndex >= 0 && lastIndex < entries.Count)
        {
            var cur = entries[lastIndex];
            if (cur.Start <= t && t < cur.End) return lastIndex;

            if (t < cur.Start)
            {
                for (int i = Mathf.Max(0, lastIndex - 3); i < lastIndex; i++)
                {
                    var e = entries[i];
                    if (e.Start <= t && t < e.End) return i;
                    if (t < e.Start) break;
                }
            }
            else
            {
                for (int i = lastIndex + 1; i < Mathf.Min(entries.Count, lastIndex + 4); i++)
                {
                    var e = entries[i];
                    if (e.Start <= t && t < e.End) return i;
                    if (t < e.Start) break;
                }
            }
        }

        if (startsCache == null || startsCache.Length != entries.Count)
        {
            startsCache = new double[entries.Count];
            for (int i = 0; i < entries.Count; i++) startsCache[i] = entries[i].Start;
        }

        int lo = 0, hi = startsCache.Length - 1, cand = -1;
        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            if (startsCache[mid] <= t)
            {
                cand = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }

        if (cand >= 0)
        {
            var e = entries[cand];
            if (e.Start <= t && t < e.End) return cand;
        }
        return -1;
    }

    private void EnsureSubtitleText()
    {
        if (subtitleText == null && text != null)
            subtitleText = text.GetComponentInChildren<TMP_Text>(true);
    }

    private void OnDestroy()
    {
        if (videoLoadCoroutine != null)
        {
            StopCoroutine(videoLoadCoroutine);
        }
        
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.url = string.Empty;
            videoPlayer.clip = null;
        }
        
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged_Subtitle;
    }

    private string[] BuildEntryKeys(int day, int count)
    {
        if (count <= 0) return null;
        var arr = new string[count];
        for (int i = 0; i < count; i++)
            arr[i] = $"SD{day}_L{(i + 1):0000}";
        return arr;
    }

    private void RebuildLocalizedCache()
    {
        if (!useLocalization || string.IsNullOrEmpty(subtitleTableName) || entryKeys == null)
        {
            localizedCache = null;
            return;
        }

        if (localizedCache == null || localizedCache.Length != entryKeys.Length)
            localizedCache = new string[entryKeys.Length];

        for (int i = 0; i < entryKeys.Length; i++)
        {
            var key = entryKeys[i];
            string s = string.Empty;

            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    s = LocalizationSettings.StringDatabase.GetLocalizedString(subtitleTableName, key);
                }
                catch { s = string.Empty; }
            }

            localizedCache[i] = s;
        }
    }

    private string TryGetLocalized(int idx)
    {
        if (!useLocalization || localizedCache == null || idx < 0 || idx >= localizedCache.Length)
            return null;

        var cached = localizedCache[idx];
        if (!string.IsNullOrEmpty(cached)) return cached;

        try
        {
            var key = (entryKeys != null && idx < entryKeys.Length) ? entryKeys[idx] : null;
            if (!string.IsNullOrEmpty(key))
            {
                string s = LocalizationSettings.StringDatabase.GetLocalizedString(subtitleTableName, key);
                if (!string.IsNullOrEmpty(s))
                {
                    localizedCache[idx] = s;
                    return s;
                }
            }
        }
        catch { }

        return null;
    }

    private void OnLocaleChanged_Subtitle(Locale _)
    {
        RebuildLocalizedCache();

        if (subtitleText != null && lastIndex >= 0 && entries != null && lastIndex < entries.Count)
        {
            var e = entries[lastIndex];
            string loc = TryGetLocalized(lastIndex);
            subtitleText.text = !string.IsNullOrEmpty(loc)
                ? loc
                : ((currentLanguage == LANGUAGE.ENGLISH) ? e.EngText : e.KorText);
        }
    }

    private void ShowSkipHint()
    {
        skipArmed = true;

        if (skipHintRoot == null)
        {
            Debug.LogWarning("[MainVideo] skipHintRoot is null.");
            return;
        }
        var cg = skipHintRoot.GetComponent<CanvasGroup>() ?? skipHintRoot.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        skipHintRoot.SetActive(true);

        if (skipHintFadeCo != null)
        {
            StopCoroutine(skipHintFadeCo);
            skipHintFadeCo = null;
        }

        skipHintFadeCo = StartCoroutine(FadeOutSkipHint(cg, skipHintFadeDuration));
    }

    private IEnumerator FadeOutSkipHint(CanvasGroup cg, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / duration);
            yield return null;
        }
        cg.alpha = 0f;

        if (skipHintRoot != null) skipHintRoot.SetActive(false);
        skipArmed = false;
        skipHintFadeCo = null;
    }

    private void HideSkipHintImmediate()
    {
        if (skipHintFadeCo != null)
        {
            StopCoroutine(skipHintFadeCo);
            skipHintFadeCo = null;
        }
        if (skipHintRoot != null)
        {
            var cg = skipHintRoot.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0f;
            skipHintRoot.SetActive(false);
        }
        skipArmed = false;
    }

    public void SetAllowSkipForPrologue(bool enable)
    {
        allowSkip = enable;
        Debug.Log($"[MainVideo] Prologue skip enabled: {enable}");
    }
}

[System.Serializable]
public class VideoSubtitleEntry
{
    public double Start;
    public double End;
    public double Duration;
    public string KorText;
    public string EngText;
}

public static class MainVideoCsvLoader
{
    public static List<VideoSubtitleEntry> Load(int day, double fps)
    {
        string path = $"CSV/main_video_csv/SrcDay{day}";
        TextAsset csv = Resources.Load<TextAsset>(path);
        if (csv == null)
        {
            Debug.LogError($"CSV not found at Resources/{path}.csv");
            return new List<VideoSubtitleEntry>();
        }

        var lines = SplitLines(csv.text);
        var result = new List<VideoSubtitleEntry>();

        bool headerSkipped = false;
        
        foreach (var raw in lines)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;

            if (!headerSkipped)
            {
                headerSkipped = true;
                continue;
            }

            var cols = ParseCsvLine(raw);
            if (cols.Count < 4)
            {
                Debug.LogWarning($"Not enough columns: {raw}");
                continue;
            }

            string startStr = cols[0].Trim();
            string endStr = cols[1].Trim();
            string korText = cols[2];
            string engText = cols[3];

            double start = ParseTimecodeToSeconds(startStr, fps); 
            double end   = ParseTimecodeToSeconds(endStr, fps);
            if (end < start)
            {
                Debug.LogWarning($"End < Start: {raw}");
                continue;
            }

            result.Add(new VideoSubtitleEntry
            {
                Start = start,
                End = end,
                Duration = end - start,
                KorText = korText,
                EngText = engText
            });
        }

        result.Sort((a, b) => a.Start.CompareTo(b.Start));
        return result;
    }

    private static double ParseTimecodeToSeconds(string tc, double fps)
    {
        if (fps <= 0) fps = 30;

        fps = Math.Round(fps);

        var parts = tc.Replace(';', ':').Split(':');

        if (parts.Length != 4)
        {
            Debug.LogWarning($"Bad timecode: {tc} (expected hh:mm:ss:cc)");
            return 0;
        }

        int hh = SafeInt(parts[0]);
        int mm = SafeInt(parts[1]);
        int ss = SafeInt(parts[2]);
        int ff = SafeInt(parts[3]);

        if (ff < 0) ff = 0;

        return hh * 3600 + mm * 60 + ss + (ff / fps);
    }

    private static int SafeInt(string s)
    {
        int v;
        if (int.TryParse(s, out v)) return v;
        return 0;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var cols = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                if (c == ',')
                {
                    cols.Add(sb.ToString());
                    sb.Length = 0;
                }
                else if (c == '"')
                {
                    inQuotes = true;
                }
                else
                {
                    sb.Append(c);
                }
            }
        }

        cols.Add(sb.ToString());
        return cols;
    }

    private static List<string> SplitLines(string text)
    {
        var list = new List<string>();
        int i = 0, len = text.Length;
        var sb = new StringBuilder();

        while (i < len)
        {
            char c = text[i++];
            if (c == '\r')
            {
                if (i < len && text[i] == '\n') i++;
                list.Add(sb.ToString());
                sb.Length = 0;
            }
            else if (c == '\n')
            {
                list.Add(sb.ToString());
                sb.Length = 0;
            }
            else
            {
                sb.Append(c);
            }
        }
        if (sb.Length > 0) list.Add(sb.ToString());
        return list;
    }
}