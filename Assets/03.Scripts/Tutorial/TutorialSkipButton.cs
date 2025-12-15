using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSkipButton : MonoBehaviour
{
    public void OnClickSkipButton()
    {
        if (!RecentManager.Exists())
        {
            RecentManager.Load();
        }
        RecentManager.tutoSceneEnd();
        LoadSceneManager.Instance.LoadScene("TutorialScene", "MainScene", 0);
        
    }
}
