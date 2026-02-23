using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

public class BinocularExitController : MonoBehaviour
{

    GameObject camera;

    private void OnMouseDown()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.iconClick, transform.position);
        DoorController door = FindObjectOfType<DoorController>();
        if (door != null)
        {
            door.SetDoorForDialogue(true);
        }
        //screen�� Ű�� parent�� destroy 
        Destroy(this.transform.parent.gameObject);
        camera = GameObject.FindWithTag("MainCamera");

        if (camera)
        {
            camera.GetComponent<ScrollManager>().StopCamera(false);
        }
        InputGuard.WorldInputLocked = false;
    }


}
