using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Moonnote : MonoBehaviour
{
    [SerializeField] GameObject MoonnoteUI;

    public GameObject MoonnoteLight;
    public Animator animator;
    public Transform canvasTransform;
    public GameObject UIBalloon;
    // Start is called before the first frame update
    void Start()
    {
        MoonnoteUI = Resources.Load<GameObject>("MoonnoteUI");
        animator = this.transform.GetChild(0).GetComponent<Animator>();
        canvasTransform = GameObject.Find("Canvas").transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // 마우스가 UI 위에 있을 때는 이 함수가 동작하지 않도록 함
            return;
        }
        Debug.Log("문노트 클릭");
        if (MoonnoteLight.activeSelf == true)
        {
            Instantiate(MoonnoteUI, canvasTransform);
            MoonnoteUI.SetActive(true);
            MoonnoteLight.SetActive(false);
            UIBalloon.SetActive(false);
        }
    }

    public void anion(GameObject Balloon)
    {
        UIBalloon = Balloon;
        MoonnoteLight.SetActive(true);
    }
}
