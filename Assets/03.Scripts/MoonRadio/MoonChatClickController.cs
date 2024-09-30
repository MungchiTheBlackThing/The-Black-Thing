using Assets.Script.DialClass;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
public class MoonChatClickController : MonoBehaviour, IChatInterface
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

        //MoonRadio�� �б� ���� ���� ���� ���½��Ѿ���.
        List<MoonRadioDial> Dial = DataManager.Instance.MoonRadioParser.GetMoonRadioDial(chapter, number, lan);
        
        int len = Dial.Count;
        for (int i = 0; i < len; i++)
        {
            GameObject moonRadioObj = Instantiate(pref[(int)Dial[i].Actor].prefab, this.transform);

            moonRadioObj.GetComponent<ChatAreaScript>().SettingText(Dial[i].KorText,this);
            moonRadioObj.SetActive(false);

            if (i == 0)
                moonRadioObj.SetActive(true);
            else
                moonRadioObj.SetActive(false);

            radioScript.Add(moonRadioObj);
        }
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
    }


    public void Reset(int MoonRadioIdx)
    {
        //������ number + 1�� ����.
        Init(pc.GetChapter(), MoonRadioIdx, pc.GetLanguage()); 
    }
}
