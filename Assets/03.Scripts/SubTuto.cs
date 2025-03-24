using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class RecentItem
{
    public GameObject obj;
    public int value;
    public int index;
}

public class SubTuto : MonoBehaviour
{
    [SerializeField] SubPanel subPanel;
    [SerializeField] TouchGuide touch;
    [SerializeField] GameObject nickname;
    [SerializeField] TutorialManager tutorialManager;
    [SerializeField] CameraZoom cameraZoom;
    [SerializeField]
    public GameObject UIBalloon;
    [SerializeField]
    public Moonnote moonnote;
    [SerializeField]
    GameObject SystemUI;
    [SerializeField]
    PlayerController playerController;
    [SerializeField]
    DotController dotController;

    public string prefabPath = "TouchGuide";

    Vector3 guide1 = new Vector3(-810, -145, 0);
    Vector3 guide2 = new Vector3(-1095, -195, 0);
    Vector3 guide3 = new Vector3(-1100, -400, 0);

    [SerializeField]
    public List<RecentItem> Recents = new List<RecentItem>();

    public void tutorial_2(GameObject selectedDot, int determine, int index)
    {
        GameObject touchguide = Resources.Load<GameObject>(prefabPath);
        if (touchguide != null)
        {
            GameObject instance = Instantiate(touchguide, subPanel.gameObject.transform);
            instance.transform.localPosition = guide1;
            instance.SetActive(true);
            touch = instance.GetComponent<TouchGuide>();
        }
        else
        {
            Debug.LogError("프리팹을 찾을 수 없습니다!");
        }
        touch.tuto2(selectedDot, determine);
    }

    public void tutorial_3(GameObject selectedDot, int determine, int index)
    {
        GameObject touchguide = Resources.Load<GameObject>(prefabPath);
        if (touchguide != null)
        {
            GameObject instance = Instantiate(touchguide, subPanel.gameObject.transform);
            instance.transform.localPosition = guide2;
            instance.SetActive(true);
            touch = instance.GetComponent<TouchGuide>();
        }
        else
        {
            Debug.LogError("프리팹을 찾을 수 없습니다!");
        }
        touch.tuto3(selectedDot, determine);
    }
    public void tutorial_4(GameObject selectedDot, int determine, int index)
    {
        GameObject door = GameObject.Find("fix_door");
        Debug.Log(door);
        door.transform.GetChild(1).GetComponent<DoorController>().open();
        subPanel.clickon();
        if (determine == 0)
        {
            subPanel.dotballoon(selectedDot);
        }
        else
        {
            subPanel.playerballoon(selectedDot);
        }
    }

    public void tutorial_5(GameObject selectedDot, int determine, int index)
    {
        if (determine == 0)
        {
            subPanel.dotballoon(selectedDot);
        }
        else
        {
            subPanel.playerballoon(selectedDot);
        }
        nickname.SetActive(true);
    }

    public void tutorial_7(GameObject selectedDot, int determine, int index)
    {
        cameraZoom.Zoom();
        Recents.Add(new RecentItem { obj = selectedDot, value = determine ,index = index });
        subPanel.gameObject.SetActive(false);
    }

    public void tutorial_8(GameObject selectedDot, int determine, int index)
    {
        Recents.Add(new RecentItem { obj = selectedDot, value = determine, index = index });
        tutorialManager.Dot.ChangeState(DotPatternState.Phase, "anim_watching", 0);
        moonnote = GameObject.FindWithTag("moonnote").GetComponent<Moonnote>();
        StartCoroutine(Scroallable());
    }

    public void tutorial_9(GameObject selectedDot, int determine, int index)
    {
        Recents.Add(new RecentItem { obj = selectedDot, value = determine, index = index });
        if (!tutorialManager)
        {
            Subcontinue();
        }
        else
        {
            dotController.tutorial = false;
            playerController.NextPhase();
            playerController.WritePlayerFile();
            StartCoroutine(LoadSceneCoroutine("MainScene"));
        }
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOperation.isDone)
        {
            Debug.Log($"Loading progress: {asyncOperation.progress * 100}%");
            yield return null;
        }
    }

    public void tuto7()
    {
        tutorialManager.ChangeGameState(TutorialState.Main);
    }

    public void zoomout()
    {
        tutorialManager.ChangeGameState(TutorialState.Sub);
    }

    public void Subcontinue()
    {
        if (Recents.Count > 0)
        {
            RecentItem lastItem = Recents[Recents.Count - 1];
            if (lastItem.value == 0)
            {
                subPanel.dotballoon(lastItem.obj);
            }
            else
            {
                subPanel.playerballoon(lastItem.obj);
            }
        }
    }

    public IEnumerator Scroallable()
    {
        yield return new WaitForSeconds(5f);
        GameObject dot = tutorialManager.Dot.gameObject;
        dot.GetComponent<DotController>().Invisible();
        UIBalloon.SetActive(true);
        moonnote.anion(UIBalloon);
        SystemUI.SetActive(true);
    }

    public void skiptouchGuide()
    {
        GameObject touchguide = Resources.Load<GameObject>(prefabPath);
        if (touchguide != null)
        {
            GameObject instance = Instantiate(touchguide, subPanel.gameObject.transform);
            instance.transform.localPosition = guide3;
            instance.SetActive(true);
            touch = instance.GetComponent<TouchGuide>();
        }
        else
        {
            Debug.LogError("프리팹을 찾을 수 없습니다!");
        }
        touch.skipGuide();
    }
}
