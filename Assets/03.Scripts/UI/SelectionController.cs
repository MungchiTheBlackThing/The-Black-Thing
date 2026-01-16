using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionController : MonoBehaviour
{
    //isFour 변수에 따라 선택지 갯수가 달라진다.
    [SerializeField]
    bool isFour;

    [SerializeField]
    GameObject actionButton;

    private int selectedCount = 0;
    //선택지는 나중에 저장한다.
    public void Choose()
    {
        GameObject option = EventSystem.current.currentSelectedGameObject;
        if (option == null) return;

        // ✅ "Checking"만 토글
        Transform checkingTr = option.transform.Find("Checking");
        if (checkingTr == null)
        {
            Debug.LogError($"[SelectionController] 'Checking' not found under {option.name}");
            return;
        }

        GameObject checking = checkingTr.gameObject;

        if (checking.activeSelf)
        {
            checking.SetActive(false);
            selectedCount--;
        }
        else
        {
            checking.SetActive(true);
            selectedCount++;
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.dialougeSelect, transform.position);
        }

        if (selectedCount < 0) selectedCount = 0;
        if (actionButton != null) actionButton.SetActive(selectedCount > 0);
    }

    public void OnDisable()
    {
        // ✅ 하위 전체에서 "Checking"만 끄기 (구조가 Options/Select... 여도 안전)
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "Checking")
                t.gameObject.SetActive(false);
        }

        selectedCount = 0;
        if (actionButton != null) actionButton.SetActive(false);
    }
}
