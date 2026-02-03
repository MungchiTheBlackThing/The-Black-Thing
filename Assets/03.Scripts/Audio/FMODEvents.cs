using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : Singleton<FMODEvents>
{
    [field: Header("Diary")]
    [field: SerializeField] public EventReference Pagesound { get; private set; }
    [field: SerializeField] public EventReference DiaryButton { get; private set; }

    [field: Header("Menu")]
    [field: SerializeField] public EventReference MenuOn { get; private set; }
    [field: SerializeField] public EventReference checklistOn { get; private set; }
    [field: SerializeField] public EventReference checklistSet { get; private set; }
    [field: SerializeField] public EventReference moonnote { get; private set; }
    [field: SerializeField] public EventReference buttonClick { get; private set; }
    
    [field: SerializeField] public EventReference iconClick { get; private set; }
    [field: SerializeField] public EventReference alert_p { get; private set; }
    [field: SerializeField] public EventReference alert_n { get; private set; }
    [field: SerializeField] public EventReference lockClick { get; private set; }

    [field: Header("Object")]
    [field: SerializeField] public EventReference mold { get; private set; }
    [field: SerializeField] public EventReference hourglass { get; private set; }
    [field: SerializeField] public EventReference dreamcatcher { get; private set; }
    [field: SerializeField] public EventReference door { get; private set; }

    [field: SerializeField] public EventReference note { get; private set; }
    [field: SerializeField] public EventReference binocular { get; private set; }
    [field: SerializeField] public EventReference diary { get; private set; }
    [field: SerializeField] public EventReference mapradio { get; private set; }

    [field: Header("Dialogue")]
    [field: SerializeField] public EventReference mainEnter { get; private set; }
    [field: SerializeField] public EventReference dialougueDefault { get; private set; }
    [field: SerializeField] public EventReference dialougeSelect { get; private set; }
    [field: SerializeField] public EventReference dialouguecheckbox { get; private set; }

    [field: Header("Moonradio")]
    [field: SerializeField] public EventReference earthsend { get; private set; }
    [field: SerializeField] public EventReference moonbuttonoff { get; private set; }
    [field: SerializeField] public EventReference moonbuttonclick { get; private set; }
    [field: SerializeField] public EventReference moonbuttonlock { get; private set; }
    [field: SerializeField] public EventReference earthsent { get; private set; }

    [field: Header("Moonchat")]
    [field: SerializeField] public EventReference red { get; private set; }
    [field: SerializeField] public EventReference edison { get; private set; }
    [field: SerializeField] public EventReference cello { get; private set; }

    [field: Header("BinoCular")]
    [field: SerializeField] public EventReference[] binosuccess { get; private set; }
    [field: SerializeField] public EventReference[] binofails { get; private set; }

    [field: Header("BGM")]
    [field: SerializeField] public EventReference bgmmain_1 { get; private set; }
    [field: SerializeField] public EventReference bgmmain_2 { get; private set; }
    [field: SerializeField] public EventReference bgmmain_3 { get; private set; }
    [field: SerializeField] public EventReference bgmmain_3_ver2 { get; private set; }
    [field: SerializeField] public EventReference bgmmain_4 { get; private set; }
    [field: SerializeField] public EventReference bgmmain_death { get; private set; }
    [field: SerializeField] public EventReference bgm_intro { get; private set; }
    [field: SerializeField] public EventReference bgm_moonradio { get; private set; }

    // --- �߰�: AMB ---
    [field: Header("AMB")]
    [field: SerializeField] public EventReference ambRoom { get; private set; }   // event:/AMB/AMB_Room (�Ķ���� ��ȯ��)
}
