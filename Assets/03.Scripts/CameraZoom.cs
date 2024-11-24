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
    public void Anioff()
    {
        animator.enabled=false;
        Camera cam = Camera.main;
        cam.orthographicSize = 6.45f;
        subTuto.tuto7();
    }
}
