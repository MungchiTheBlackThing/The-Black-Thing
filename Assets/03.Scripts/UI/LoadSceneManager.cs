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

    [Header("로딩챕터 패널 요소")]
    public TextMeshProUGUI chTitleText;
    public TextMeshProUGUI chLoadingText;
    public Slider loadingSlider;
    public TextMeshProUGUI loadingNumText;

    [Header("로딩 패널 로컬라이제이션")]
    [SerializeField]
    private string stringTableName = "UITexts";

    private string _targetSceneName;
    private int _targetChapter;

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
    
    public void LoadScene(string sceneName, int chapter = 0)
    {
        _targetSceneName = sceneName;
        _targetChapter = chapter;

        Debug.Log($"Scene Loading:{sceneName}, Chapter: {chapter}");

        InitLoadingState();
        Debug.Log($"Loading scene: {sceneName} with chapter: {chapter}");
        OpenLoadingUI(_targetChapter);
        Debug.Log($"OpenLoadingUI called with chapter: {_targetChapter}");
        StartCoroutine(LoadSceneCoroutine(_targetSceneName));
    }

    private void InitLoadingState()
    {
        loadingSlider.value = 0;
        loadingNumText.text = "0%";
    }

    private void OpenLoadingUI(int chapterNumber)
    {
        //챕터로딩 패널
        if (chapterNumber > 0)
        {
            chapterLoadingScreenPanel.SetActive(true);
            defaultLoadingScreenPanel.SetActive(false);

            StringTable stringTable = LocalizationSettings.StringDatabase.GetTable(stringTableName);
            if (stringTable != null)
            {
                string titleKey = $"ch{chapterNumber} title";
                if (chTitleText != null)
                    chTitleText.text = stringTable.GetEntry(titleKey)?.GetLocalizedString() ?? $"default title text";


                string loadingKey = $"ch{chapterNumber} loading";
                if (chLoadingText != null)
                    chLoadingText.text = stringTable.GetEntry(loadingKey)?.GetLocalizedString() ?? "default loading text";
            }
            else
            {
                Debug.LogError($"StringTable not found");
            }
        }
        //디폴트로딩 패널
        else
        {
            chapterLoadingScreenPanel.SetActive(false);
            defaultLoadingScreenPanel.SetActive(true);
        }
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return CheckLocalizationSettings();
        Debug.Log($"StartCoroutine 전");
        yield return StartCoroutine(LoadingOperation(sceneName));
        Debug.Log($"Scene {sceneName} loaded successfully.");
        CompleteLoading();
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

    private IEnumerator LoadingOperation(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (loadingSlider != null) loadingSlider.value = progress;
            if (loadingNumText != null) loadingNumText.text = $"{(progress * 100):F0}%";
            yield return null;
        }
        Debug.Log("[LoadingOperation] 씬 데이터 로드 완료 (isDone: true).");

        if (loadingSlider != null) loadingSlider.value = 1f;
        if (loadingNumText != null) loadingNumText.text = "100%";
        yield return new WaitForSeconds(0.2f);

        Debug.Log($"allowSceneActivation 전");
        operation.allowSceneActivation = true;
        Debug.Log($"allowSceneActivation 후");

        yield return new WaitForSeconds(5.0f);

        Debug.Log($"씬 로딩 끝");
    }

    private void CompleteLoading()
    {
        if (defaultLoadingScreenPanel != null) defaultLoadingScreenPanel.SetActive(false);
        if (chapterLoadingScreenPanel != null) chapterLoadingScreenPanel.SetActive(false);

        _targetSceneName = null;
        _targetChapter = 0;
    }
}
