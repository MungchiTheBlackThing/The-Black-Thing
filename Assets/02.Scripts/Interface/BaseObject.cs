using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

/*������Ʈ Ǯ�� ����� BaseObject*/
public class BaseObject : MonoBehaviour
{
    static int nextValidID = 0;
    int id;

    [SerializeField]
    List<bool> chapter;

    public int ID
    {
        set
        {
            id = nextValidID++;
        }
        get
        {
            return id;
        }
    }

    public bool IsCurrentChapter(int chapter)
    {
        //chapter�� 1�� ŭ.
        return this.chapter[chapter-1];
    }
    
}
