using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public enum EChecklist
{
    Icon,
    Note
}

[System.Serializable]
public struct checklist
{
    [SerializeField]
    public GamePatternState patternState;

    [SerializeField]
    public EChecklist eChecklist;

    [SerializeField]
    public GameObject IconObject;

    [SerializeField]
    public List<GameObject> noteObjects;
}
public class ChecklistController : MonoBehaviour
{
    private PlayerController pc;

    [SerializeField]
    GameObject checkList;
    [SerializeField]
    GameObject skipBut;
    [SerializeField]
    checklist[] checklists;

    TranslateManager translator;

    [SerializeField]
    TMP_Text[] phase;

    GameObject activeIcon;
    // Start is called before the first frame update
    void Start()
    {
        pc = GameObject.FindWithTag("Player").gameObject.GetComponent<PlayerController>();
        pc.nextPhaseDelegate += NextPhase;

        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();
        translator.translatorDel += Translate;

        InitPhase((GamePatternState)pc.GetAlreadyEndedPhase());
    }

    void Translate(LANGUAGE language, TMP_FontAsset font)
    {
        //�����Ѵ�.
        Debug.Log("Checklist �����մϴ�.\n");

        int Idx = (int)language;

        phase[0].text = DataManager.Instance.Settings.checklist.phase1[Idx];
        phase[1].text = DataManager.Instance.Settings.checklist.phase2[Idx];
        phase[2].text = DataManager.Instance.Settings.checklist.phase3[Idx];
        phase[3].text = DataManager.Instance.Settings.checklist.phase4[Idx];


        for(int i=0;i<phase.Length;i++)
        {
            phase[i].font = font;
        }
    }
    
    private void InitPhase(GamePatternState state)
    {
        int Last = (int)GamePatternState.NextChapter;

        foreach (var Object in checklists[Last].noteObjects)
        {
            Object.SetActive(false);
        }

        int Idx = (int)state;
        activeIcon = checklists[Idx].IconObject;
        activeIcon.SetActive(true);

        foreach (var note in checklists[Idx].noteObjects)
        {
            if (note.activeSelf == false)
            {
                note.SetActive(true);
            }
        }
    }

    public void NextPhase(GamePatternState state)
    {
        StartCoroutine(ChangeState(state));
    }

    //�ڷ�ƾ���� �Ѵ�.
    IEnumerator ChangeState(GamePatternState state)
    {
        yield return new WaitForSeconds(5.5f);

        int Idx = (int)state;

        if (state == GamePatternState.Watching)
        {
            int Last = (int)GamePatternState.NextChapter;

            foreach (var Object in checklists[Last].noteObjects)
            {
                Object.SetActive(false);
            }
        }

        if (activeIcon)
        {
            activeIcon.SetActive(false);
        }

        activeIcon = checklists[Idx].IconObject;
        activeIcon.SetActive(true);


        foreach (var note in checklists[Idx].noteObjects)
        {
            if (note.activeSelf == false)
            {
                note.SetActive(true);
            }
        }

        if (checklists[Idx].eChecklist == EChecklist.Note)
        {
            OnClickCheckListIcon();
        }

        yield return null;
    }

    public void OnClickCheckListIcon()
    {
        if (checkList.activeSelf == false)
        {
            checkList.SetActive(true);
            StartCoroutine(CloseAlter(checkList));
        }
        else
            checkList.SetActive(false);
    }

    IEnumerator CloseAlter(GameObject checkList)
    {
        yield return new WaitForSeconds(2f);
        checkList.SetActive(false);
    }

    private void OnDisable()
    {
        checkList.SetActive(false);
    }
}
