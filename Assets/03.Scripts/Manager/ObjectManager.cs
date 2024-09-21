using Assets.Script.TimeEnum;
using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
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

    bool isObjectLoadComplete;
    float loadProgress;

    //Dictionary<���� �ð�, FileID> FileID; ����
    public ObjectManager()
    {
        pool = new ObjectPool();
        mains = new Dictionary<string, GameObject>();
        watches = new List<GameObject>();
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

    // �ε� �Ϸ� ���θ� ��ȯ
    public bool IsLoadObjectComplete()
    {
        return isObjectLoadComplete;
    }

    // ���� �ε� ���� ��Ȳ�� ��ȯ
    public float GetLoadProgress()
    {
        return loadProgress;
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

    // �񵿱� �ε带 ���� �ڷ�ƾ
    public IEnumerator LoadObjectAsync(string path, int chapter)
    {

        const string MainPath = "https://drive.google.com/uc?export=download&id=1ZlmdZEtzqa7yX37gHmFfibFzSkMz73mG";
        Action<AssetBundle> callback = LoadMainBackground;

        yield return StartCoroutine(pool.LoadFromMemoryAsync(MainPath, callback));

        isObjectLoadComplete = false;  // �ε尡 ���۵ǹǷ� false�� ����
        loadProgress = 0f;  // ���� ��Ȳ �ʱ�ȭ

        // ���������� ��ο��� ��� ���ҽ��� ���� �����ɴϴ�. 
        // �̰��� ��ο� � ������Ʈ�� �ִ��� Ȯ���ϴ� �ܰ��� ��, ���� ������Ʈ�� �ε����� ����.

        //���� �ε带 ���⼭ �ε�����

        System.Object[] allObjects = Resources.LoadAll(path, typeof(GameObject));
        int totalObjects = allObjects.Length;

        int i = 0;

        foreach (GameObject obj in allObjects)
        {
            // �� ������Ʈ�� �񵿱������� �ε�
            ResourceRequest resourceRequest = Resources.LoadAsync<GameObject>(path + "/" + obj.name);
            
            while (!resourceRequest.isDone)
            {
                loadProgress = (i + resourceRequest.progress) / totalObjects;  // ����� ������Ʈ
                yield return null;
            }

            if (resourceRequest.asset != null)
            {
                GameObject obj2 = resourceRequest.asset as GameObject;

                // Instantiate�� ���� ������Ʈ ���� �� ����
                GameObject newObj = Instantiate(obj2, this.transform);

                // "(Clone)" ����
                string name = newObj.name.Substring(0, newObj.name.IndexOf("("));
                newObj.name = name;

                // InsertMemory �� ����
                pool.InsertMemory(newObj);

                if (newObj.GetComponent<BaseObject>().IsCurrentChapter(chapter))
                {
                    newObj.SetActive(true);
                }
                else
                {
                    newObj.SetActive(false);
                }
            }
            
            i++;

        }

        yield return new WaitForSeconds(1f);
        isObjectLoadComplete = true;
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
