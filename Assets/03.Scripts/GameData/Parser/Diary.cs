using System;
using System.Collections.Generic;

[System.Serializable]
public class DiaryEntry
{
    public int id; // Diary Entry�� ID
    public List<string> title; // ���� �迭
    public List<string> Text; // �ؽ�Ʈ �迭
    public DiarySubEntry DiarySubEntry; // ����/���� ���� ��Ʈ��
    public List<string> imagePath; // �̹��� ��� �迭
}

[System.Serializable]
public class DiarySubEntry
{
    public SubEntry success; // ���� ���� ��Ʈ��
    public SubEntry fail; // ���� ���� ��Ʈ��
}

[System.Serializable]
public class SubEntry
{
    public List<string> text; // �ؽ�Ʈ �迭
}

[System.Serializable]
public class DiaryData
{
    public List<DiaryEntry> DiaryEntry; // ��ü Diary Entry ����Ʈ
}
