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
    [SerializeField] GameObject confirmButtonObject; // Button의 GameObject 자체 (Button 말고!)

    private void OnEnable()
    {
        playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        ValidateInput(); // 팝업이 열릴 때도 검사
        nameInputField.onValueChanged.AddListener(delegate { ValidateInput(); });
    }

    private void OnDisable()
    {
        nameInputField.onValueChanged.RemoveAllListeners(); // 리스너 정리
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
