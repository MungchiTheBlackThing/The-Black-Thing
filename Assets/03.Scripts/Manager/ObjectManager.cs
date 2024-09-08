using Assets.Script.TimeEnum;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ObjectPool;

public class ObjectManager : MonoBehaviour
{
    //��� ���°� ������Ʈ���� �����Ѵ�.
    private ObjectPool pool;

    List<GameObject>  watches;


    [SerializeField]
    GameObject skipSleep;

    Dictionary<string, GameObject> mains;

    public delegate void ActiveSystemUIDelegate(bool InActive);

    public ActiveSystemUIDelegate activeSystemUIDelegate;
    public ObjectManager()
    {
        pool = new ObjectPool();
        mains = new Dictionary<string, GameObject>();
        watches = new List<GameObject>();
    }

    public void InitMainBackground(string InPath)
    {
        string path = "";
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            path = Application.dataPath + "/Raw/"+InPath;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            path = "jar:file://" + Application.dataPath + "!/assets/"+InPath;
        }
        else
        {
            path = Application.dataPath + "/StreamingAssets/"+ InPath;
        }

        Action<AssetBundle> callback = LoadMainBackground;

        StartCoroutine(pool.LoadFromMemoryAsync(path, callback));

    }

    public GameObject SetMain(string background)
    {

        if(mains.ContainsKey(background))
        {

            foreach (var w in mains)
            {
                w.Value.SetActive(false);
            }

            mains[background].SetActive(true);

            return mains[background];
        }

        return null;
    }

    private void LoadMainBackground(AssetBundle bundle)
    {
        if(bundle != null)
        {
            GameObject[] prefab = bundle.LoadAllAssets<GameObject>();
            

            foreach(GameObject pf in prefab)
            {
                GameObject realObj = Instantiate(pf,this.transform);

                string name = realObj.name.Substring(0, realObj.name.IndexOf("(")); 
                realObj.name = name; //(clone)�� ã�Ƴ�.
                mains.Add(name,realObj);
                realObj.SetActive(false);
            }

            bundle.Unload(false);
        }
    }

    public void LoadObject(string path, int chapter)
    {
        GameObject[] obj = Resources.LoadAll<GameObject>(path);
        foreach (GameObject obj2 in obj)
        {
            //Instantiate�� ���ؼ� InsertMemory�� ����
            GameObject newObj = Instantiate(obj2, this.transform);
            string name = newObj.name.Substring(0, newObj.name.IndexOf("("));
            newObj.name = name; //(clone)�� ã�Ƴ�.
            //newObj�� clone�� ���� 
            pool.InsertMemory(newObj);
        }
    }

    //�� é�͸� �Ѱ��� �� ȣ��Ǵ� �Լ�, �� Phase Watching�� �� ȣ���Ѵ�. 
    public void SettingChapter(int chapter)
    {

        //��� Obj�� �����ͼ� �˻��ؾ��Ѵ�.
        List<GameObject> values = pool.GetValues();

        foreach (GameObject value in values)
        {
            if (value.GetComponent<BaseObject>().IsCurrentChapter(chapter))
            {
                value.SetActive(true);
            }
            else
            {
                value.SetActive(false);
            }
        }
    }

    public IWatchingInterface GetWatchingObject(EWatching type)
    {
        IWatchingInterface search = null;
        List<GameObject> values;
        if (watches.Count == 0)
        {
            //��� Obj�� �����ͼ� �˻��ؾ��Ѵ�.
            values = pool.GetValues();
        }
        else
        {
            values = watches;
        }

        foreach (GameObject value in values)
        {
            IWatchingInterface watching = value.GetComponent<IWatchingInterface>();

            if (watching != null)
            {
                if (watching.IsCurrentPattern(type))
                {
                    search = watching;
                }

                if(watches.Count < 2)
                {
                    Debug.Log(watching.ToString());
                    watches.Add(value);
                }
            }

        }

        return search;
    }

    public ISleepingInterface GetSleepingObject()
    {
        List<GameObject> values = pool.GetValues();

        foreach (GameObject value in values)
        {
            ISleepingInterface search = value.GetComponent<ISleepingInterface>();

            if (search != null)
            {
                return search;
            }
        
        }

        return null;
    }

    public void PlayThinking()
    {
        GameObject bookPile=pool.SearchMemory("phase_bookpile");

        if(bookPile)
        {
            bookPile.SetActive(false);
        }
    }
    public void Translate(LANGUAGE language)
    {
        Debug.Log("���� ������Ʈ �����մϴ�.\n");
    }

    public void SkipSleeping(bool isActive)
    {
        skipSleep.SetActive(isActive);
    }

}
