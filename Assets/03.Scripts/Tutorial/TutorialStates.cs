using Assets.Script.DialClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Tutorial
{
    public class Sub : GameState
    {
        private GameObject subdial;
        const int pos = 3;
        const string anim = "anim_default";
        private bool _initEnterCalled = false;

        public Sub()
        {

        }

        //멤버 변수 대사 
        public override void Init()
        {

        }

        public void InitEnter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
        {//위치 초기화 먼저 하도록 수정
            RecentData data = RecentManager.Load();
            if (data.tutonum == 0)
            {
                if (data != null && data.isContinue == 1)
                {
                    dot.ChangeState(DotPatternState.Default, anim, pos);
                    dot.GetComponent<DotController>().tutorial = true;
                }
                else
                {
                    dot.ChangeState(DotPatternState.Default, anim, pos);
                    dot.GetComponent<DotController>().tutorial = true;
                }
            }
            if (data.tutonum == 1)
            {
                if (data.index == 69)
                {
                    dot.ChangeState(DotPatternState.Default, anim, 1);
                    dot.tutorial = true;
                }
                else
                {
                    dot.ChangeState(DotPatternState.Default, anim, pos);
                    dot.GetComponent<DotController>().tutorial = true;
                }
            }

            _initEnterCalled = true;
        }

        public override void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
        {
            var door = GameObject.Find("fix_door");
            var scroll = manager != null ? manager.ScrollManager : null;
            var subPanel = manager != null ? manager.subDialoguePanel : null;

            Debug.Log($"[Tuto.Sub.Enter] manager={(manager!=null)} dot={(dot!=null)} door={(door!=null)} scroll={(scroll!=null)} subPanel={(subPanel!=null)}");

            if (manager == null || dot == null)
            {
                Debug.LogError("[Tuto.Sub.Enter] manager/dot null. Abort Enter.");
                return;
            }

            if (door == null)
            {
                Debug.LogError("[Tuto.Sub.Enter] fix_door not found. Abort Enter.");
                return;
            }

            if (door.transform.childCount <= 1 || door.transform.GetChild(1).GetComponent<DoorController>() == null)
            {
                Debug.LogError("[Tuto.Sub.Enter] fix_door child(1) DoorController missing. Abort Enter.");
                return;
            }

            if (scroll == null || subPanel == null)
            {
                Debug.LogError("[Tuto.Sub.Enter] ScrollManager/subDialoguePanel missing. Abort Enter.");
                return;
            }

            RecentData data = RecentManager.Load();

            Debug.Log("데이터 튜토리얼 번호: " + data.tutonum + "인덱스" + data.index);

            if (!_initEnterCalled)
            {
                InitEnter(manager, dot, tutomanger);
            }
            if (data.tutonum == 0)
            {
                if (data != null && data.isContinue == 1)
                {
                    manager.ScrollManager.stopscroll();
                    manager.ScrollManager.MoveCamera(new Vector3((float)5.70, 0, -10), 2);
                    Utility.Instance.InvokeAfterDelay(() => recentStart(data.index), 2f);
                    subdial = manager.subDialoguePanel;
                }
                else
                {
                    door.transform.GetChild(1).GetComponent<DoorController>().close();
                    Utility.Instance.WaitForFirstTouch(() => //튜토리얼 시작 전(카메라 이동 전) 클릭 한 번 해야 넘어가도록 걸어줌
                    {
                        manager.ScrollManager.stopscroll();
                        manager.ScrollManager.MoveCamera(new Vector3((float)5.70, 0, -10), 2);
                        Utility.Instance.InvokeAfterDelay(substart, 2f);
                        subdial = manager.subDialoguePanel;
                    });
                }
            }
            else if (data.tutonum == 1)
            {
                if (data.index == 69 && !data.watching)
                {
                    manager.ScrollManager.stopscroll();
                    Debug.Log("두번째 튜토리얼 서브");
                    manager.CameraZoom.ZoomOut();
                    //InvokeHelper.Instance.InvokeAfterDelay(subcontinue, 4.0f);
                    GameObject fix_moonradio = GameObject.Find("fix_moonradio");
                    GameObject moonote = Resources.Load<GameObject>("moonnote");
                    Utility.InstantiatePrefab(moonote, fix_moonradio.transform);
                    subdial = manager.subDialoguePanel;
                }
                else if (data.index == 69 && data.watching)
                {
                    Debug.Log("sd");
                    manager.Menu.tutonum2laterON();
                }
                else
                {
                    manager.ScrollManager.stopscroll();
                    manager.ScrollManager.MoveCamera(new Vector3((float)5.70, 0, -10), 2);
                    Utility.Instance.InvokeAfterDelay(() => recentStart(data.index), 2f);
                    subdial = manager.subDialoguePanel;
                }
            }
            _initEnterCalled = false;
        }

        public override void Exit(GameManager manager, TutorialManager tutomanger = null)
        {
            //메인씬 로드?
        }
        public void substart()
        {
            Debug.Log("튜토리얼 대화 시작");
            subdial.SetActive(true);
            subdial.GetComponent<SubDialogue>().StartSub("tutorial_sub");
        }
        public void subcontinue()
        {
            subdial.GetComponent<SubDialogue>().SubContinue();
        }
        public void recentStart(int index)
        {
            subdial.GetComponent<SubDialogue>().Tuto_start(index, 1.0f);
        }
    }
    public class Main: MainDialogue
    {
        public override void Init()
        {

        }

        public override void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger=null)
        {
            dot.ChangeState(DotPatternState.Main, "body_default1", 16, "face_null");
            manager.StartTutoMain();
            manager.ScrollManager.MoveCamera(new Vector3(0, 0, -10), 0.1f);
        }

        public override void Exit(GameManager manager, TutorialManager tutomanger)
        {
            Debug.Log("메인 Exit2");
            dot.TriggerMain(false);
            Debug.Log(tutomanger);
            manager.ScrollManager.StopCamera(false);
            if (background)
            {
                Debug.Log("현재 배경:" + background.name);
                background.SetActive(false);
            }
            RecentManager.TutoNumChange();
            //manager.ObjectManager.activeSystemUIDelegate(true);
            //SystemUI.SetActive(true);
        }

        public void changestate(GameManager manager)
        {
            TutorialManager.Instance.ChangeGameState(TutorialState.Sub);
        }
    }
};