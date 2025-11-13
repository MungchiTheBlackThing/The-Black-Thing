using Assets.Script.DialClass;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public enum EMoonChacter
{
    red,
    edison,
    sircello
}

[System.Serializable]
struct BubblePrefab
{
    [SerializeField]
    public EMoonChacter type;
    [SerializeField]
    public GameObject prefab;
}
public class MoonChatClickController : MonoBehaviour, IPointerDownHandler
{
    // Start is called before the first frame update
    [SerializeField]
    List<BubblePrefab> pref;
    
    [SerializeField]
    List<GameObject> radioScript;

    [SerializeField]
    GameObject exitBut;

    [SerializeField]
    ScrollRect scrollrect; 

    IPlayerInterface pc;

    int number = 1;
    int curIdx;
    public void OnEnable()
    {
        curIdx = 0;
        if (pc != null)
        {
            Init(pc.GetChapter(), pc.GetMoonRadioIdx(), pc.GetLanguage());
        }
    }

    public void Start()
    {
        radioScript = new List<GameObject>();

        pc = GameObject.FindWithTag("Player").GetComponent<IPlayerInterface>();
                if(pc != null)
        {
            Init(pc.GetChapter(), pc.GetMoonRadioIdx() ,pc.GetLanguage());
        }
    }

    void Init(int chapter, int number, LANGUAGE lan)
    {

        for(int i=0;i< radioScript.Count;i++)
        {
            Destroy(radioScript[i]);
        }
        radioScript.Clear();

        List<MoonRadioDial> Dial = DataManager.Instance.MoonRadioParser.GetMoonRadioDial(chapter, number, lan);
        
        int len = Dial.Count;
        for (int i = 0; i < len; i++)
        {
            GameObject moonRadioObj = Instantiate(pref[(int)Dial[i].Actor].prefab, this.transform);

            string key = Dial[i].TextKey;
            string localizedText = LocalizationSettings.StringDatabase
            .GetLocalizedString("MoonRadioText", key);
            
            moonRadioObj.GetComponent<ChatAreaScript>().SettingText(localizedText);
            moonRadioObj.SetActive(false);

            if (i == 0)
                moonRadioObj.SetActive(true);
            else
                moonRadioObj.SetActive(false);

            radioScript.Add(moonRadioObj);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        RunScript();
    }

    public void RunScript()
    {
        curIdx += 1;
        if (curIdx >= radioScript.Count)
        {
            if (!exitBut.activeSelf)
            {
                exitBut.SetActive(true);
            }
            return;
        }
        radioScript[curIdx].gameObject.SetActive(true);

        StartCoroutine(ScrollToBottom()); // 주희 추가, 새 채팅 하단에 고정하도록
    }
 
    IEnumerator ScrollToBottom()   //// 주희 추가 함수, 레이아웃 강제 업데이트 및 코루틴 개선
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollrect.content);
        yield return null; // 추가로 한 프레임 더 대기
        scrollrect.verticalNormalizedPosition = 0f;
   }



    public void Reset(int MoonRadioIdx)
    {
        Init(pc.GetChapter(), MoonRadioIdx, pc.GetLanguage()); 
    }
}
