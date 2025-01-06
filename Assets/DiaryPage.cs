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
    DiarySub[]sub_diary;

    public void UpdateDiaryPage(List<string> title, List<string> text, RightPage subs, List<string> imagePath, int languageIdx, List<bool> isSuccess)
    {
        this.title.text = title[languageIdx];
        this.text.text = text[languageIdx];

        for(int i=0; i<sub_diary.Length; i++)
        {
            if (isSuccess[i])
            {
                sub_diary[i].text.text = subs.sub[i].success[languageIdx];
            }
            else
            {
                sub_diary[i].text.text = subs.sub[i].fail[languageIdx];
            }

            Sprite sprite = Resources.Load<Sprite>(imagePath[i]);
            if (sprite != null)
            {
                sub_diary[i].image.sprite = sprite; // Image에 Sprite 할당
            }
            else
            {
                Debug.LogError($"이미지 로드 실패: {imagePath[i]}");
            }
        }
    }
}
