using UnityEngine;

public class FORQA_SKIP : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] MainPanel mainDial;
    [SerializeField] SubDialogue subDial;

    CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
    }

    void Update()
    {
        bool show = player.GetChapter() >= 2 &&
                    ((mainDial != null && mainDial.isActiveAndEnabled ) ||
                     (subDial != null && subDial.currentDialogueList.Count > 0));

        cg.alpha = show ? 1f : 0f;
        cg.interactable = show;
        cg.blocksRaycasts = show;
    }
}
