using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    //��� ���°� ������Ʈ���� �����Ѵ�.
    private ObjectPool pool;

    public ObjectManager()
    {
        pool = new ObjectPool();
    }

    public void LoadObject(string path, int chapter)
    {
        GameObject[] obj = Resources.LoadAll<GameObject>(path);

        foreach (GameObject obj2 in obj)
        {
            //Instantiate�� ���ؼ� InsertMemory�� ����
            GameObject newObj = Instantiate(obj2, this.transform.GetChild(0));

            if(newObj.GetComponent<BaseObject>().IsCurrentChapter(chapter))
            {
                newObj.SetActive(true);
            }
            else
            {
                newObj.SetActive(false);
            }

            pool.InsertMemory(newObj);
        }
    }


    public void SettingChapter(int chapter)
    {

    }
}
