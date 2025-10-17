using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class LetterController : BaseObject, IWatchingInterface
{
    [SerializeField]
    EWatching type;

    [SerializeField]
    GameObject alert;

    [SerializeField]
    GameObject noteUI;

    [SerializeField]
    GameObject note;

    [SerializeField]
    TextMeshProUGUI noteText;

    GameObject canvas;
    PlayerController playerController;

    bool createdNoteUI = false;
    int currentChapter = -1;
    static readonly int[] LetterChapters = { 3, 4, 6, 7, 9, 11, 12, 14 };
    private string _stringTableName = "WatchingNoteUI";

    void Start()
    {
        canvas = GameObject.Find("Canvas");

        playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        currentChapter = playerController.GetChapter();
        Debug.Log($"ApplyLocalizedNoteForChapter start: ch={currentChapter}");
    }

    public bool IsCurrentPattern(EWatching curPattern)
    {
        return curPattern == type;
    }

    public void OpenWatching(int Chapter)
    {
        alert.SetActive(true);
    }

    private void OnMouseDown()
    {
        if (alert.activeSelf)
            alert.SetActive(false);

        if (noteUI != null)
        {
            noteUI.SetActive(true);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.note, this.transform.position);
        }
        else
        {
            noteUI = Instantiate(note, canvas.transform);
            createdNoteUI = true;
        }
        
        noteText = noteUI.GetComponentInChildren<TextMeshProUGUI>(true);

        if (noteText == null)
            Debug.LogError("noteText is null");

        ApplyLocalizedNoteForChapter();
    }

    public void CloseWatching()
    {
        alert.SetActive(false);

        // if (noteUI != null)
        // {
        //     if (createdNoteUI)
        //     {
        //         Destroy(noteUI);  
        //     }
        //     else
        //     {
        //         noteUI.SetActive(false); 
        //     }
        //     noteUI = null;
        //     createdNoteUI = false;
        // }
    }

    void ApplyLocalizedNoteForChapter()
    {
        Debug.Log("실행됨");
        if (System.Array.IndexOf(LetterChapters, currentChapter) < 0) return;
        if (noteUI == null) return;

        string key = $"Note_Ch{currentChapter}";
        StringTable stringTable = LocalizationSettings.StringDatabase.GetTable(_stringTableName);

        if (stringTable != null)
        {
            var entry = stringTable.GetEntry(key);
            if (entry != null)
            {
                noteText.text = entry.GetLocalizedString();
            }
            else
            {
                Debug.LogWarning($"Key '{key}' not found in String Table '{_stringTableName}'.");
            }
        }
        else
        {
            Debug.LogError($"String Table 없음");
        }
    }
}
