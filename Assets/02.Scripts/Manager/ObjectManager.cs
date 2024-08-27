using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ObjectPool;

public class ObjectManager : MonoBehaviour
{
    //��� ���°� ������Ʈ���� �����Ѵ�.
    private ObjectPool pool;

    TranslateManager translator;
    public ObjectManager()
    {
        pool = new ObjectPool(); 
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

    public void Translate(LANGUAGE language)
    {
        Debug.Log("���� ������Ʈ �����մϴ�.\n");
    }
}
