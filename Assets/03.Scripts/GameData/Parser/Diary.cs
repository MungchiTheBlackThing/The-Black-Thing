using System;
using System.Collections.Generic;

[Serializable]
public class Diary
{
    public List<DiaryEntry> DiaryEntry; // 여러 DiaryEntry를 포함하는 리스트
}

[Serializable]
public class DiaryEntry
{
    public int id;                        // DiaryEntry의 ID
    public string titleKey;               // 제목 키
    public string leftPageKey;            // 텍스트 키
    public RightPage rightPage;           // DiarySubEntry 객체
    public List<string> imagePath;        // 이미지 경로 리스트
}

[System.Serializable]
public class RightPage
{
    public List<SubEntry> sub;
}


[Serializable]
public class SubEntry
{
    public string successKey; // 성공 텍스트 키
    public string failKey;    // 실패 텍스트 키
}
