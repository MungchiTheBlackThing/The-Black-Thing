using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NicknamePopup : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] SubTuto subTuto;
    [SerializeField] SubPanel subPanel;
    [SerializeField] GameObject confirmButtonObject; // Button�� GameObject ��ü (Button ����!)

    private void OnEnable()
    {
        playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        ValidateInput(); // �˾��� ���� ���� �˻�
        nameInputField.onValueChanged.AddListener(delegate { ValidateInput(); });
    }

    private void OnDisable()
    {
        nameInputField.onValueChanged.RemoveAllListeners(); // ������ ����
    }

    private void ValidateInput()
    {
        string trimmed = nameInputField.text.Trim();
        confirmButtonObject.SetActive(!string.IsNullOrEmpty(trimmed));
    }

    public void SaveNickname()
    {
        playerController.SetNickName(nameInputField.text.Trim());
        playerController.WritePlayerFile();
        subPanel.gameObject.SetActive(true);
        this.transform.parent.gameObject.SetActive(false);
    }
}
