using System;
using System.Collections.Generic;

using System;
using System.Collections.Generic;
using UnityEngine;
using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using UnityEngine;

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
    public List<string> leftPage;             // 텍스트 리스트
    public DiarySubEntry rightPage;   // DiarySubEntry 객체
    public List<string> imagePath;        // 이미지 경로 리스트
}

[Serializable]
public class DiarySubEntry
{
    public Sub1 sub1; // sub1 객체
    public Sub1 sub2; // sub2 객체
    public Sub1 sub3; // sub3 객체
    public Sub1 sub4; // sub4 객체
}

[Serializable]
public class Sub1
{
    public string[] success; // 성공 텍스트 그룹 (0: 한글, 1: 영어)
    public string[] fail;    // 실패 텍스트 그룹 (0: 한글, 1: 영어)
}
