using Assets.Script.DialClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tutorial
{
    
    public class Sub : GameState
    {
        public Sub()
        {

        }

        //��� ���� ��� 
        public override void Init()
        {
            
        }

        public override void Enter(GameManager manager, DotController dot = null)
        {
            Debug.Log("Ʃ�丮�� ���� ����");
            GameObject door = GameObject.Find("fix_door");
            Debug.Log(door);
            door.transform.GetChild(1).GetComponent<DoorController>().close();
            manager.ScrollManager.MoveCamera(new Vector3((float)5.70, 0, -10), 2);
        }

        public override void Exit(GameManager manager)
        {

        }

    }

    public class Main: MainDialogue
    {

        //��� ���� ��� 
        public override void Init()
        {

        }

        public override void Enter(GameManager manager, DotController dot = null)
        {
            Debug.Log("Ʃ�丮�� ���� ����");
        }

        public override void Exit(GameManager manager)
        {

        }
    }

};