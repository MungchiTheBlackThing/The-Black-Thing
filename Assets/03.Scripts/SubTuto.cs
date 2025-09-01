using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SubTuto : MonoBehaviour
{
    [SerializeField] SubDialogue subDialogue;
    [SerializeField] SubPanel subPanel;
    [SerializeField] TouchGuide touch;
    [SerializeField] GameObject nickname;
    [SerializeField] TutorialManager tutorialManager;
    [SerializeField] CameraZoom cameraZoom;
    [SerializeField] public GameObject UIBalloon;
    [SerializeField] public Moonnote moonnote;
    [SerializeField] GameObject SystemUI;
    [SerializeField] PlayerController playerController;
    [SerializeField] DotController dotController;

    public string prefabPath = "TouchGuide";

    Vector3 guide1 = new Vector3(-810, -145, 0);
    Vector3 guide2 = new Vector3(-1095, -195, 0);
    Vector3 guide3 = new Vector3(-1100, -400, 0);

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
        door.transform.GetChild(1).GetComponent<DoorController>().open();
        subPanel.clickon();

        if (determine == 0)
            subPanel.dotballoon(selectedDot);
        else
            subPanel.playerballoon(selectedDot);
    }

    public void tutorial_5(GameObject selectedDot, int determine, int index)
    {
        RecentManager.Save(selectedDot, determine, index); // 저장
        if (determine == 0)
            subPanel.dotballoon(selectedDot);
        else
            subPanel.playerballoon(selectedDot);

        nickname.SetActive(true);
    }

    public void tutorial_7(GameObject selectedDot, int determine, int index)
    {
        RecentData data = RecentManager.Load();
        if (data.tutonum == 0)
        {
            cameraZoom.Zoom();
            subPanel.gameObject.SetActive(false);
        }
        else
        {
            subPanel.dotballoon(selectedDot);
        }
        RecentManager.Save(selectedDot, determine, index); // 저장
    }

    public void tutorial_8(GameObject selectedDot, int determine, int index)
    {
        //RecentManager.Save(selectedDot, determine, index); // 저장
        RecentData data = RecentManager.Load();
        data.value = 1;
        RecentManager.Save(selectedDot, 1, 69); // 저장
        tutorialManager.Dot.ChangeState(DotPatternState.Phase, "anim_watching", 1.5f);
        moonnote = GameObject.FindWithTag("moonnote").GetComponent<Moonnote>();
        StartCoroutine(Scroallable());
    }

    public void tutorial_9(GameObject selectedDot, int determine, int index)
    {
        //RecentManager.Save(selectedDot, determine, index); // 저장
        if (!tutorialManager)
        {
            Subcontinue();
        }
        else
        {
            dotController.tutorial = false;
            Debug.Log("3");
            playerController.NextPhase();
            playerController.WritePlayerFile();
            RecentManager.tutoSceneEnd();
            LoadSceneManager.Instance.LoadScene("Tutorial", "MainScene", 1);
        }
    }

    public void tutorial_10(GameObject selectedDot, int determine, int index)
    {
        //RecentManager.Save(selectedDot, determine, index); // 저장
        if (playerController.GetAlreadyEndedPhase() == 5)
        {
            Subcontinue();
        }
        else
        {
            subDialogue.TutoExit();
            playerController.NextPhase();
            playerController.WritePlayerFile();
        }
    }

    public void tutorial_11(GameObject selectedDot, int determine, int index)
    {
        //RecentManager.Save(selectedDot, determine, index); // 저장
        dotController.GoSleep();
        StartCoroutine(subcon());
    }

    private IEnumerator subcon()
    {
        yield return new WaitForSeconds(4f);
        Subcontinue();
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
        RecentData data = RecentManager.Load();
        if (data != null && data.isContinue == 1)
        {
            GameObject targetObj = subPanel.FindPanelObjectByName(data.objectName);
            if (targetObj == null)
            {
                Debug.LogWarning($"오브젝트 {data.objectName} 를 찾을 수 없습니다.");
                return;
            }

            subPanel.prePos = dotController.Position;

            if (subDialogue.currentDialogueList == null || subDialogue.currentDialogueList.Count == 0)
            {
                Debug.Log("서브이어 1");
                subDialogue.StartSub("tutorial_sub", data.index);
            }
            else
            {
                Debug.Log("서브이어 2");
                if (data.value == 0)
                    subPanel.dotballoon(targetObj);
                else
                    subPanel.playerballoon(targetObj);
            }
        }
        else
        {
            Debug.Log("이어할 튜토리얼 데이터 없음");
        }
    }

    public IEnumerator Scroallable()
    {
        yield return new WaitForSeconds(4f);
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