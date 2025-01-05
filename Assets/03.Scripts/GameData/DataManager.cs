using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/*½Ì±ÛÅæÀ¸·Î ±¸Çö ¿¹Á¤*/
public class DataManager : MonoBehaviour
{
    static DataManager instance;
    #region Json¿¡ ´ëÇÑ º¯¼ö 
    Chapters chapterList;

    public Chapters ChapterList
    {
        get
        {
            return chapterList;
        }
        set
        {
            chapterList = value;
        }
    }

    SettingInfo settings;

    public SettingInfo Settings
    {
        get
        {
            return settings;
        }
        set
        {
            settings = value;
        }
    }

    Poems poemData;

    public Poems PoemData
    {
        get
        {
            return poemData;
        }
        set
        {
            poemData = value;
        }
    }

    MoonRadioParser moonRadioParser;
    public MoonRadioParser MoonRadioParser
    {
        get 
        { 
            return moonRadioParser; 
        }
        set
        {
            moonRadioParser = value;
        }
    }

    Diary diaryData;
    public Diary DiaryData
    {
        get
        {
            return diaryData;
        }
        set
        {
            diaryData = value;
        }
    }

    #endregion
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            //¾ÀÀÌ ²¨Áö³ª?
        }
    }

    public static DataManager Instance
    {
        get
        {
            if(instance == null)
            {
                return null;
            }
            return instance; 
        }
    }
}
