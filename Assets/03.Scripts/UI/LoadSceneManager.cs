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
    public Image loadingSliderFill;
    public TextMeshProUGUI loadingNumText;

    public FadeInOutManager fadeInOut;

    [Header("로딩 패널 로컬라이제이션 테이블")]
    private string _stringTableName = "ChapterLoadingUIText";

    private string _currentSceneName;
    private string _targetSceneName;
    private int _targetChapter;

    private float _visualProgress = 0f;
    private float _realProgress = 0f;

    private bool _isLoadChapterImage = false;

    public event System.Action OnLoadingUIShown;

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
   

    // IntroScene -> 튜토/메인씬, 챕터 번호가 0이면 디폴트 로딩 패널을 사용하고, 그 외의 경우 챕터 로딩 패널을 사용하여 씬 로드
    public void LoadScene(string currentSceneName, string targetSceneName, int chapter = 0)
    {
        _currentSceneName = currentSceneName;
        _targetSceneName = targetSceneName;
        _targetChapter = chapter;
        _isLoadChapterImage = false;

        InitLoadingState();

        StartCoroutine(OpenLoadingUI()); //UI 열기
        StartCoroutine(LoadSceneCoroutine()); //로딩 진행
    }

    // MainScene에서 챕터 변경시 이미지 로드
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
        _isLoadChapterImage = true;
        InitLoadingState();
        StartCoroutine(LoadChapter());
    }

    public IEnumerator ShowDefaultOverlayOnce(float seconds = 2.0f)
    {
        fadeInOutImg.SetActive(true);

        yield return FadeOutAndWait(0.2f);

        defaultLoadingScreenPanel.SetActive(true);
        chapterLoadingScreenPanel.SetActive(false);

        yield return FadeInAndWait(seconds);

        fadeInOutImg.SetActive(false);
    }

    public void HideAllLoadingOverlays()
    {
        defaultLoadingScreenPanel.SetActive(false);
        chapterLoadingScreenPanel.SetActive(false);
        fadeInOutImg.SetActive(false);
    }

    private void InitLoadingState()
    {
        if (loadingSlider != null)
        {
            loadingSlider.value = 0;
            if (loadingSliderFill != null)
                loadingSliderFill.fillAmount = 0f;
        }

        if (loadingNumText != null)
            loadingNumText.text = "0%";

        fadeInOutImg.SetActive(true);
    }

    private IEnumerator OpenLoadingUI()
    {
        yield return FadeOutAndWait(0.2f);

        if (_targetChapter > 0) //챕터로딩 패널
        {
            OpenChapterPanel();
        }
        else //디폴트로딩 패널
        {
            OpenDefaultPanel();
        }

        yield return FadeInAndWait(0.2f);
        OnLoadingUIShown?.Invoke();
    }

    private void OpenChapterPanel()
    {
        chapterLoadingScreenPanel.SetActive(true);
        defaultLoadingScreenPanel.SetActive(false);

        StringTable stringTable = LocalizationSettings.StringDatabase.GetTable(_stringTableName);
        Debug.Log($"[LoadSceneManager]변경되어야 하는 챕터: {_targetChapter}");
        if (stringTable != null)
        {
            string titleKey = $"loading_title_ch{_targetChapter}";
            if (chTitleText != null)
                chTitleText.text = stringTable.GetEntry(titleKey)?.GetLocalizedString() ?? "default title text";

            string loadingKey = $"loading_contents_ch{_targetChapter}";
            if (chLoadingText != null)
                chLoadingText.text = stringTable.GetEntry(loadingKey)?.GetLocalizedString() ?? "default loading text";
        }
        else
        {
            Debug.LogError("StringTable not found");
        }
    }

    private void OpenDefaultPanel()
    {
        chapterLoadingScreenPanel.SetActive(false);
        defaultLoadingScreenPanel.SetActive(true);
    }

    private IEnumerator LoadSceneCoroutine()
    {
        yield return CheckLocalizationSettings();
        if (_isLoadChapterImage)
        {
            yield return LoadChapterImageAsync();
            _isLoadChapterImage = false;
        }
        else
        {
            yield return StartCoroutine(LoadingOperation());
        }
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

    private IEnumerator LoadChapterImageAsync()
    {
        _visualProgress = 0f;
        _realProgress = 0f;

        float fakeLoadTime = 1.5f;
        float elapsedTime = 0f;
        while (elapsedTime < fakeLoadTime)
        {
            elapsedTime += Time.deltaTime;
            _visualProgress = Mathf.Clamp01(elapsedTime / fakeLoadTime) * 0.9f;
            UpdateLoadingUI(_visualProgress);
            yield return null;
        }
        _visualProgress = 0.9f;
        UpdateLoadingUI(_visualProgress);

        yield return UpdateProgress(_visualProgress);
    }

    private IEnumerator LoadingOperation()
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(_targetSceneName, LoadSceneMode.Single);
        loadOperation.allowSceneActivation = false;
        
        _visualProgress = 0f;
        _realProgress = 0f;

        // allowSceneActivation = false 상태에서는 progress가 0.9까지만 진행되므로
        yield return UpdateLoadingProgress(loadOperation);

        // 나머지는 페이크로 자연스럽게 채워준다
        yield return UpdateProgress(_visualProgress);

        loadOperation.allowSceneActivation = true;

        yield return new WaitUntil(() => loadOperation.isDone);
        yield return new WaitForSeconds(3.0f); // 씬 진입 후 오브젝트 초기화 대기 시간 추가

        if (fadeInOut != null)
        {
            fadeInOut.StartFadeOut();
            yield return new WaitForSeconds(fadeInOut.GetFadeTime() + 0.3f);
            fadeInOut.StartFadeIn();
        }

        CompleteLoading();
    }

    private IEnumerator UpdateLoadingProgress(AsyncOperation loadOperation)
    { 
        while (_visualProgress < 0.9f)
        {
            _realProgress = Mathf.Clamp01(loadOperation.progress / 0.9f);
            _visualProgress += Time.deltaTime * 0.3f;

            if (_visualProgress > _realProgress) 
                _visualProgress = _realProgress;

            UpdateLoadingUI(_visualProgress);
            yield return null;
        }
    }

    private IEnumerator UpdateProgress(float startProgress)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _visualProgress = Mathf.Lerp(startProgress, 1f, elapsed / duration);

            UpdateLoadingUI(_visualProgress);
            yield return null;
        }

        UpdateLoadingUI(1f);
        yield return new WaitForSeconds(0.5f);
    }
    
    private void CompleteLoading()
    {
        defaultLoadingScreenPanel.SetActive(false);
        chapterLoadingScreenPanel.SetActive(false);
        fadeInOutImg.SetActive(false);

        _targetSceneName = null;
        _targetChapter = 0;
    }

    
    private IEnumerator LoadChapter()
    {
        yield return StartCoroutine(OpenLoadingUI());

        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(LoadSceneCoroutine());
 
        Utility.Instance.WaitForFirstTouch(() =>
        {
            StartCoroutine(CloseChapterUI());
        });
    }


    private IEnumerator CloseChapterUI()
    {
        yield return FadeOutAndWait(0.2f);

        CompleteLoading();

        yield return FadeInAndWait(0.2f);
    }

    #region Helpers

    private IEnumerator FadeOutAndWait(float extraDelay)
    {
        if (fadeInOut == null)
            yield break;

        fadeInOut.StartFadeOut();
        yield return new WaitForSeconds(fadeInOut.GetFadeTime() + extraDelay);
    }

    private IEnumerator FadeInAndWait(float extraDelay)
    {
        if (fadeInOut == null)
            yield break;

        fadeInOut.StartFadeIn();
        yield return new WaitForSeconds(fadeInOut.GetFadeTime() + extraDelay);
    }

    private void UpdateLoadingUI(float progress)
    {
        if (loadingSlider != null && loadingSliderFill != null)
        {
            // loadingSlider.value는 주석 처리되어 있었으므로 그대로 둠
            loadingSliderFill.fillAmount = progress;
        }

        if (loadingNumText != null)
            loadingNumText.text = $"{(progress * 100):F0}%";
    }

    #endregion
}
