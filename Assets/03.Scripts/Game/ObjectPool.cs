using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ObjectPool
{
    //�޸� Ȱ��ȭ ��Ȱ��ȭ�� �ǹ��ϴ� �ǹ��ϴ� Ŭ���� ����
    public class PoolItem
    {
        public PoolItem() { }
        public PoolItem(bool isActive, GameObject gameObject)
        {
            _isActive = isActive;
            _gameObject = gameObject;
        }
        public bool _isActive;
        public GameObject _gameObject;
    }

    private Dictionary<string, PoolItem> _memory = new Dictionary<string, PoolItem>();

    public IEnumerator LoadFromMemoryAsync(string path, System.Action<AssetBundle> callback)
    {

        UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(path);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to download AssetBundle: {www.error}");
        }
        else
        {
            // AssetBundle�� �ٿ�ε��� �� ��������
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);

            // �ݹ� ȣ��
            if (bundle != null)
            {
                Debug.Log("AssetBundle successfully downloaded and loaded!");
                callback?.Invoke(bundle);
            }
            else
            {
                Debug.LogError("Failed to load AssetBundle.");
                callback?.Invoke(null);
            }
        }
    }
    
    //�˻� ���
    public GameObject SearchMemory(string objectName)
    {
        if (_memory.ContainsKey(objectName))
        {
            return _memory[objectName]._gameObject;
        }

        return null; //���� 
    }

    public Dictionary<string, PoolItem> GetMemory()
    {
        return _memory;
    }

    public List<GameObject> GetValues()
    {
        List<GameObject> values = new List<GameObject>();

        foreach (PoolItem value in _memory.Values)
        {
            values.Add(value._gameObject);
        }

        return values;
    }
    public bool InsertMemory(GameObject gameObject)
    {
        if (_memory.ContainsKey(gameObject.name))
        {
            return false; //���빰�� �־ ����
        }

        _memory.Add(gameObject.name, new PoolItem(gameObject.activeSelf, gameObject));
        return true;
    }

    public void SetActiveObject(string objectName)
    {
        if (_memory.ContainsKey(objectName) && _memory[objectName]._isActive == false)
        {
            _memory[objectName]._gameObject.SetActive(true);
            _memory[objectName]._isActive = true;
        }
    }
    public void DeactivateObject(string objectName)
    {
        if (_memory.ContainsKey(objectName) && _memory[objectName]._isActive == true)
        {
            _memory[objectName]._isActive = false;
            _memory[objectName]._gameObject.SetActive(_memory[objectName]._isActive);
        }
    }
    //���� ��� ��ųʸ� ���� �� ��
    public void DestroyObjects()
    {
        foreach (KeyValuePair<string, PoolItem> item in _memory)
        {
            GameObject.Destroy(item.Value._gameObject);
        }

        _memory.Clear();
    }
}
