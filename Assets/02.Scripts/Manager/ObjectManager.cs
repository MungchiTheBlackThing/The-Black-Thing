using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ObjectPool;

public class ObjectManager : MonoBehaviour
{
    //��� ���°� ������Ʈ���� �����Ѵ�.
    private ObjectPool pool;

    TranslateManager translator;

    List<GameObject>  watches;

    public ObjectManager()
    {
        pool = new ObjectPool();
        watches = new List<GameObject>();
    }

    private void Start()
    {
        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();
        translator.translatorDel += Translate;
    }
    public void LoadObject(string path, int chapter)
    {
        GameObject[] obj = Resources.LoadAll<GameObject>(path);
        foreach (GameObject obj2 in obj)
        {
            //Instantiate�� ���ؼ� InsertMemory�� ����
            GameObject newObj = Instantiate(obj2, this.transform.GetChild(0));
            
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

    public void Translate(LANGUAGE language)
    {
        Debug.Log("���� ������Ʈ �����մϴ�.\n");
    }
}
