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
        // 먼저 전부 초기화함
        text.text = ""; // 왼쪽 페이지 직접 초기화
        title.text = ""; // 타이틀도
        for (int i = 0; i < sub_diary.Length; i++)
        {
            SetLocalization(sub_diary[i].textLocalize, null);
            sub_diary[i].image.sprite = null;
            sub_diary[i].image.enabled = false;
        }
        
        // 2. 타이틀 + 왼쪽 페이지 세팅
        SetLocalization(titleLocalize, titleKey);
        SetLocalization(textLocalize, leftPageKey);

        for (int i = 0; i < sub_diary.Length; i++)
        {
            if (i >= imagePath.Count) {
                Debug.LogWarning($"[DiaryPage] Skipping sub-diary {i} due to index out of bounds (image/success).");
                continue;
            }
            if (subs == null || subs.sub == null || i >= subs.sub.Count) {
                Debug.LogWarning($"[DiaryPage] Skipping sub-diary {i} because subs data is null or index is out of bounds.");
                continue;
            }

            bool success = (isSuccess != null && i < isSuccess.Count) && isSuccess[i];
            string targetKey = success ? subs.sub[i].successKey : subs.sub[i].failKey;

            SetLocalization(sub_diary[i].textLocalize, targetKey);

            Sprite sprite = Resources.Load<Sprite>(imagePath[i]);
            if (sprite != null)
            {
                sub_diary[i].image.sprite = sprite;
                sub_diary[i].image.enabled = true;
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

            var tmp = localizeEvent.GetComponent<TMP_Text>();
            if (tmp != null) tmp.text = "";
        
            return;
        }

        localizeEvent.StringReference.SetReference(DIARY_TABLE, key);
        var nick = playerController != null ? playerController.GetNickName() : "";
        var args = new Dictionary<string, object> { { "nickname", nick } };
        localizeEvent.StringReference.Arguments = new object[] { args };

        localizeEvent.RefreshString();
        Debug.Log($"[DiaryPage] SetLocalization on '{localizeEvent.gameObject.name}' with Table: '{DIARY_TABLE}', Key: '{key}'.");
    }
}
