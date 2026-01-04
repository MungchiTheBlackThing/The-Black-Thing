using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndAnimSingleton : MonoBehaviour
{
    private static EndAnimSingleton _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);   // 중복이면 자폭
            return;
        }
        _instance = this;
    }
}
