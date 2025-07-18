using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    public static LoadingController Instance { get; private set; }

    [SerializeField]
    private TextMeshProUGUI textChapter;
    private TextMeshProUGUI textDailyTips;

    private AsyncOperation operation;
    private string targetSceneName;
    private int currentChapter;

    public GameObject defaultLoadingScreen;
    public GameObject chapterLoadingScreen;

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

    public static void LoadScene(string sceneName, int chapter = 0)
    {
        if (Instance == null)
        {
            Debug.LogError("LoadingController instance is not initialized.");
            return;
        }

        Instance.targetSceneName = sceneName;
        Instance.currentChapter = chapter;
        Debug.Log($"Scene Loading: {sceneName}");
        SceneManager.LoadScene("LoadingScene");

        //StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    void OnEnable()
    {
        if (!string.IsNullOrEmpty(targetSceneName) && SceneManager.GetActiveScene().name == "LoadingScene")
        {
            if (currentChapter > 0)
            {
                chapterLoadingScreen.SetActive(true);
                defaultLoadingScreen.SetActive(false);
                //textChapter.text = $"Chapter {currentChapter}";
            }
            else
            {
                chapterLoadingScreen.SetActive(false);
                defaultLoadingScreen.SetActive(true);
            }
            StartCoroutine(LoadSceneCoroutine(targetSceneName));
        }
    }

    IEnumerator LoadSceneCoroutine(string sceneName)
    {
        operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {

            yield return null;
        }
    }
}
