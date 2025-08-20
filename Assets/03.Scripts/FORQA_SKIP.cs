using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FORQA_SKIP : MonoBehaviour
{
    [SerializeField]
    PlayerController player;

    [SerializeField]
    MainDialogue MainDial;

    [SerializeField]
    SubDialogue SubDial;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if (player.GetChapter() >= 2 && (MainDial.currentDialogueList.Count > 0 || SubDial.currentDialogueList.Count > 0))
        {
            this.gameObject.SetActive(true);
        } 
    }
}
