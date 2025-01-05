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
    public List<DiaryEntry> DiaryEntry; // ���� DiaryEntry�� �����ϴ� ����Ʈ
}

[Serializable]
public class DiaryEntry
{
    public int id;                        // DiaryEntry�� ID
    public List<string> title;            // ���� ����Ʈ
    public List<string> leftPage;             // �ؽ�Ʈ ����Ʈ
    public DiarySubEntry rightPage;   // DiarySubEntry ��ü
    public List<string> imagePath;        // �̹��� ��� ����Ʈ
}

[Serializable]
public class DiarySubEntry
{
    public Sub1 sub1; // sub1 ��ü
    public Sub1 sub2; // sub2 ��ü
    public Sub1 sub3; // sub3 ��ü
    public Sub1 sub4; // sub4 ��ü
}

[Serializable]
public class Sub1
{
    public string[] success; // ���� �ؽ�Ʈ �׷� (0: �ѱ�, 1: ����)
    public string[] fail;    // ���� �ؽ�Ʈ �׷� (0: �ѱ�, 1: ����)
}
