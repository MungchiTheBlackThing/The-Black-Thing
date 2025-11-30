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
    public List<string> title;            // 제목 리스트
    public List<string> leftPage;         // 텍스트 리스트
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
    public string[] success; // 성공 텍스트 그룹 (0: 한글, 1: 영어)
    public string[] fail;    // 실패 텍스트 그룹 (0: 한글, 1: 영어)
}
