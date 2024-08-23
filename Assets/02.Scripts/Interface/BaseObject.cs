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
    int chapter;

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

    bool IsCurrentChapter(int chpater)
    {
        return this.chapter == chapter;
    }
    
}
