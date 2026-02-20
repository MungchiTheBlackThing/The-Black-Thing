using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization;

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
    private GameManager gameManager;

    private Coroutine _autoCloseCo;


    [SerializeField]
    GameObject checkList;
    [SerializeField]
    GameObject subcheck;
    [SerializeField]
    GameObject skipBut;
    [SerializeField]
    checklist[] checklists;

    TranslateManager translator;

    [SerializeField]
    TMP_Text[] phase;
    [SerializeField]
    ObjectManager objectManager;

    GameObject activeIcon;
    public GameObject iconCheckListNoteObj;
    public TMP_Text dial2Text;

    // Start is called before the first frame update
    void Start()
    {
        pc = GameObject.FindWithTag("Player").gameObject.GetComponent<PlayerController>();
        pc.nextPhaseDelegate -= NextPhase;
        pc.nextPhaseDelegate += NextPhase;

        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();
        translator.translatorDel += Translate;

        objectManager.activeSystemUIDelegate += CallbackActiveSystemUI;
        
        gameManager = FindObjectOfType<GameManager>();
        
        InitPhase((GamePatternState)pc.GetCurrentPhase());
    }
    public void CallbackActiveSystemUI(bool InActive)
    {
        if (GameManager.isend)
        {
            gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(InActive);
    }
    void Translate(LANGUAGE language, TMP_FontAsset font)
    {

        int Idx = (int)language;

        phase[0].text = DataManager.Instance.Settings.checklist.phase1[Idx];
        phase[1].text = DataManager.Instance.Settings.checklist.phase2[Idx];
        phase[2].text = DataManager.Instance.Settings.checklist.phase3[Idx];
        phase[3].text = DataManager.Instance.Settings.checklist.phase4[Idx];


        //for(int i=0;i<phase.Length;i++)
        //{
        //    phase[i].font = font;
        //}
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

    private Coroutine changeStateCo;

    public void NextPhase(GamePatternState state)
    {
        if (changeStateCo != null) StopCoroutine(changeStateCo);
        changeStateCo = StartCoroutine(ChangeState(state));
    }


    IEnumerator ChangeState(GamePatternState state)
    {
        yield return new WaitForSeconds(2.5f);
        if (GameManager.isend) yield break;

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
            _autoCloseCo = StartCoroutine(AutoCloseChecklist());
        }

    }

    IEnumerator AutoCloseChecklist()
    {
        yield return new WaitForSeconds(4f);
        if (checkList.activeSelf)
            checkList.SetActive(false);
    }

    public void OnClickCheckListIcon()
    {
        if (GameManager.isend)
        {
            gameObject.SetActive(false);
            return;
        }
        if (checkList.activeSelf == false)
        {
            // 자동닫기 코루틴 취소
            if (_autoCloseCo != null)
            {
                StopCoroutine(_autoCloseCo);
                _autoCloseCo = null;
            }

            int ph = pc.GetCurrentPhase();
            checkList.SetActive(true);
            if (ph == 0 || ph == 2 || ph == 4 || ph == 6)
            {
                subcheck.SetActive(true);
            }
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.checklistOn, this.transform.position);
        }
        else
            checkList.SetActive(false);
    }
    
    private bool wasIconCheckListNoteObjActive = false;
    
    void Update()
    {
        if (iconCheckListNoteObj != null)
        {
            bool isActive = iconCheckListNoteObj.activeSelf;
            wasIconCheckListNoteObjActive = isActive;
        }
    }




    private void OnDisable()
    {
        checkList.SetActive(false);
    }
}
