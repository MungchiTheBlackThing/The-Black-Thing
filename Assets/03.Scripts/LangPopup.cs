using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class LangPopup : MonoBehaviour
{
    const string playerInfoDataFileName = "PlayerData.json";
    public TMP_Dropdown myDropdown; // �ν����Ϳ��� ����
    [SerializeField] IntroScene intro;
    [SerializeField] PlayerInfo playerInfo;

    void Start()
    {
        // ��Ӵٿ� �� ���� �̺�Ʈ�� ������ �߰�
        myDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDropdownValueChanged(int index)
    {
        string selectedText = myDropdown.options[index].text;
        Debug.Log($"[�θ�] ���õ� �ε���: {index}, �ؽ�Ʈ: {selectedText}");

        switch (myDropdown.value)
        {
            case 0:
                playerInfo.language = LANGUAGE.KOREAN;
                if (intro)
                    intro.WritePlayerFile();
                break;
            case 1:
                playerInfo.language = LANGUAGE.ENGLISH;
                if (intro)
                    intro.WritePlayerFile();
                break;
        }
    }

   
    void OnDestroy()
    {
        myDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }
}
