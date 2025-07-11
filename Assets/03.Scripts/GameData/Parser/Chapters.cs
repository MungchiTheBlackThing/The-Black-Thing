using System.Collections;
using System.Collections.Generic;
using System;


[System.Serializable]
public class ChapterInfo
{
    public int id;
    public string chapter;
    public string[] title;
    public string[] loadText;
    public string[] subTitle;
    public string[] longText;
    public string mainFilePath;
    public string[] subLockFilePath;
    public string[] subFilePath;
    public string[] reward;
}


[System.Serializable]
public class Chapters
{
    public ChapterInfo[] chapters;
}
