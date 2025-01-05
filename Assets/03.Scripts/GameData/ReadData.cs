using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

public class ReadData : MonoBehaviour
{
    // Start is called before the first frame update

    private void Awake()
    {
        StartCoroutine(LoadResourceCoroutine());
        DataManager.Instance.MoonRadioParser = new MoonRadioParser();
        DataManager.Instance.MoonRadioParser.LoadMoonRadio();
    }

    private IEnumerator LoadResourceCoroutine()
    {
        // ���ҽ��� �񵿱������� �ε�
        ResourceRequest resourceRequest = Resources.LoadAsync<TextAsset>("Json/Chapters");

        // �ε��� �Ϸ�� ������ ���
        yield return resourceRequest;

        // �ε��� �Ϸ�Ǹ� Asset�� ���� ����
        if (resourceRequest.asset != null)
        {
            TextAsset loadedTextAsset = resourceRequest.asset as TextAsset;
            DataManager.Instance.ChapterList = JsonUtility.FromJson<Chapters>(loadedTextAsset.ToString());
        }

        // ���ҽ��� �񵿱������� �ε�
        resourceRequest = Resources.LoadAsync<TextAsset>("Json/Setting");

        // �ε��� �Ϸ�� ������ ���
        yield return resourceRequest;

        // �ε��� �Ϸ�Ǹ� Asset�� ���� ����
        if (resourceRequest.asset != null)
        {
            TextAsset loadedTextAsset = resourceRequest.asset as TextAsset;
            DataManager.Instance.Settings = JsonUtility.FromJson<SettingInfo>(loadedTextAsset.ToString());
        }
        
        resourceRequest = Resources.LoadAsync<TextAsset>("Json/PoemsData");

        // �ε��� �Ϸ�� ������ ���
        yield return resourceRequest;

        // �ε��� �Ϸ�Ǹ� Asset�� ���� ����
        if (resourceRequest.asset != null)
        {
            TextAsset loadedTextAsset = resourceRequest.asset as TextAsset;
            DataManager.Instance.PoemData = JsonUtility.FromJson<Poems>(loadedTextAsset.ToString());
        }

        resourceRequest = Resources.LoadAsync<TextAsset>("Json/DiaryData");

        // �ε��� �Ϸ�� ������ ���
        yield return resourceRequest;

        // �ε��� �Ϸ�Ǹ� Asset�� ���� ����
        if (resourceRequest.asset != null)
        {
            TextAsset loadedTextAsset = resourceRequest.asset as TextAsset;
            DataManager.Instance.DiaryData = JsonUtility.FromJson<Diary>(loadedTextAsset.ToString());
        }
    }
}
