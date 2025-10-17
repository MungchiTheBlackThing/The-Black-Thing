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
            Debug.LogWarning("SubDialAlert: player 또는 chapterInfo가 비어 있음");
            return;
        }

        int phase = player.GetAlreadyEndedPhase();
        List<int> subseq = player.GetWatchedList();

        if (secondText != null)
        {
            int lastValue = (subseq.Count > 0) ? subseq[subseq.Count - 1] : 0;

            int remain = Mathf.Max(0, GetPhaseLength(phase) - lastValue);
            secondText.text = secondText.text.Replace("<int>", remain.ToString());
        }


        // 성공 여부
        List<bool> successPhase = player.GetSubPhase(chapterInfo.id);

        // 아이콘 적용
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
            Debug.Log("성공 페이즈: " + successPhase[allowindex]);
            // 슬롯 세팅
            if (successPhase[allowindex])
            {
                Debug.Log("성공 이미지 " + i + " " + chapterInfo.subLockFilePath[allowindex]);
                images[i].sprite = Resources.Load<Sprite>(chapterInfo.subFilePath[allowindex]);
            }
            else
            {
                Debug.Log("실패 이미지 " + i + " " + chapterInfo.subLockFilePath[allowindex]);
                images[i].sprite = Resources.Load<Sprite>(chapterInfo.subLockFilePath[allowindex]);
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
            case 2: return 2;
            case 4: return 3;
            case 6: return 4;
            default: return 0;
        }
    }
}
