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
    [SerializeField] GameObject confirmButtonObject;

    private CanvasGroup confirmButtonCanvasGroup;

    private void OnEnable()
    {
        playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>();

        // 버튼 오브젝트에 CanvasGroup이 없으면 추가
        confirmButtonCanvasGroup = confirmButtonObject.GetComponent<CanvasGroup>();
        if (confirmButtonCanvasGroup == null)
            confirmButtonCanvasGroup = confirmButtonObject.AddComponent<CanvasGroup>();

        ValidateInput();
        nameInputField.onValueChanged.AddListener(delegate { ValidateInput(); });
    }

    private void OnDisable()
    {
        nameInputField.onValueChanged.RemoveAllListeners();
    }

    private void ValidateInput()
    {
        string trimmed = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            // 닉네임이 없으면 투명도 50% + 상호작용 비활성화
            confirmButtonCanvasGroup.alpha = 0.5f;
            confirmButtonCanvasGroup.interactable = false;
            confirmButtonCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            // 닉네임 있으면 불투명 + 상호작용 가능
            confirmButtonCanvasGroup.alpha = 1f;
            confirmButtonCanvasGroup.interactable = true;
            confirmButtonCanvasGroup.blocksRaycasts = true;
        }
    }

    public void SaveNickname()
    {
        playerController.SetNickName(nameInputField.text.Trim());
        playerController.WritePlayerFile();
        subPanel.gameObject.SetActive(true);
        this.transform.parent.gameObject.SetActive(false);
    }
}
