using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/*�̱������� ���� ����*/
public class DataManager : MonoBehaviour
{
    static DataManager instance;
    #region Json�� ���� ���� 
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

    Death deathdata;
    public Death DeathData
    {
        get
        {
            return deathdata;
        }
        set
        {
            deathdata = value;
        }
    }

    DotReview dotReview;
    public DotReview DotReview
    {
        get
        {
            return dotReview;
        }
        set
        {
            dotReview = value;
        }
    }

    UIText uIText;
    public UIText UIText
    {
        get { return uIText; }
        set { uIText = value; }
    }

    Watchinginfo watchinginfo;

    public Watchinginfo Watchinginfo
    {
        get { return watchinginfo; }
        set { watchinginfo = value; }
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
            //���� ������?
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
