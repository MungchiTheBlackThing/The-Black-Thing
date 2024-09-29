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
