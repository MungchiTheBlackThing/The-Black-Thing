using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cursor : MonoBehaviour
{
    private enum name
    {
        bino2,
        bino5,
        bino8,
        bino10,
        bino13
    }
    [SerializeField]
    GameObject camera;

    public bool isrelease = false;
    public bool isSuccess = false;
    public GameObject target;
    [SerializeField]
    private name num;

    [SerializeField]
    GameObject systemUI;
    void Start()
    {
        isrelease = false;
    }

    private void OnEnable()
    {
        if(camera == null)
        {
            camera = GameObject.FindWithTag("MainCamera");
        }

        if (systemUI == null)
        {
            systemUI = GameObject.Find("SystemUI");
        }

        if (camera)
        {
            camera.GetComponent<ScrollManager>().StopCamera(true);
        }

        var height = 2 * Camera.main.orthographicSize - 2;
        var width = height * Camera.main.aspect - 2;

        width = Random.Range(-(width / 2), width / 2);
        height = Random.Range(-(height / 2), height / 2);
        this.transform.position = new Vector3(width, height);

        //위치 랜덤으로 변경
        if(systemUI)
        {
            systemUI.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if(systemUI)
        {
            systemUI.SetActive(true);
        }
    }
    private void Update() 
    {
        if(isSuccess == false)
        {
            if (Input.GetMouseButton(0))
            {
                ObjectMove();
            }
            if (Input.GetMouseButtonDown(0))
            {
                List<RaycastResult> results = new();
                        //마우스 클릭한 좌표값 가져오기
                Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        //해당 좌표에 있는 오브젝트 찾기
                RaycastHit2D[] hit = Physics2D.RaycastAll(pos, Vector2.zero, 0f);

                for (int i = 0; i < hit.Length; i++)
                    if (hit[i].collider.tag == "Mungchi")
                    {
                        target.GetComponent<MungchiClick>().OnMouseDown();
                        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.binosuccess[(int)num], this.transform.position);
                        isSuccess =true;
                        break;
                    }
                    else
                    {
                        int index = Random.Range(0, 2);
                        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.binofails[index], this.transform.position);
                    }
            }
        }
        
    }

    private void ObjectMove()
    {
        // Screen 좌표계인 mousePosition을 World 좌표계로 바꾼다
        Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                Input.mousePosition.y, -Camera.main.transform.position.z));

        transform.position = point;
    }

}
