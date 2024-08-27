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

    LanguageInfo settings;

    public LanguageInfo Settings
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
