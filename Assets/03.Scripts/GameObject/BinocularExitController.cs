using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

public class BinocularExitController : MonoBehaviour
{

    GameObject camera;

    private void OnMouseDown()
    {
        //screen�� Ű�� parent�� destroy 
        Destroy(this.transform.parent.gameObject);
        camera = GameObject.FindWithTag("MainCamera");

        if (camera)
        {
            camera.GetComponent<ScrollManager>().StopCamera(false);
        }
    }


}
