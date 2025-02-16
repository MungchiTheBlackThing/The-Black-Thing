using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alertmanager : MonoBehaviour
{
    public GameObject Dragicon;
    public GameObject SpiderAlert;
    public GameObject ProgressAlert;
    public GameObject ChapterAlert;
    public List<GameObject> RewardAlerts = new List<GameObject>();
    [SerializeField] public bool isAlert = false;
    // Start is called before the first frame update
   
    public void Alerton()
    {
        SpiderAlert.SetActive(true);
        ProgressAlert.SetActive(true);
    }
    public void openChapter()
    {
        ChapterAlert.SetActive(true);
    }
    public void Alertoff()
    {
        SpiderAlert.SetActive(false);
        ProgressAlert.SetActive(false);
        if (isAlert)
            ChapterAlert.SetActive(false);
        foreach(GameObject alert in RewardAlerts)
        {
            alert.SetActive(false);
        }
        isAlert = false;
    }
}
