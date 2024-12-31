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
            // ���콺�� UI ���� ���� ���� �� �Լ��� �������� �ʵ��� ��
            return;
        }
        Debug.Log("����Ʈ Ŭ��");
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
