using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[Serializable]
class DiarySub
{
    [SerializeField]
    public TMP_Text text;
    [SerializeField]
    public Image image;
}

public class DiaryPage : MonoBehaviour
{
    [SerializeField]
    TMP_Text title;
    [SerializeField]
    TMP_Text text;
    [SerializeField]
    TMP_Text sub_title;

    [SerializeField]
    DiarySub[]sub_diary;

    public void UpdateDiaryPage(string title, string text, string sub_title, DiarySubEntry[] subs)
    {
        this.title.text = title;
        this.text.text = text;
        this.sub_title.text = sub_title;

        for(int i=0; i<subs.Length; i++)
        {
            sub_diary[i].text.text = subs[i].text;
            sub_diary[i].image.sprite = subs[i].image;
        }

    }
}
