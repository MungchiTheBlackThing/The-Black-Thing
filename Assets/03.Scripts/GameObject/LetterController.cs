using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    GameObject canvas;

    void Start()
    {
        canvas = GameObject.Find("Canvas");
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
        if(alert.activeSelf)
        {
            alert.SetActive(false);
            if (noteUI != null)
            {
                noteUI.SetActive(true);
                AudioManager.instance.PlayOneShot(FMODEvents.instance.note, this.transform.position);
            }
            else
            {
                noteUI = Instantiate(note, canvas.transform);
            }
        }
    }

    public void CloseWatching()
    {
        alert.SetActive(false);
    }
}
