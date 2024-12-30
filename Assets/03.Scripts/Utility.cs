using UnityEngine;
using System;
using System.Collections;

public class Utility : MonoBehaviour
{
    public static Utility Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Action�� ���� �ð� �Ŀ� �����ϴ� �޼���
    public void InvokeAfterDelay(Action method, float delay)
    {
        StartCoroutine(InvokeCoroutine(method, delay));
    }

    private IEnumerator InvokeCoroutine(Action method, float delay)
    {
        yield return new WaitForSeconds(delay);
        method?.Invoke();
    }

    public static GameObject InstantiatePrefab(GameObject prefab, Transform parent)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab�� null�Դϴ�.");
            return null;
        }

        return Instantiate(prefab, parent);
    }
}