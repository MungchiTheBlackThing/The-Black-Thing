using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public void PassData(ChapterInfo chapterInfo, PlayerController player)
    {
        this.title.text=chapterInfo.title[(int)player.getLanguage()];
        this.sentence.text=chapterInfo.loadText[(int)player.getLanguage()];
        this.player=player;

        //켜질 때 현재 chapter값보다 작으면
        if(chapterInfo.id<this.player.GetChapter())
        {
            for(int i=0;i<phaseEdUI.Count;i++) 
            {
                phaseEdUI[i].SetActive(true);
            }
            for(int i=0;i<phaseIngUI.Count;i++) 
            {
                phaseIngUI[i].SetActive(true);
            }
        }
        else
        {
            //Player Phase 단계에 따라서 진행.
            for(int i=0;i<=this.player.GetAlreadyEndedPhase();i++)
            {
                if(phaseIngUI.Count<=i) continue;
                phaseIngUI[i].SetActive(true);
            }
            for(int i=0;i<this.player.GetAlreadyEndedPhase();i++)
            {
                if(phaseEdUI.Count<=i) continue;
                phaseEdUI[i].SetActive(true);
            }
        }

        for(int i=0;i<subPhaseUI.Count;i++)
        {
            if(subPhaseUIObject[i].activeSelf == false)
            {
                subPhaseUI[i].sprite=Resources.Load<Sprite>(chapterInfo.subFilePath[i]);
            }
            else
            {
                subPhaseUI[i].sprite=Resources.Load<Sprite>(chapterInfo.subLockFilePath[i]);
            }
        }
    }

    private void OnDisable() {
        
        for(int i=0;i<phaseEdUI.Count;i++) 
        {
            phaseEdUI[i].SetActive(false);
        }
        for(int i=0;i<phaseIngUI.Count;i++) 
        {
            phaseIngUI[i].SetActive(false);
        }
    }
}
