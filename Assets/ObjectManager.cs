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

    public void loadObject(string path)
    {
        GameObject[] obj = Resources.LoadAll<GameObject>(path);

        foreach (GameObject obj2 in obj)
        {
            //Instantiate�� ���ؼ� InsertMemory�� ����

            GameObject newObj = Instantiate(obj2, this.transform.GetChild(0));
            pool.InsertMemory(newObj);

        }
    }

}
