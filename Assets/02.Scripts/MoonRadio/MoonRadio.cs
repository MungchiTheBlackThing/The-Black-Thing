using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoonRadio : BaseObject
{
    [SerializeField]
    GameObject MoonRadioController;

    Animator blinkMoonRadioAnim;

    private void Start()
    {
        blinkMoonRadioAnim = GetComponent<Animator>();
        MoonRadioController = GameObject.Find("MoonRadio").transform.GetChild(0).gameObject;
    }
    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // ���콺�� UI ���� ���� ���� �� �Լ��� �������� �ʵ��� ��
            return;
        }

        if (MoonRadioController.activeSelf == false)
        {
            //���� �� speed 1, ���� �ƴϸ� 0
            blinkMoonRadioAnim.SetFloat("speed", 0f);
            MoonRadioController.SetActive(true);
        }
    }
}
