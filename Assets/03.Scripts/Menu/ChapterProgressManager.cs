using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class ChapterProgressManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> phaseIngUI;
    [SerializeField]
    List<GameObject> phaseEdUI;
    [SerializeField]
    TMP_Text title;
    [SerializeField]
    TMP_Text sentence;

    PlayerController player;
    
    [SerializeField]
    List<Image> subPhaseUI; 
    [SerializeField]
    List<GameObject> subPhaseUIObject;
    [SerializeField]
    Alertmanager alertmanager;
    
    private string _stringTableName = "SystemUIText";

    public void PassData(ChapterInfo chapterInfo, PlayerController player)
    {
        StringTable stringTable = LocalizationSettings.StringDatabase.GetTable(_stringTableName);
        var titleKey = $"progress_title_ch{chapterInfo.id}";
        var sentenceKey = $"progress_longtext_ch{chapterInfo.id}";
        this.title.text = stringTable.GetEntry(titleKey).GetLocalizedString();
        this.sentence.text = stringTable.GetEntry(sentenceKey).GetLocalizedString();

        //this.title.text = chapterInfo.title[(int)player.GetLanguage()];
        //this.sentence.text = chapterInfo.longText[(int)player.GetLanguage()];

        this.player = player;

        List<bool> successPhase = this.player.GetSubPhase(chapterInfo.id);

        //phase Ing/Ed UI 처리 
        //켜질 때 현재 chapter값보다 작으면(이전 챕터) 모든 UI 켜짐
        if (chapterInfo.id < this.player.GetChapter())
        {
            for (int i = 0; i < phaseEdUI.Count; i++)
            {
                phaseEdUI[i].SetActive(true);
            }
            for (int i = 0; i < phaseIngUI.Count; i++)
            {
                phaseIngUI[i].SetActive(true);
            }
        }
        else
        {
            //Player Phase 단계에 따라서 진행.
            int uiPhaseIndex = this.player.GetAlreadyEndedPhase() / 2;
            Debug.Log($"uiPhaseIndex: {uiPhaseIndex}, Player Phase: {this.player.GetAlreadyEndedPhase()}");

            if (uiPhaseIndex < 0 || uiPhaseIndex >= phaseIngUI.Count)
            {
                Debug.LogError($"잘못된 uiPhaseIndex");
                uiPhaseIndex = -1;
            }

            for (int i = 0; i < phaseIngUI.Count; i++)
            {
                phaseIngUI[i].SetActive(i <= uiPhaseIndex);
            }
            for (int i = 0; i < phaseEdUI.Count; i++)
            {
                phaseEdUI[i].SetActive(i < uiPhaseIndex);
            }
        }

        //subPhaseUIObject 처리
        for (int i = 0; i < successPhase.Count; i++)
        {
            Debug.Log($"서브페이즈 {i}: {successPhase[i]}");
            if (successPhase[i])
            {
                subPhaseUIObject[i].SetActive(false);
                subPhaseUI[i].sprite = Resources.Load<Sprite>(chapterInfo.subFilePath[i]);
            }
            else
            {
                subPhaseUIObject[i].SetActive(true);
                subPhaseUI[i].sprite = Resources.Load<Sprite>(chapterInfo.subLockFilePath[i]);
            }
        }
        Invoke(nameof(alertoff), 2f);
    }

    private void alertoff()
    {
        if (alertmanager != null)
            alertmanager.Alertoff();
    }

    private void OnDisable() 
    {
        for(int i=0;i<phaseEdUI.Count;i++) 
        {
            phaseEdUI[i].SetActive(false);
        }
        for(int i=0;i<phaseIngUI.Count;i++) 
        {
            phaseIngUI[i].SetActive(false);
        }
        if (alertmanager != null)
            alertmanager.Alertoff();
    }
}
