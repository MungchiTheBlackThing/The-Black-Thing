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

    [SerializeField] private GameManager gameManager;

    private int lastvalue = 0;
    void OnEnable()
    {
        foreach (Image image in images)
        {
            image.gameObject.SetActive(true);
        }
        int findChapter = player.GetChapter();
        ChapterInfo chapterInfo = DataManager.Instance.ChapterList.chapters[findChapter];
        Debug.Log("챕터 정보: " + chapterInfo);
        if (player == null || chapterInfo == null)
        {
            Debug.LogWarning("SubDialAlert: player 또는 chapterInfo가 비어 있음");
            return;
        }

        int phase = player.GetCurrentPhase();
        List<int> subseq = player.GetWatchedList();

        if (secondText != null)
        {
            lastvalue = (subseq.Count > 0) ? subseq[subseq.Count - 1] : 0;

            int remain = Mathf.Max(0, GetPhaseLength(phase) - lastvalue);
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
            if (successPhase[allowindex] && allowindex <= lastvalue - 1)
            {
                Debug.Log("성공 이미지 " + i + " " + chapterInfo.subFilePath[allowindex]);
                images[i].sprite = Resources.Load<Sprite>(chapterInfo.subFilePath[allowindex]);
            }
            else
            {
                Debug.Log("실패 이미지 " + i + " " + chapterInfo.subLockFilePath[allowindex]);
                images[i].sprite = Resources.Load<Sprite>(chapterInfo.subLockFilePath[allowindex]);
            }

        }
    }

    public void subtimeskip()
    {
        gameManager.SkipSubDialWaitAndShowNow();
    }


    private List<int> GetAllowedIndices(int phase)
    {
        switch (phase)
        {
            case 2: return new List<int> { 0, 1 };
            case 4: return new List<int> { 2 };
            case 6: return new List<int> { 3 };
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
