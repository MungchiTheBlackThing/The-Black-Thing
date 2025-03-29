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
        
        public Sub()
        {

        }

        //멤버 변수 대사 
        public override void Init()
        {

        }

        public override void Enter(GameManager manager, DotController dot = null, TutorialManager tutomanger = null)
        {
            GameObject door = GameObject.Find("fix_door");
           
            if (manager.TutoNum == 0)
            {
                dot.ChangeState(DotPatternState.Default, anim, pos);
                dot.GetComponent<DotController>().tutorial = true;
                door.transform.GetChild(1).GetComponent<DoorController>().close();
                manager.ScrollManager.stopscroll();
                manager.ScrollManager.MoveCamera(new Vector3((float)5.70, 0, -10), 2);
                Utility.Instance.InvokeAfterDelay(substart, 2f);
                subdial = manager.subDialoguePanel;
            }
            if (manager.TutoNum == 1)
            {
                dot.tutorial = true;
                manager.ScrollManager.stopscroll();
                manager.ScrollManager.MoveCamera(new Vector3((float)5.70, 0, -10), 0.1f);
                Debug.Log("두번째 튜토리얼 서브");
                dot.ChangeState(DotPatternState.Default, anim, 1);
                //InvokeHelper.Instance.InvokeAfterDelay(subcontinue, 4.0f);
                GameObject fix_moonradio = GameObject.Find("fix_moonradio");
                GameObject moonote = Resources.Load<GameObject>("moonnote");
                Utility.InstantiatePrefab(moonote, fix_moonradio.transform);
            }
            
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
            manager.TutoNum = 1;
            //manager.ObjectManager.activeSystemUIDelegate(true);
            //SystemUI.SetActive(true);
            
        }

        public void changestate(GameManager manager)
        {
            TutorialManager.Instance.ChangeGameState(TutorialState.Sub);
        }
    }
};