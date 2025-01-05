using System;
using System.Collections.Generic;

[System.Serializable]
public class DiaryEntry
{
    public int id; // Diary Entry의 ID
    public List<string> title; // 제목 배열
    public List<string> Text; // 텍스트 배열
    public DiarySubEntry DiarySubEntry; // 성공/실패 서브 엔트리
    public List<string> imagePath; // 이미지 경로 배열
}

[System.Serializable]
public class DiarySubEntry
{
    public SubEntry success; // 성공 서브 엔트리
    public SubEntry fail; // 실패 서브 엔트리
}

[System.Serializable]
public class SubEntry
{
    public List<string> text; // 텍스트 배열
}

[System.Serializable]
public class DiaryData
{
    public List<DiaryEntry> DiaryEntry; // 전체 Diary Entry 리스트
}
