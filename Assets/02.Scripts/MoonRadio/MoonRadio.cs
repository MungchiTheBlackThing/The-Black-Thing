using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MoonRadio : BaseObject
{
    [SerializeField]
    GameObject MoonRadioController;

    Animator blinkMoonRadioAnim;

    private void Start()
    {
        blinkMoonRadioAnim = GetComponent<Animator>();
    }
    private void OnMouseDown()
    {
        if(MoonRadioController.activeSelf == false)
        {
            //���� �� speed 1, ���� �ƴϸ� 0
            blinkMoonRadioAnim.SetFloat("speed", 0f);
            MoonRadioController.SetActive(true);
        }
    }
}
