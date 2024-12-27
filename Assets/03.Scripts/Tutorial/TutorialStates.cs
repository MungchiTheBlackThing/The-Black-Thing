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

        //��� ���� ��� 
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
                InvokeHelper.Instance.InvokeAfterDelay(substart, 2f);
                subdial = manager.subDialoguePanel;
            }
            if (manager.TutoNum == 1)
            {
                dot.tutorial = false;
                manager.ScrollManager.stopscroll();
                manager.ScrollManager.MoveCamera(new Vector3((float)5.70, 0, -10), 0.1f);
                Debug.Log("�ι�° Ʃ�丮�� ����");
                dot.ChangeState(DotPatternState.Default, anim, 0);
                //InvokeHelper.Instance.InvokeAfterDelay(subcontinue, 4.0f);
            }
            
        }

        public override void Exit(GameManager manager, TutorialManager tutomanger = null)
        {
            //���ξ� �ε�?
        }
        public void substart()
        {
            Debug.Log("Ʃ�丮�� ��ȭ ����");
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
            dot.ChangeState(DotPatternState.Main, "body_default1", 14, "face_null");
            manager.StartTutoMain();
            manager.ScrollManager.MoveCamera(new Vector3(0, 0, -10), 0.1f);
        }

        public override void Exit(GameManager manager, TutorialManager tutomanger)
        {
            Debug.Log("���� Exit2");
            dot.TriggerMain(false);
            Debug.Log(tutomanger);
            manager.ScrollManager.StopCamera(false);
            if (background)
            {
                Debug.Log("���� ���:" + background.name);
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