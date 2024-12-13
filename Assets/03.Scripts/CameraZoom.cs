using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    Animator animator;
    Vector2 originVector;

    [SerializeField] SubTuto subTuto;
    // Start is called before the first frame update
    void Start()
    {
        originVector = this.transform.position;
        animator = this.GetComponent<Animator>();
        animator.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Zoom()
    {
        animator.enabled=true;
        animator.SetTrigger("Zoom");
    }
    public void ZoomEnd()
    {
        animator.enabled=false;
        Camera cam = Camera.main;
        cam.orthographicSize = 6.45f;
        cam.gameObject.transform.position = new Vector3(0, 0, -10);
        subTuto.tuto7();
    }

    public void ZoomOut()
    {
        animator.enabled = true;
        animator.SetTrigger("ZoomOut");
        Debug.Log("Ä«¸Þ¶ó ÁÜ¾Æ¿ô");
        subTuto.zoomout();
    }
    public void ZoomOutEnd()
    {
        animator.enabled = false;
        Camera cam = Camera.main;
        cam.orthographicSize = 6.45f;
        Debug.Log("ÁÜ¾Æ¿ô ³¡");
    }
}
