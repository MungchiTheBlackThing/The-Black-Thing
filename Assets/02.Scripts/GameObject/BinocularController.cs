using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BinocularController : MonoBehaviour , IWatchingInterface
{
    [SerializeField]
    GameObject alert;
   
    [SerializeField]
    List<GameObject> watching;
    
    int phaseIdx = 0; //-1�� �ٲ������.

    GameObject watchingBackground;
    GameObject screenBackground;
    GameObject phase;

    private void Start()
    {
        watchingBackground = GameObject.Find("Phase").gameObject;
        screenBackground = GameObject.FindWithTag("ObjectManager").gameObject.transform.GetChild(0).gameObject;
    }
    public void OpenWatching(int Chapter)
    {
        alert.SetActive(true);
        phaseIdx += 1;
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // ���콺�� UI ���� ���� ���� �� �Լ��� �������� �ʵ��� ��
            return;
        }

        if (alert.activeSelf)
        {
            screenBackground.SetActive(false);
            //Ŭ����~
            phase = Instantiate(watching[phaseIdx], watchingBackground.transform);
        }
    }
}
