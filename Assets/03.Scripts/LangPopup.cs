using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class LangPopup : MonoBehaviour
{
    const string playerInfoDataFileName = "PlayerData.json";
    public TMP_Dropdown myDropdown; // 인스펙터에서 연결
    [SerializeField] IntroScene intro;
    [SerializeField] PlayerInfo playerInfo;

    void Start()
    {
        // 드롭다운 값 변경 이벤트에 리스너 추가
        myDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDropdownValueChanged(int index)
    {
        string selectedText = myDropdown.options[index].text;
        Debug.Log($"[부모] 선택된 인덱스: {index}, 텍스트: {selectedText}");

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
