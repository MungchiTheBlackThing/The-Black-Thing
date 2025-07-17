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
    [SerializeField] MypageUIController mypage;

    void Start()
    {
        // 드롭다운 값 변경 이벤트에 리스너 추가
        myDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDropdownValueChanged(int index)
    {
        string selectedText = myDropdown.options[index].text;
        Debug.Log($"[부모] 선택된 인덱스: {index}, 텍스트: {selectedText}");

        if (intro) //게임 처음 시작시에만 사용
        {
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
        else
        {
            switch (myDropdown.value)
            {
                case 0:
                    mypage.SetKorean();
                    break;
                case 1:
                    mypage.SetEnglish();
                    break;
            }
        }
    }

   
    void OnDestroy()
    {
        myDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }
}
