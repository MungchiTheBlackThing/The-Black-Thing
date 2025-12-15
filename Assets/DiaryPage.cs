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
    PlayerController playerController;

    [SerializeField]
    TMP_Text title;
    [SerializeField]
    TMP_Text text;

    [SerializeField]
    DiarySub[]sub_diary;

    public void UpdateDiaryPage(List<string> title, List<string> text, RightPage subs, List<string> imagePath, int languageIdx, List<bool> isSuccess)
    {
        this.title.text = GetTokenString(title[languageIdx]);
        this.text.text = GetTokenString(text[languageIdx]);

        for(int i=0; i<sub_diary.Length; i++)
        {
            string target = "";
            if (isSuccess[i])
            {
                target = subs.sub[i].success[languageIdx];
            }
            else
            {
                target = subs.sub[i].fail[languageIdx];
            }

            sub_diary[i].text.text = GetTokenString(target);

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

    /// <summary>
    /// <> 토큰을 데이터로 변환
    /// </summary>
    string GetTokenString(string target)
    {
        target = target.Replace("<nickname>", playerController.GetNickName());

        return target;
    }
}
