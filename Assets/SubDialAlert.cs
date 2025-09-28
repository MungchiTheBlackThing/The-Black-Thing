using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class SubDialAlert : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    [SerializeField] private TextMeshProUGUI secondText;

    [SerializeField] private List<Image> images;

    void OnEnable()
    {
        foreach (Image image in images)
        {
            image.gameObject.SetActive(true);
        }
        int findChapter = player.GetChapter();
        ChapterInfo chapterInfo = DataManager.Instance.ChapterList.chapters[findChapter];

        if (player == null || chapterInfo == null)
        {
            Debug.LogWarning("SubDialAlert: player �Ǵ� chapterInfo�� ��� ����");
            return;
        }

        int phase = player.GetAlreadyEndedPhase();
        int subseq = player.GetSubseq();

        // <int> ġȯ
        if (secondText != null)
        {
            int remain = Mathf.Max(0, GetPhaseLength(phase) - subseq);
            secondText.text = secondText.text.Replace("<int>", remain.ToString());
        }

        // ���� ����
        List<bool> successPhase = player.GetSubPhase(chapterInfo.id);

        // ������ ����
        ApplySubPhaseIcons(chapterInfo, successPhase, phase);
    }


    private void ApplySubPhaseIcons(ChapterInfo chapterInfo, List<bool> successPhase, int phase)
    {
        List<int> allowed = GetAllowedIndices(phase); // 1-based
        Debug.Log("allow: "+allowed);
        Debug.Log(allowed.Count);
        for (int i =0; i<allowed.Count; i++)
        {
            int allowindex = allowed[i];
            Debug.Log("���� ������: " + successPhase[allowindex]);
            // ���� ����
            if (successPhase[allowindex])
            {
                Debug.Log("���� �̹��� " + i + " " + chapterInfo.subLockFilePath[i]);
                images[i].sprite = Resources.Load<Sprite>(chapterInfo.subFilePath[i]);
            }
            else
            {
                Debug.Log("���� �̹��� " + i + " " + chapterInfo.subLockFilePath[i]);
                images[i].sprite = Resources.Load<Sprite>(chapterInfo.subLockFilePath[i]);
            }

        }
    }


    private List<int> GetAllowedIndices(int phase)
    {
        switch (phase)
        {
            case 2: return new List<int> { 1, 2 };
            case 4: return new List<int> { 3 };
            case 6: return new List<int> { 4 };
            default: return new List<int>();
        }
    }

    private int GetPhaseLength(int phase)
    {
        switch (phase)
        {
            case 2: return 3;
            case 4: return 4;
            case 6: return 5;
            default: return 0;
        }
    }
}
