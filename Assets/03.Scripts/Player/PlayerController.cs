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
    //실제 플레이어
    private PlayerInfo player;
    //player 접속 경과 시간
    float elapsedTime;
    //임시 저장을 위한 serialize..
    [SerializeField]
    string nickname;
    [SerializeField]
    private int currentChapter;
    const float passTime = 1800f; //30분을 기준으로 한다.
    // Start is called before the first frame update

    public delegate void NextPhaseDelegate(GamePatternState state);
    public NextPhaseDelegate nextPhaseDelegate;

    /*준현아 페이지 저장할 때 idx는 이 변수 사용하면 된다.*/
    [SerializeField]
    [Tooltip("뒤로가기 구현을 위한 스택")]
    Stack<int> gobackPage;

    [SerializeField]
    [Tooltip("번역 매니저")]
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
        //앞으로 player을 동적으로 생성해서 관리할 예정.. 아직은 미리 초기화해서 사용한다.
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
    //1시간이 되었는지 체크하기 위해서 저정용도
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
        //배열 변수에 넣는다.
        if (Enum.TryParse<EReward>(reward, true, out eReward))
        {
            //플레이어 컨트롤러에 어떤 보상을 받았는지 리스트 추가.
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
            //챕터가 증가함
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
        Debug.Log("페이즈 인덱스" + phaseIdx);
        Debug.Log("이거 체크: " + GetChapter() * 4 + phaseIdx);
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
    //시간 설정 : (현재 시간 - watching이 진행된 시간)+60분
    public void PassWathingTime()
    {
        //현재 진행시간에 60분을 더한다.
        //Time.deltaTime => 1초 
        //1분 => 60초
        //60분 => 60*60 => 3600초
        //30분 => 60*30 => 1800초
        //120분 => 60*120 => 7200초
        elapsedTime += (passTime * 2); //1시간 Update
    }
    public void PassWriting()
    {
        elapsedTime += (passTime);
    }
    public void PassThinkingTime()
    {
        elapsedTime += (passTime * 4); //2시간 1800*4 => 7200
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
        //PlayerInfo 클래스 내에 플레이어 정보를 Json 형태로 포멧팅 된 문자열 생성
        //만약 player nextchapter라면, 변경
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
            // 애플리케이션이 백그라운드로 전환될 때 실행할 코드
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
