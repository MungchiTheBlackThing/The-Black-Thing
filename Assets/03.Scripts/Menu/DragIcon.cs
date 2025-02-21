using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    public void Settings(int chapter, ChapterInfo info, LANGUAGE language)
    {
        this.chapter=chapter;
        this.title= info.title[(int)language];
        this.sprite = Resources.Load<Sprite>(info.mainFilePath);
        this.subTitle= info.subTitle[(int)language]; //Update ¿¹Á¤

        titleText.text=this.title;
        subText.text=this.subTitle;
        image.sprite=this.sprite;
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
