using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Localization.Components;
using UnityEngine.Localization;

[Serializable]
class DiarySub
{
    [SerializeField]
    public TMP_Text text;
    [SerializeField]
    public LocalizeStringEvent textLocalize;
    [SerializeField]
    public Image image;
}

public class DiaryPage : MonoBehaviour
{
    [SerializeField]
    PlayerController playerController;

    [SerializeField]
    TMP_Text title;
    [SerializeField]
    private LocalizeStringEvent titleLocalize;
    [SerializeField]
    TMP_Text text;
    [SerializeField]
    private LocalizeStringEvent textLocalize;

    [SerializeField]
    DiarySub[] sub_diary;

    private const string DIARY_TABLE = "DiaryText";

    public void UpdateDiaryPage(string titleKey, string leftPageKey, RightPage subs, List<string> imagePath, List<bool> isSuccess)
    {
        Debug.Log($"[DiaryPage] UpdateDiaryPage received. titleKey: {titleKey}, leftPageKey: {leftPageKey}");
        SetLocalization(titleLocalize, titleKey);
        SetLocalization(textLocalize, leftPageKey);

        for (int i = 0; i < sub_diary.Length; i++)
        {
            if (i >= imagePath.Count || i >= isSuccess.Count) {
                Debug.LogWarning($"[DiaryPage] Skipping sub-diary {i} due to index out of bounds (image/success).");
                continue;
            }
            if (subs == null || subs.sub == null || i >= subs.sub.Count) {
                Debug.LogWarning($"[DiaryPage] Skipping sub-diary {i} because subs data is null or index is out of bounds.");
                continue;
            }

            string targetKey = isSuccess[i] ? subs.sub[i].successKey : subs.sub[i].failKey;
            Debug.Log($"[DiaryPage] Sub-diary {i}: isSuccess={isSuccess[i]}, targetKey={targetKey}");
            SetLocalization(sub_diary[i].textLocalize, targetKey);

            Sprite sprite = Resources.Load<Sprite>(imagePath[i]);
            if (sprite != null)
            {
                sub_diary[i].image.sprite = sprite;
            }
            else
            {
                Debug.LogError($"이미지 로드 실패: {imagePath[i]}");
            }
        }
    }

    private void SetLocalization(LocalizeStringEvent localizeEvent, string key)
    {
        if (localizeEvent == null) {
            Debug.LogError($"[DiaryPage] SetLocalization failed: LocalizeStringEvent is null. Key was '{key}'. Check Inspector assignments.");
            return;
        }
        if (string.IsNullOrEmpty(key)) {
            Debug.LogWarning($"[DiaryPage] SetLocalization: Key is null or empty for {localizeEvent.gameObject.name}. Clearing text.");
            // 키가 비어있을 경우 텍스트를 비우는 처리 추가
            localizeEvent.StringReference.SetReference(null, null);
            localizeEvent.RefreshString();
            return;
        }

        localizeEvent.StringReference.SetReference(DIARY_TABLE, key);
        var args = new Dictionary<string, object> { { "nickname", playerController.GetNickName() } };
        localizeEvent.StringReference.Arguments = new object[] { args };

        localizeEvent.RefreshString();
        Debug.Log($"[DiaryPage] SetLocalization on '{localizeEvent.gameObject.name}' with Table: '{DIARY_TABLE}', Key: '{key}'.");
    }
}
