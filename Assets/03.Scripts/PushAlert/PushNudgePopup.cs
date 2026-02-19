using System;
using TMPro;
using UnityEngine;

public class PushNudgePopup : MonoBehaviour
{
    [SerializeField] TMP_Text bodyText;
    [SerializeField] TMP_Text confirmButtonText;
    [SerializeField] TMP_Text cancelButtonText;

    Action _onConfirm;
    Action _onCancel;

    public void Show(string text, string confirmLabel, string cancelLabel, Action onConfirm, Action onCancel)
    {
        bodyText.text = text;
        confirmButtonText.text = confirmLabel;
        cancelButtonText.text = cancelLabel;
        _onConfirm = onConfirm;
        _onCancel = onCancel;
        gameObject.SetActive(true);
    }

    public void OnConfirmClicked()
    {
        gameObject.SetActive(false);
        _onConfirm?.Invoke();
    }

    public void OnCancelClicked()
    {
        gameObject.SetActive(false);
        _onCancel?.Invoke();
    }
}