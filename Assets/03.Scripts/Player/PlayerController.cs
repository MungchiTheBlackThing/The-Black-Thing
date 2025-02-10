using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using UnityEngine.Android;
using Assets.Script.Reward;

public enum LANGUAGE 
{ 
    KOREAN = 0,
    ENGLISH
}

public class PlayerController : MonoBehaviour, IPlayerInterface
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
    private int currentChapter;
    const float passTime = 1800f; //30���� �������� �Ѵ�.
    // Start is called before the first frame update

    public delegate void NextPhaseDelegate(GamePatternState state);
    public NextPhaseDelegate nextPhaseDelegate;

    /*������ ������ ������ �� idx�� �� ���� ����ϸ� �ȴ�.*/
    [SerializeField]
    [Tooltip("�ڷΰ��� ������ ���� ����")]
    Stack<int> gobackPage;

    [SerializeField]
    [Tooltip("���� �Ŵ���")]
    TranslateManager translateManager;
    [SerializeField]
    GameObject gamemanger;

    public delegate void SuccessSubDialDelegate(int phase, string subTitle);
    public SuccessSubDialDelegate successSubDialDelegate;

    public string currentReward = "";


    [SerializeField]
    SubDialogue subDialogue;
    private void Awake()
    {
        //������ player�� �������� �����ؼ� ������ ����.. ������ �̸� �ʱ�ȭ�ؼ� ����Ѵ�.
        gobackPage = new Stack<int>();
        player = new PlayerInfo(nickname, 1, GamePatternState.Watching);
        readStringFromPlayerFile();
    }
    private void Start()
    {
        translateManager = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();
        translateManager.Translate(GetLanguage());
        
        successSubDialDelegate += SuccessSubDial;
        currentChapter = GetChapter();

    }
    // Update is called once per frame
    //1�ð��� �Ǿ����� üũ�ϱ� ���ؼ� �����뵵
    void Update()
    {
        elapsedTime += Time.deltaTime;

    }
   

    public void ProgressSubDial(string title)
    {
        currentReward = title;
    }

    void SuccessSubDial(int phase, string subTitle)
    {
        Debug.Log("SuccessSubDial" + subTitle);
        string reward = "reward" + subTitle.Substring(subTitle.IndexOf('_'));

        EReward eReward;

        if (gamemanger.GetComponent<Alertmanager>() != null)    
            gamemanger.GetComponent<Alertmanager>().Alerton();
        //�迭 ������ �ִ´�.
        if (Enum.TryParse<EReward>(reward, true, out eReward))
        {
            //�÷��̾� ��Ʈ�ѷ��� � ������ �޾Ҵ��� ����Ʈ �߰�.
            AddReward(eReward);
        }
        SetSubPhase(subDialogue.subseq - 2);
    }
    public void NextPhase()
    {
        int phase = GetAlreadyEndedPhase();

        phase += 1;

        if (phase > (int)GamePatternState.NextChapter)
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

    public void SetSubPhase(int phaseIdx)
    {
        if (phaseIdx < 0 || phaseIdx >= 4) return;
        Debug.Log("������ �ε���" + phaseIdx);
        Debug.Log("�̰� üũ: " + GetChapter() * 4 + phaseIdx);
        if (gamemanger.GetComponent<Alertmanager>() != null)
        {
            gamemanger.GetComponent<Alertmanager>().isAlert = true;
            gamemanger.GetComponent<Alertmanager>().RewardAlerts[phaseIdx].SetActive(true);
        }
            
        player.SetSubPhase(phaseIdx);
    }

    public List<bool> GetSubPhase(int Chapter)
    {
        if (Chapter <= 0 || Chapter > 15) return null;

        return player.GetSubPhase(Chapter - 1);
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

        translateManager.Translate(player.language);
    }

    public void SetLanguage(string language)
    {
        LANGUAGE lang;
        if (Enum.TryParse(language, true, out lang))
        {
            SetLanguage(lang);
        }
    }

    public void SetMoonRadioIdx(int Idx)
    {
        player.MoonRadioIdx = Idx;
    }

    public int GetMoonRadioIdx()
    {
        return player.MoonRadioIdx;
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
    public bool GetIsDiaryCheck()
    {
        return player.IsDiaryCheck;
    }

    public void SetIsUpdatedDiary(bool isCheck)
    {
        player.isUpdatedDiary = isCheck;
    }
    public bool GetIsUpdatedDiary()
    {
        return player.isUpdatedDiary;
    }

    public int GetChapter()
    {
        return player.CurrentChapter;
    }

    public void AddReward(EReward InRewardName)
    {
        player.rewardList.Add(InRewardName);
    }

    public List<EReward> GetRewards()
    {
        return player.rewardList;
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
        //���� player nextchapter���, ����
        player.currentPhase = player.currentPhase == GamePatternState.NextChapter ? GamePatternState.Watching : player.currentPhase;
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

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // ���ø����̼��� ��׶���� ��ȯ�� �� ������ �ڵ�
            WritePlayerFile();
        }
    }

    private void OnApplicationQuit()
    {
        WritePlayerFile();
    }

    private void OnDestroy()
    {
        WritePlayerFile();
    }
}
