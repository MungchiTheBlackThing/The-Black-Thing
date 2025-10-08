using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;
public class DragIcon : MonoBehaviour
{

    [SerializeField]
    TMP_Text titleText;
    [SerializeField]
    TMP_Text subText;
    [SerializeField]
    Image image;
    [SerializeField]
    GameObject lockObject;
    int chapter;
    string title;
    Sprite sprite;
    string subTitle;
    public GameObject RedAlert;
    string _stringTableName = "SystemUIText";

    public void Settings(int chapter, ChapterInfo info, LANGUAGE language)
    {
        StringTable stringtable = LocalizationSettings.StringDatabase.GetTable(_stringTableName);
        string titleKey = $"progress_title_ch{chapter}";
        string subKey = $"progress_subtitle_ch{chapter}";

        this.title = stringtable.GetEntry(titleKey).GetLocalizedString();
        this.subTitle = stringtable.GetEntry(subKey).GetLocalizedString();
        this.chapter = chapter;
        this.sprite = Resources.Load<Sprite>(info.mainFilePath);

        //this.subTitle = info.subTitle[(int)language];
        //this.title = info.title[(int)language];

        titleText.text = this.title;
        subText.text = this.subTitle;
        image.sprite = this.sprite;
    }

    public bool isLocking()
    {
        return lockObject.active;
    }
    public void DestoryLock()
    {
        if(lockObject.active)
            lockObject.SetActive(false);
    }
}
