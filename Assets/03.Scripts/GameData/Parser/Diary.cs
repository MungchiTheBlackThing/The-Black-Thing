using System;
using System.Collections.Generic;

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
    public List<string> leftPage;         // �ؽ�Ʈ ����Ʈ
    public RightPage rightPage;           // DiarySubEntry ��ü
    public List<string> imagePath;        // �̹��� ��� ����Ʈ
}

[System.Serializable]
public class RightPage
{
    public List<SubEntry> sub;
}


[Serializable]
public class SubEntry
{
    public string[] success; // ���� �ؽ�Ʈ �׷� (0: �ѱ�, 1: ����)
    public string[] fail;    // ���� �ؽ�Ʈ �׷� (0: �ѱ�, 1: ����)
}
