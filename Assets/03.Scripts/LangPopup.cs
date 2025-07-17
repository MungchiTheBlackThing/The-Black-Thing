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
    [SerializeField] MypageUIController mypage;

    void Start()
    {
        // ��Ӵٿ� �� ���� �̺�Ʈ�� ������ �߰�
        myDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDropdownValueChanged(int index)
    {
        string selectedText = myDropdown.options[index].text;
        Debug.Log($"[�θ�] ���õ� �ε���: {index}, �ؽ�Ʈ: {selectedText}");

        if (intro) //���� ó�� ���۽ÿ��� ���
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
