using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class MainVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    [SerializeField] public GameObject Rawimage;

    private bool waitingToPlay = false;

    [SerializeField] GameObject text;
    [SerializeField] int chapter;
    [SerializeField] GameObject background;

    // subtitle fields
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

    private bool isVideoPlaying = false;
    private bool skipArmed = false;
    private Coroutine skipHintFadeCo;
    public event System.Action OnUserSkipRequested;

    private void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.skipOnDrop = true;
            videoPlayer.isLooping = false;
        }
        Rawimage.SetActive(false);
        if (text != null)
            subtitleText = text.GetComponentInChildren<TMP_Text>(true);

        HideSkipHintImmediate();
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

        // load subtitles for this chapter
        entries = MainVideoCsvLoader.Load(chapter);
        lastIndex = -1;
        startsCache = null;
        EnsureSubtitleText();
        if (subtitleText != null) subtitleText.text = "";

        subtitleTableName = $"SrcDay{Day}";
        entryKeys = (entries != null) ? BuildEntryKeys(Day, entries.Count) : null;
        RebuildLocalizedCache();
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged_Subtitle;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged_Subtitle;

        Rawimage.SetActive(false);

        string path = $"StoryAnimation/AnimDay{Day}";
        VideoClip clip = Resources.Load<VideoClip>(path);

        if (clip == null)
        {
            Debug.LogError($"VideoClip not found at Resources/{path}");
            return;
        }

        videoPlayer.Stop();
        videoPlayer.time = 0;
        videoPlayer.frame = 0;
        videoPlayer.playOnAwake = false;
        videoPlayer.playbackSpeed = 1f;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetDirectAudioMute(0, true);
        videoPlayer.SetDirectAudioVolume(0, 1.0f);

        videoPlayer.clip = clip;
        videoPlayer.gameObject.SetActive(true);
        videoPlayer.Prepare();

        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.prepareCompleted += OnPrepared;

        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.loopPointReached += OnVideoEnd;

        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.errorReceived += OnVideoError;

        videoPlayer.seekCompleted -= OnSeekCompleted;
        videoPlayer.seekCompleted += OnSeekCompleted;
    }

    public void PlayVideo()
    {
        Debug.Log("Video start");
        StartCoroutine(FadeInAndPlay());
    }

    private IEnumerator FadeInAndPlay()
    {
        CanvasGroup bgCg = background.GetComponent<CanvasGroup>() ?? background.AddComponent<CanvasGroup>();
        bgCg.alpha = 0f;
        background.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(bgCg, 0f, 1f, 1f));

        Rawimage.SetActive(true);
        text.SetActive(true);
        HideSkipHintImmediate();

        yield return null; // or: yield return new WaitForEndOfFrame();
        //Rawimage.transform.SetAsLastSibling();//

        if (videoPlayer.isPrepared)
        {
            if (AudioManager.instance != null) AudioManager.instance.StopBGM();
            else Debug.LogWarning("[MainVideo] AudioManager.instance is null");
            videoPlayer.SetDirectAudioMute(0, false);
            isVideoPlaying = true;
            videoPlayer.Play();
        }
        else
        {
            waitingToPlay = true;
            StartCoroutine(WaitForVideoToPrepare());
        }
    }

    private IEnumerator WaitForVideoToPrepare()
    {
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        if (waitingToPlay)
        {
            waitingToPlay = false;
            if (AudioManager.instance != null) AudioManager.instance.StopBGM();
            else Debug.LogWarning("[MainVideo] AudioManager.instance is null");
            videoPlayer.SetDirectAudioMute(0, false);
            isVideoPlaying = true;
            videoPlayer.Play();
        }
    }

    private void OnPrepared(VideoPlayer vp)
    {
        Debug.Log("Video prepared.");
        lastIndex = -1;
        if (subtitleText != null) subtitleText.text = "";
    }

    private void OnSeekCompleted(VideoPlayer vp)
    {
        lastIndex = -1;
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        isVideoPlaying = false;
        HideSkipHintImmediate();
        StartCoroutine(FadeOutAndEnd(vp));
    }

    private IEnumerator FadeOutAndEnd(VideoPlayer vp)
    {
        vp.Stop();
        vp.time = 0;
        text.SetActive(false);

        Rawimage.SetActive(false);

        yield return null;
        //Rawimage.transform.SetAsFirstSibling();//

        var bgCg = background.GetComponent<CanvasGroup>() ?? background.AddComponent<CanvasGroup>();
        yield return StartCoroutine(FadeCanvasGroup(bgCg, 1f, 0f, 1f));
        background.SetActive(false);

        if (AudioManager.instance != null) AudioManager.instance.UpdateBGMByChapter(chapter);
        else Debug.LogWarning("[MainVideo] AudioManager.instance is null");
    }

    private void OnDisable()
    {
        isVideoPlaying = false;
        HideSkipHintImmediate();
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;
        }
        if (subtitleText != null) subtitleText.text = "";
        text.SetActive(false);
        EndGameNow();
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged_Subtitle;
    }

    private void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError("VideoPlayer Error: " + message);
        Rawimage.SetActive(false);
        if (subtitleText != null) subtitleText.text = "";
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
        // subtitle sync
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
        //스킵 입력처리
        if (isVideoPlaying)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!skipArmed)
                {
                    ShowSkipHint();
                }
                else
                {
                    isVideoPlaying = false;
                    HideSkipHintImmediate();
                    OnUserSkipRequested?.Invoke();
                }
            }
        }
    }

    private int FindIndexAtTime(double t)
    {
        // quick check around lastIndex
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

        // binary search on starts
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

    public void EndGameNow()
    {
        ResetVideoToStart();
    }

    private void ResetVideoToStart()
    {
        StopAllCoroutines();
        waitingToPlay = false;

        // 비디오 정리
        if (videoPlayer != null)
        {
            try
            {
                videoPlayer.prepareCompleted -= OnPrepared;
                videoPlayer.loopPointReached -= OnVideoEnd;
                videoPlayer.errorReceived -= OnVideoError;
                videoPlayer.seekCompleted -= OnSeekCompleted;

                videoPlayer.Stop();
                videoPlayer.time = 0;
                videoPlayer.frame = 0;
                videoPlayer.SetDirectAudioMute(0, true);

                if (videoPlayer.clip != null)
                {
                    videoPlayer.Prepare();
                }
            }
            catch { }
        }

        if (text != null) text.SetActive(false);

        if (Rawimage != null)
        {
            //Rawimage.transform.SetAsFirstSibling();
            Rawimage.SetActive(false);
        }

        if (background != null)
        {
            var cg = background.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0f;
            background.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        EndGameNow();
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

}



/*--------------------------------------------CSV 파싱 부분----------------------------------------------------*/

[System.Serializable]
public class VideoSubtitleEntry
{
    public double Start;     // seconds
    public double End;       // seconds
    public double Duration;  // End - Start
    public string KorText;
    public string EngText;
}

public static class MainVideoCsvLoader
{
    public static List<VideoSubtitleEntry> Load(int day)
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
                continue; // skip header
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

            double start = ParseTimecodeToSeconds(startStr); 
            double end = ParseTimecodeToSeconds(endStr);
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

    private static double ParseTimecodeToSeconds(string tc)
    {
        var parts = tc.Split(':');
        if (parts.Length != 4)
        {
            Debug.LogWarning($"Bad timecode: {tc} (expected hh:mm:ss:cc)");
            return 0;
        }

        int hh = SafeInt(parts[0]);
        int mm = SafeInt(parts[1]);
        int ss = SafeInt(parts[2]);
        int cc = SafeInt(parts[3]);

        if (cc < 0) cc = 0;
        if (cc > 99) cc = 99;

        double seconds = hh * 3600 + mm * 60 + ss + (cc * 0.01);
        return seconds;
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
