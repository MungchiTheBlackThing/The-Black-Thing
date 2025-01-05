using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

public class ReadData : MonoBehaviour
{
    // Start is called before the first frame update

    private void Awake()
    {
        StartCoroutine(LoadResourceCoroutine());
        DataManager.Instance.MoonRadioParser = new MoonRadioParser();
        DataManager.Instance.MoonRadioParser.LoadMoonRadio();
    }

    private IEnumerator LoadResourceCoroutine()
    {
        // 리소스를 비동기적으로 로드
        ResourceRequest resourceRequest = Resources.LoadAsync<TextAsset>("Json/Chapters");

        // 로딩이 완료될 때까지 대기
        yield return resourceRequest;

        // 로딩이 완료되면 Asset에 접근 가능
        if (resourceRequest.asset != null)
        {
            TextAsset loadedTextAsset = resourceRequest.asset as TextAsset;
            DataManager.Instance.ChapterList = JsonUtility.FromJson<Chapters>(loadedTextAsset.ToString());
        }

        // 리소스를 비동기적으로 로드
        resourceRequest = Resources.LoadAsync<TextAsset>("Json/Setting");

        // 로딩이 완료될 때까지 대기
        yield return resourceRequest;

        // 로딩이 완료되면 Asset에 접근 가능
        if (resourceRequest.asset != null)
        {
            TextAsset loadedTextAsset = resourceRequest.asset as TextAsset;
            DataManager.Instance.Settings = JsonUtility.FromJson<SettingInfo>(loadedTextAsset.ToString());
        }
        
        resourceRequest = Resources.LoadAsync<TextAsset>("Json/PoemsData");

        // 로딩이 완료될 때까지 대기
        yield return resourceRequest;

        // 로딩이 완료되면 Asset에 접근 가능
        if (resourceRequest.asset != null)
        {
            TextAsset loadedTextAsset = resourceRequest.asset as TextAsset;
            DataManager.Instance.PoemData = JsonUtility.FromJson<Poems>(loadedTextAsset.ToString());
        }

        resourceRequest = Resources.LoadAsync<TextAsset>("Json/DiaryData");

        // 로딩이 완료될 때까지 대기
        yield return resourceRequest;

        // 로딩이 완료되면 Asset에 접근 가능
        if (resourceRequest.asset != null)
        {
            TextAsset loadedTextAsset = resourceRequest.asset as TextAsset;
            DataManager.Instance.DiaryData = JsonUtility.FromJson<Diary>(loadedTextAsset.ToString());
        }
    }
}
