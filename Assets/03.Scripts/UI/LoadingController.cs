using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingController : MonoBehaviour
{
    [SerializeField]
    TMP_Text title;
    [SerializeField]
    TMP_Text dailytips;

    [SerializeField]
    PlayerController pc;
    int currentChapter;
    // Start is called before the first frame update
    void Start()
    {
        if(pc)
        { 
            currentChapter = pc.GetChapter();
            int lang = (int)pc.GetLanguage();
            title.text = DataManager.Instance.ChapterList.chapters[currentChapter].title[lang];
            dailytips.text = DataManager.Instance.ChapterList.chapters[currentChapter].loadText[lang];
        }
    }

}
