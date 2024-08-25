using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;

public enum LANGUAGE 
{ 
    KOREAN = 0,
    ENGLISH
}

public class PlayerController : MonoBehaviour
{
    const string playerInfoDataFileName = "PlayerData.json";
    //���� �÷��̾�
    private PlayerInfo player;
    //player ���� ��� �ð�
    float elapsedTime;
    //�ӽ� ������ ���� serialize..
    [SerializeField]
    string nickname;
    [SerializeField]
    int currentChapter;
    public bool isDiaryCheck = false;
    bool isNextChapter = false;
    const float passTime = 1800f; //30���� �������� �Ѵ�.
    // Start is called before the first frame update

    [SerializeField]
    bool isEng;

    public delegate void NextPhaseDelegate(GamePatternState state);
    public NextPhaseDelegate nextPhaseDelegate;

    /*������ ������ ������ �� idx�� �� ���� ����ϸ� �ȴ�.*/
    [SerializeField]
    [Tooltip("�ڷΰ��� ������ ���� ����")]
    Stack<int> gobackPage;
    private void Awake()
    {
        //������ player�� �������� �����ؼ� ������ ����.. ������ �̸� �ʱ�ȭ�ؼ� ����Ѵ�.
        gobackPage = new Stack<int>();
        player = new PlayerInfo(nickname, 1, GamePatternState.Watching);
        readStringFromPlayerFile();
    }
    private void Start()
    {
        nextPhaseDelegate(player.currentPhase);
    }
    // Update is called once per frame
    //1�ð��� �Ǿ����� üũ�ϱ� ���ؼ� �����뵵
    void Update()
    {
        elapsedTime += Time.deltaTime;


        //�Ʒ� ���� ����, �׽�Ʈ ����
        if(isEng)
        {
            player.language = LANGUAGE.ENGLISH;
            isEng = false;
        }
    }
    public void NextPhase()
    {
        int phase = GetAlreadyEndedPhase();

        phase += 1;

        if((GamePatternState)phase > GamePatternState.NextChapter)
        {
            player.currentPhase = GamePatternState.Watching;
            //é�Ͱ� ������
            SetChapter();
        }
        else
        {
            player.currentPhase = (GamePatternState)phase;
        }

        nextPhaseDelegate(player.currentPhase);
    }

    public float GetTime()
    {
        return elapsedTime;
    }

    public bool GetisPushNotificationEnabled()
    {
        return player.isPushNotificationEnabled;
    }

    public void SetisPushNotificationEnabled(bool isPushNotificationEnabled)
    {
        player.isPushNotificationEnabled = isPushNotificationEnabled;
    }

    public void SetBGMVolume(float value)
    {
        player.bgmVolume = value;
    }

    public void SetSEVolume(float value)
    {
        player.sfxVolume = value;
    }

    public void SetChapter()
    {
        player.CurrentChapter += 1;
        currentChapter = player.CurrentChapter;
    }
    public void SetLanguage(LANGUAGE language)
    {
        player.language = language;
    }

    public void SetLanguage(string language)
    {
        LANGUAGE lang;
        if(Enum.TryParse(language,true,out lang))
        {
            player.language = lang;
        }
    }
    public LANGUAGE GetLanguage()
    {
        return player.language;
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
        elapsedTime += (passTime * 2); //1�ð� Update
    }
    public void PassWriting()
    {
        elapsedTime += (passTime);
    }
    public void PassThinkingTime()
    {
        elapsedTime += (passTime * 4); //2�ð� 1800*4 => 7200
    }
    public void EntryGame(DateTime dateTime)
    {
        if (player != null)
        {
            player.Datetime = dateTime;
        }
    }

    public int GetAlreadyEndedPhase()
    {
        return (int)player.currentPhase;
    }

    public void SetIsDiaryCheck(bool isCheck)
    {
        player.isDiaryCheck = isCheck;
    }

    public int GetChapter()
    {
        return player.CurrentChapter;
    }

    public string GetNickName()
    {
        return player.Nickname;
    }
    public void SetNickName(string InName)
    {
        player.Nickname = InName;
    }
    public float GetAcousticVolume()
    {
        return player.AcousticVolume;
    }
    public float GetMusicVolume()
    {
        return player.BgmVolume;
    }

    public void WritePlayerFile()
    {
        //PlayerInfo Ŭ���� ���� �÷��̾� ������ Json ���·� ������ �� ���ڿ� ����
        string jsonData = JsonUtility.ToJson(player);
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

            if (player != null)
            {
                player = JsonUtility.FromJson<PlayerInfo>(json);
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

    private void OnDestroy()
    {
        WritePlayerFile();
    }
}
