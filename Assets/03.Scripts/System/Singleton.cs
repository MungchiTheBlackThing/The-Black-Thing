using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // 씬에 이미 존재하는지 탐색
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    // 없으면 GameObject 생성
                    GameObject go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();
                }

                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        // 중복 생성 방지
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
