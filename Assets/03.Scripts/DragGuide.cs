using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DragGuide : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] SubDialogue subDialogue;
    private ScrollManager _scrollManager;
    // Start is called before the first frame update
    void Awake()
    {
        _camera = Camera.main;
        _scrollManager = _camera.GetComponent<ScrollManager>();
        subDialogue = GameObject.Find("SubDialougue").GetComponent<SubDialogue>();
        _scrollManager.scrollable();
    }

    private void Update()
    {
        if (_camera.transform.position.x < -1.5f)
        {
            Dragend();
        }
    }
    public void Dragend()
    {
        this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        _scrollManager.stopscroll();
        _scrollManager.MoveCamera(
            new Vector3(-5.5f, 0, -10), 
            2f,
            onComplete: () => {
                if (this != null)
                StartCoroutine(SubStart());  // 카메라 이동 완료 후 실행
            }
        );
    }
    private IEnumerator SubStart()
    {
        yield return new WaitForSeconds(0.5f); 
        subDialogue.Tuto_start(69, 1.5f); 
        Destroy(this.gameObject);
    }

}
