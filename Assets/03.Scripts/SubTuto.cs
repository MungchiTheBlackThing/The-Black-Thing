using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubTuto : MonoBehaviour
{
    [SerializeField] SubPanel subPanel;
    [SerializeField] TouchGuide touch;
    [SerializeField] GameObject nickname;
    [SerializeField] TutorialManager tutorialManager;
    [SerializeField] CameraZoom cameraZoom;
    [SerializeField] 
    public GameObject UIBalloon;
    
    public string prefabPath = "TouchGuide"; 
    public Vector3 guide1 = new Vector3(-810, -145, 0);
    public Vector3 guide2 = new Vector3(-1095, -195, 0);
    public List<(GameObject,int)> Recents = new List<(GameObject,int)> ();
    // Update is called once per frame
    public void tutorial_2(GameObject selectedDot, int determine)
    {
        GameObject touchguide = Resources.Load<GameObject>(prefabPath);
        if (touchguide != null)
        {
            // 인스턴스화 및 활성화
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

    public void tutorial_3(GameObject selectedDot, int determine)
    {
        GameObject touchguide = Resources.Load<GameObject>(prefabPath);
        if (touchguide != null)
        {
            // 인스턴스화 및 활성화
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
    public void tutorial_4(GameObject selectedDot, int determine)
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

    public void tutorial_5(GameObject selectedDot, int determine)
    { 
        if (determine == 0)
        {
            subPanel.dotballoon(selectedDot);
        }
        else
        {
            subPanel.playerballoon(selectedDot);
        }
        //subPanel.gameObject.SetActive(false);
        nickname.SetActive(true);
    }

    public void tutorial_7(GameObject selectedDot, int determine)
    {
        cameraZoom.Zoom();
        Recents.Add((selectedDot, determine));
        subPanel.gameObject.SetActive(false);
    }

    public void tutorial_8(GameObject selectedDot, int determine)
    {
        Recents.Add((selectedDot, determine));
        tutorialManager.Dot.ChangeState(DotPatternState.Phase, "anim_watching", 0);
        StartCoroutine(Scroallable());
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
        if (Recents.Count > 0 && Recents[Recents.Count - 1].Item2 == 0)
        {
            subPanel.dotballoon(Recents[Recents.Count - 1].Item1);
        }
        else
        {
            subPanel.playerballoon(Recents[Recents.Count - 1].Item1);
        }
    }

    public IEnumerator Scroallable()
    {
        yield return new WaitForSeconds(5f);
        GameObject dot = tutorialManager.Dot.gameObject;
        dot.GetComponent<DotController>().Invisible();
        //cameraZoom.gameObject.GetComponent<ScrollManager>().scrollable();
        // 여기에 다이어리 쪽지 관련 플레이어 말 띄우기, 불빛 애니메이션 실행
        UIBalloon.SetActive(true);
    }


}
