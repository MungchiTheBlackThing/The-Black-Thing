using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
public class PlayerController : MonoBehaviour
{

    const string playerInfoDataFileName = "PlayerData.json";
    public static PlayerInfo _player;
    //player ���� ��� �ð�
    float _elapsedTime;

    //�ӽ� ������ ���� serialize..
    [SerializeField]
    string nickname;
    [SerializeField]
    int currentChapter;


    public bool isDiaryCheck = false;
    bool isNextChapter = false;
    const float _passTime = 1800f; //30���� �������� �Ѵ�.
    // Start is called before the first frame update
    private void Awake()
    {
        //������ player�� �������� �����ؼ� ������ ����.. ������ �̸� �ʱ�ȭ�ؼ� ����Ѵ�.
        _player = new PlayerInfo(0, nickname, 1);
        //WritePlayerFile();
        readStringFromPlayerFile();
    }

    void Start()
    {
    }

    // Update is called once per frame
    //1�ð��� �Ǿ����� üũ�ϱ� ���ؼ� �����뵵
    void Update()
    {
        _elapsedTime += Time.deltaTime;
    }

    public void Init()
    {
        _player = new PlayerInfo(0, nickname, 1);
        WritePlayerFile();
    }

    public float GetTime()
    {
        return _elapsedTime;
    }

    public void SetChapter()
    {
        _player.CurrentChapter += 1;
        currentChapter = _player.CurrentChapter;
    }
    //�ð� ���� : (���� �ð� - watching�� ����� �ð�)+60��
    public void PassWathingTime()
    {
        //���� ����ð��� 60���� ���Ѵ�.
        //Time.deltaTime => 1�� 
        //1�� => 60��
        //60�� => 60*60 => 3600��
        //30�� => 60*30 => 1800��
        //120�� => 60*120 => 7200��
        _elapsedTime += (_passTime * 2); //1�ð� Update
    }
    public void PassWriting()
    {
        _elapsedTime += (_passTime);
    }
    public void PassThinkingTime()
    {
        _elapsedTime += (_passTime * 4); //2�ð� 1800*4 => 7200
    }
    public void EntryGame(DateTime dateTime)
    {
        if (_player != null)
        {
            _player.Datetime = dateTime;
        }
    }
    public int GetAlreadyEndedPhase()
    {
        return _player.AlreadyEndedPhase;
    }
    public void SetAlreadyEndedPhase()
    {

    }

    public void SetIsDiaryCheck(bool isCheck)
    {
        _player.isDiaryCheck = isCheck;
    }
    public int GetChapter()
    {
        return _player.CurrentChapter;
    }

    public string GetNickName()
    {
        return _player.Nickname;
    }
    public void SetNickName(string InName)
    {
        _player.Nickname = InName;
    }
    public float GetAcousticVolume()
    {
        return _player.AcousticVolume;
    }
    public float GetMusicVolume()
    {
        return _player.AcousticVolume;
    }

    public void WritePlayerFile()
    {
        //PlayerInfo Ŭ���� ���� �÷��̾� ������ Json ���·� ������ �� ���ڿ� ����
        string jsonData = JsonUtility.ToJson(_player);
        string path = pathForDocumentsFile(playerInfoDataFileName);
        File.WriteAllText(path, jsonData);
    }

    void readStringFromPlayerFile()
    {
        string path = pathForDocumentsFile(playerInfoDataFileName);

        if (File.Exists(path))
        {
            FileStream fileStream = new FileStream(path, FileMode.Open);
            byte[] data = new byte[fileStream.Length];
            fileStream.Read(data, 0, data.Length);
            fileStream.Close();
            string json = Encoding.UTF8.GetString(data);

            if (_player != null)
            {
                _player = JsonUtility.FromJson<PlayerInfo>(json);
            }
        }
        else
        {
            WritePlayerFile();
        }
    }
    string pathForDocumentsFile(string filename)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Path.Combine(path, "Documents"), filename);

        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            string path = Application.persistentDataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
        else
        {
            string path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Application.dataPath, filename);
        }

    }

    private void OnApplicationPause(bool pauseStatus)
    {
        WritePlayerFile();
    }
    void OnApplicationQuit()
    {
        WritePlayerFile();
    }
}
