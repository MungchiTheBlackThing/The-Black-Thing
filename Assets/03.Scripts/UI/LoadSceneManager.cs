using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

public class LoadSceneManager : MonoBehaviour
{
    public static LoadSceneManager Instance { get; private set; }

    [Header("로딩 패널")]
    public GameObject defaultLoadingScreenPanel;
    public GameObject chapterLoadingScreenPanel;
    public GameObject fadeInOutImg;

    [Header("로딩챕터 패널 요소")]
    public TextMeshProUGUI chTitleText;
    public TextMeshProUGUI chLoadingText;
    public Slider loadingSlider;
    public TextMeshProUGUI loadingNumText;

    public FadeInOutManager fadeInOut;

    [Header("로딩 패널 로컬라이제이션 테이블")]
    [SerializeField]
    private string stringTableName = "UITexts";

    private string _currentSceneName;
    private string _targetSceneName;
    private int _targetChapter;
    private float _visualProgress = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 챕터 번호가 0이면 디폴트 로딩 패널을 사용하고, 그 외의 경우 챕터 로딩 패널을 사용하여 씬 로드
    /// </summary>
    public void LoadScene(string currentSceneName, string targetSceneName, int chapter = 0)
    {
        _currentSceneName = currentSceneName;
        _targetSceneName = targetSceneName;
        _targetChapter = chapter;

        InitLoadingState();
        StartCoroutine(OpenLoadingUI());
        StartCoroutine(LoadSceneCoroutine());
    }

    private void InitLoadingState()
    {
        loadingSlider.value = 0;
        loadingNumText.text = "0%";
        fadeInOutImg.SetActive(true);
    }

    private IEnumerator OpenLoadingUI()
    {
        fadeInOut.StartFadeOut();
        yield return new WaitForSeconds(fadeInOut.GetFadeTime() + 0.2f);

        if (_targetChapter > 0) //챕터로딩 패널
        {
            chapterLoadingScreenPanel.SetActive(true);
            defaultLoadingScreenPanel.SetActive(false);

            StringTable stringTable = LocalizationSettings.StringDatabase.GetTable(stringTableName);
            if (stringTable != null)
            {
                LocalizationSettings.SelectedLocaleChanged += locale =>
                {
                    string titleKey = $"ch{_targetChapter} title";
                    if (chTitleText != null)
                        chTitleText.text = stringTable.GetEntry(titleKey)?.GetLocalizedString() ?? $"default title text";

                    string loadingKey = $"ch{_targetChapter} loading";
                    if (chLoadingText != null)
                        chLoadingText.text = stringTable.GetEntry(loadingKey)?.GetLocalizedString() ?? "default loading text";
                };
            }
            else
            {
                Debug.LogError($"StringTable not found");
            }
        }
        else //디폴트로딩 패널
        {
            chapterLoadingScreenPanel.SetActive(false);
            defaultLoadingScreenPanel.SetActive(true);
        }

        fadeInOut.StartFadeIn();

    }

    private IEnumerator LoadSceneCoroutine()
    {
        yield return CheckLocalizationSettings();
        yield return StartCoroutine(LoadingOperation());
    }

    private IEnumerator CheckLocalizationSettings()
    {
        if (LocalizationSettings.Instance == null)
        {
            Debug.LogError("LocalizationSettings instance not found");
            yield break;
        }
        yield return LocalizationSettings.InitializationOperation;
    }

    private IEnumerator LoadingOperation()
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(_targetSceneName, LoadSceneMode.Additive);
        loadOperation.allowSceneActivation = false;

        _visualProgress = 0f;

        yield return StartCoroutine(UpdateLoadingProgress(loadOperation));
        yield return StartCoroutine(UpdateFakeProgress(_visualProgress));
        yield return StartCoroutine(SwitchScenes(loadOperation));
        yield return StartCoroutine(PlayFadeInOut());

    }

    private IEnumerator UpdateLoadingProgress(AsyncOperation loadOperation)
    {
        float realProgress = 0f;

        while (_visualProgress < 0.9f)
        {
            realProgress = Mathf.Clamp01(loadOperation.progress / 0.9f);
            _visualProgress += Time.deltaTime * 0.3f;
            if (_visualProgress > realProgress) _visualProgress = realProgress;

            if (loadingSlider != null) loadingSlider.value = _visualProgress;
            if (loadingNumText != null) loadingNumText.text = $"{(_visualProgress * 100):F0}%";

            yield return null;
        }
    }

    private IEnumerator UpdateFakeProgress(float startProgress)
    {
        float fakeDuration = 2.0f;
        float elapsed = 0f;

        while (elapsed < fakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fakeDuration);
            float fakeProgress = Mathf.Lerp(startProgress, 1f, t);

            if (loadingSlider != null) loadingSlider.value = fakeProgress;
            if (loadingNumText != null) loadingNumText.text = $"{(fakeProgress * 100):F0}%";

            yield return null;
        }

        if (loadingSlider != null) loadingSlider.value = 1f;
        if (loadingNumText != null) loadingNumText.text = "100%";

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator SwitchScenes(AsyncOperation loadOperation)
    {
        loadOperation.allowSceneActivation = true;
        yield return new WaitUntil(() => loadOperation.isDone);

        AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(_currentSceneName);
        yield return unloadOperation;

        yield return new WaitForSeconds(5.0f);
    }

    private IEnumerator PlayFadeInOut()
    {
        fadeInOut.StartFadeOut();
        yield return new WaitForSeconds(fadeInOut.GetFadeTime() + 0.2f);

        fadeInOut.StartFadeIn();
        CompleteLoading();
    }

    private void CompleteLoading()
    {
        defaultLoadingScreenPanel.SetActive(false);
        chapterLoadingScreenPanel.SetActive(false);
        fadeInOutImg.SetActive(false);

        _targetSceneName = null;
        _targetChapter = 0;


    }

    /// <summary>
    /// MainScene에서 챕터 변경시 이미지 로드 (씬 전환 X)
    /// </summary>
    public void LoadChapterImage(int chapter) 
    {
        if (chapter < 1 || chapter > 14)
        {
            Debug.LogError("Invalid chapter number");
            return;
        }
        if (Instance == null)
        {
            Debug.LogError("LoadSceneManager instance not found");
            return;
        }

        _targetChapter = chapter;
        fadeInOutImg.SetActive(true);
        StartCoroutine(LoadChapter());
    }

    private IEnumerator LoadChapter()
    {
        StartCoroutine(OpenChapterUI());
        yield return new WaitForSeconds(2.0f);
        Utility.Instance.WaitForFirstTouch(() =>
        {
            StartCoroutine(CloseChapterUI());
        });
        yield return null;
    }

    private IEnumerator OpenChapterUI()
    {
        fadeInOut.StartFadeOut();
        yield return new WaitForSeconds(fadeInOut.GetFadeTime() + 0.2f);

        chapterLoadingScreenPanel.SetActive(true);
        defaultLoadingScreenPanel.SetActive(false);

        StringTable stringTable = LocalizationSettings.StringDatabase.GetTable(stringTableName);
        if (stringTable != null)
        {
            LocalizationSettings.SelectedLocaleChanged += locale =>
            {
                string titleKey = $"ch{_targetChapter} title";
                if (chTitleText != null)
                    chTitleText.text = stringTable.GetEntry(titleKey)?.GetLocalizedString() ?? $"default title text";

                string loadingKey = $"ch{_targetChapter} loading";
                if (chLoadingText != null)
                    chLoadingText.text = stringTable.GetEntry(loadingKey)?.GetLocalizedString() ?? "default loading text";
            };
        }
        else
        {
            Debug.LogError($"StringTable not found");
        }

        fadeInOut.StartFadeIn();
        yield return new WaitForSeconds(fadeInOut.GetFadeTime() + 0.2f);
    }

    private IEnumerator CloseChapterUI()
    {
        fadeInOut.StartFadeOut();
        yield return new WaitForSeconds(fadeInOut.GetFadeTime() + 0.2f);

        chapterLoadingScreenPanel.SetActive(false);
        defaultLoadingScreenPanel.SetActive(true);
        fadeInOutImg.SetActive(false);
        _targetChapter = 0;

        fadeInOut.StartFadeIn();
        yield return new WaitForSeconds(fadeInOut.GetFadeTime() + 0.2f);

    }
}
