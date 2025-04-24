using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{

    [field:Header ("Diary")]
    [field:SerializeField] public EventReference Pagesound { get; private set; }
    [field: SerializeField] public EventReference DiaryButton { get; private set; }

[field: Header("Menu")]
    [field: SerializeField] public EventReference MenuOn { get; private set; }
    [field: SerializeField] public EventReference checklistOn { get; private set; }
    [field: SerializeField] public EventReference checklistSet { get; private set; }
    [field: SerializeField] public EventReference moonnote  { get; private set; }
    [field: SerializeField] public EventReference buttonClick  { get; private set; }
    [field: SerializeField] public EventReference lockClick { get; private set; }

    [field: Header("Object")]
    [field: SerializeField] public EventReference mold { get; private set; }
    [field: SerializeField] public EventReference hourglass { get; private set; }
    [field: SerializeField] public EventReference dreamcatcher { get; private set; }
    [field: SerializeField] public EventReference door { get; private set; }
    [field: SerializeField] public EventReference note { get; private set; }
    [field: SerializeField] public EventReference binocular { get; private set; }

    [field:Header("Dialogue")]
    [field: SerializeField] public EventReference mainEnter { get; private set; }
    [field: SerializeField] public EventReference dialougueDefault { get; private set; }
    [field: SerializeField] public EventReference dialougeSelect { get; private set; }
    [field: SerializeField] public EventReference dialouguecheckbox { get; private set; }

    [field: Header("Moonradio")]
    [field: SerializeField] public EventReference earthsend { get; private set; }
    [field: SerializeField] public EventReference moonbuttonoff { get; private set; }
    [field: SerializeField] public EventReference moonbuttonclick { get; private set; }
    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            Debug.LogError("No FModEvents");
        }
        instance = this;
    }
}
