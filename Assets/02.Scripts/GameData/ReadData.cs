using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadData : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //������ �ε� 
        var loadedJson = Resources.Load<TextAsset>("Json/Chapters");

        if (loadedJson)
        {
            DataManager.Instance.ChapterList = JsonUtility.FromJson<Chapters>(loadedJson.ToString());
        }

        loadedJson = Resources.Load<TextAsset>("Json/Setting");

        if(loadedJson)
        {
            DataManager.Instance.Settings = JsonUtility.FromJson<LanguageInfo>(loadedJson.ToString());
        }
    }

}
