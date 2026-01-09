using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using UnityEngine.Android;
using Assets.Script.Reward;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

[Serializable]
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
    private GameManager gm;
    float elapsedTime;
    //임시 저장을 위한 serialize..
    [SerializeField]
    string nickname;
    [SerializeField]
    private int currentChapter;
    const float passTime = 1800f; //30분을 기준으로 한다.

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
    [SerializeField]
    GameObject RewardPopup;
    public delegate void SuccessSubDialDelegate(int phase, string subTitle);
    public SuccessSubDialDelegate successSubDialDelegate;
    [SerializeField] ObjectManager objectManager;
    public string currentReward = "";

    [SerializeField]
    SubDialogue subDialogue;

    private void Awake()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();

        //앞으로 player을 동적으로 생성해서 관리할 예정.. 아직은 미리 초기화해서 사용한다.
        Debug.Log("시작");
        gobackPage = new Stack<int>();
        player = new PlayerInfo(nickname, 1, GamePatternState.Watching);
        readStringFromPlayerFile();
        
        // 파일에서 읽은 언어와 PlayerPrefs 동기화
        if (PlayerPrefs.HasKey("Locale"))
        {
            string savedLocaleCode = PlayerPrefs.GetString("Locale");
            LANGUAGE savedLanguage = savedLocaleCode.StartsWith("ko", StringComparison.OrdinalIgnoreCase) 
                ? LANGUAGE.KOREAN 
                : LANGUAGE.ENGLISH;
            
            if (player.language != savedLanguage)
            {
                player.language = savedLanguage;
                WritePlayerFile(); 
            }
        }
        
        translateManager = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();
        Debug.Log("번역시작");
        translateManager.Translate(GetLanguage());
        successSubDialDelegate += SuccessSubDial;
        currentChapter = GetChapter();
        currentChapter = player.CurrentChapter;
        nickname = player.Nickname;
        AudioManager.Instance.UpdateBGMByChapter(gm.Chapter, gm.Pattern);
        AudioManager.Instance.SetBGMVolume(player.BgmVolume);
        AudioManager.Instance.SetSFXVolume(player.AcousticVolume);
        ChangeLocalizationLocale(GetLanguage());
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
        Debug.Log($"[SuccessSubDial] phase: {phase}, subTitle: {subTitle}, 현재 subseq: {GetSubseq()}");
        string reward = "reward" + subTitle.Substring(subTitle.IndexOf('_'));

        EReward eReward;

        int currentSubseq = GetSubseq();
        
        if (gamemanger.GetComponent<Alertmanager>() != null)
            gamemanger.GetComponent<Alertmanager>().Alerton();
        //배열 변수에 넣는다.
        if (Enum.TryParse<EReward>(reward, true, out eReward))
        {
            //플레이어 컨트롤러에 어떤 보상을 받았는지 리스트 추가.
            AddReward(eReward);
            Debug.Log($"[SuccessSubDial] Reward 추가: {eReward}");
            RewardPopup.SetActive(true);
            objectManager.RewardGlow(eReward);
        }
        
        Debug.Log($"[SuccessSubDial] SetSubPhase 호출: subseq={currentSubseq}, phaseIdx={currentSubseq - 1}");
        SetSubPhase(currentSubseq - 1);
    }
    public void NextPhase()
    {
        Debug.Log($"[NextPhase CALLER]\n{Environment.StackTrace}");
        if (GameManager.isend) return;
        foreach (var door in FindObjectsOfType<DoorController>()) //페이즈 넘어갈 때 (메인 끝나면 페이즈 넘어가니까 + 나머지 페이즈 전환은 상관 없으니까) 문 켜기
        {
            door.SetDoorForDialogue(true);
        }
        Debug.Log("NextPhase");
        if (gamemanger.GetComponent<GameManager>())
            gamemanger.GetComponent<GameManager>().StopSubDial();

        int phase = GetCurrentPhase();
        if (phase == (int)GamePatternState.MainB && player.chapter == 14)
        {
            Debug.Log("Ending");
            if (GameManager.isend) return;   // 이미 엔딩이면 중복 실행 방지
            gamemanger.GetComponent<GameManager>().Ending();
            return;
        }

        phase += 1;

        if (phase > (int)GamePatternState.NextChapter)
        {
            Debug.Log("다음 챕터");
            player.currentPhase = GamePatternState.Watching;
            SetChapter();
        }
        else
        {
            player.currentPhase = (GamePatternState)phase;

            if (player.currentPhase == GamePatternState.NextChapter)
            {
                Debug.Log("NextChapter 상태 진입 → ChangeGameState 호출");
                gamemanger.GetComponent<GameManager>().ChangeGameState(GamePatternState.NextChapter);
                return; // 더 이상 아래 로직 실행하지 않음
            }
        }
        WritePlayerFile(); //페이즈 변경 사항 저장

        // 순서: 먼저 currentPhase 적용 → SetPhase로 subseq 설정 → delegate 호출
        gamemanger.GetComponent<GameManager>().SetPhase(player.currentPhase);

        Debug.Log("[Test] nextPhaseDelegate 호출 직전");
        nextPhaseDelegate?.Invoke(player.currentPhase);
    }


    public void SetSubPhase(int phaseIdx)
    {
        if (phaseIdx < 0 || phaseIdx >= 4) return;
        Debug.Log($"[SetSubPhase] phaseIdx: {phaseIdx}, Chapter: {GetChapter()}, 계산된 인덱스: {(GetChapter() - 1) * 4 + phaseIdx}");
        if (gamemanger.GetComponent<Alertmanager>() != null)
        {
            gamemanger.GetComponent<Alertmanager>().isAlert = true;
            gamemanger.GetComponent<Alertmanager>().RewardAlerts[phaseIdx].SetActive(true);
        }

        player.SetSubPhase(phaseIdx);
        WritePlayerFile(); // 저장
        Debug.Log($"[SetSubPhase] 저장 완료, GetSubPhase 결과: {string.Join(", ", player.GetSubPhase(GetChapter()))}");
    }

    public List<bool> GetSubPhase(int Chapter)
    {
        if (Chapter <= 0 || Chapter > 15) return null;

        return player.GetSubPhase(Chapter);
    }


    public float GetTime()
    {
        return elapsedTime;
    }

    public PlayerInfo GetPlayerInfo()
    {
        return player;
    }
    public void SavePlayerInfo()
    {
        WritePlayerFile();
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
        Debug.Log("CurrentChapter: " + currentChapter);
        LoadSceneManager.Instance.LoadChapterImage(currentChapter); 
        player.subseq = 1;
        ClearWatchedSubseq(); // 봤던 서브 리스트도 초기화
        AudioManager.Instance.UpdateBGMByChapter(gm.Chapter, gm.Pattern);
        WritePlayerFile();
    }
    public void SetLanguage(LANGUAGE language)
    {
        Debug.Log("현재 언어: " + language);
        player.language = language;

        translateManager.Translate(player.language);
        ChangeLocalizationLocale(language);
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

    public void SetSubseq(int idx)
    {
        player.subseq = idx;
        WritePlayerFile();
    }
    public void plusSubseq()
    {
        player.subseq += 1;
        WritePlayerFile();
    }
    public int GetSubseq()
    {
        return player.subseq;
    }

    public List<int> GetWatchedList()
    {
        return player.watchedSubseq;
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

    public int GetCurrentPhase()
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
        return this.currentChapter;
    }

    public ArcheType GetArcheType()
    {
        return player.archeType;
    }
    public void AddReward(EReward InRewardName)
    {
        // 이미 리스트에 해당 리워드가 없다면 추가
        if (!player.rewardList.Contains(InRewardName))
        {
            player.rewardList.Add(InRewardName);
        }
    }

    public List<EReward> GetRewards()
    {
        Debug.Log("리워드 목록: " + player.rewardList);
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
    public void UpdateArcheType(string tag)
    {
        Debug.Log("플러스 태그 : " + tag);
        if (tag == "sun")
        {
            player.archeType.sun++;
        }
        else if (tag == "moon")
        {
            player.archeType.moon++;
        }
        else if (tag == "active")
        {
            player.archeType.active++;
        }
        else if (tag == "passive")
        {
            player.archeType.passive++;
        }
    }
    public void DownArcheType(string tag)
    {
        Debug.Log("마이너스 태그 : " + tag);
        if (tag == "sun")
        {
            player.archeType.sun--;
        }
        else if (tag == "moon")
        {
            player.archeType.moon--;
        }
        else if (tag == "active")
        {
            player.archeType.active--;
        }
        else if (tag == "passive")
        {
            player.archeType.passive--;
        }
    }
    public void checkdeath(int index)
    {
        player.archeType.deathnote = index;
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

    public ArcheType GetSunMoon()
    {
        return player.archeType;
    }

    public void Replay()
    {
        PlayerPrefs.SetInt("FORCE_NEW_GAME", 1);
        PlayerPrefs.Save();
        LoadSceneManager.Instance.LoadScene("MainScene", "IntroScene", 0);
    }

    public bool IsSubWatched(int id)
    {
        return player.watchedSubseq.Contains(id);
    }

    public void MarkSubWatched(int id)
    {
        if (!player.watchedSubseq.Contains(id))
            player.watchedSubseq.Add(id);
        WritePlayerFile();
    }
    public void ClearWatchedSubseq()
    {
        player.watchedSubseq.Clear();
        WritePlayerFile();
    }

    /// 1일차 다이어리 잠금 해제
    public void UnlockDiaryForChapter1()
    {
        player.diaryUnlockedInChapter1 = true;
        WritePlayerFile();
        
    }

    public bool IsDiaryUnlockedForChapter1()
    {
        return player.diaryUnlockedInChapter1;
    }

    private void ChangeLocalizationLocale(LANGUAGE language)
    {
        StartCoroutine(SetLocaleCoroutine(language));
    }

    private IEnumerator SetLocaleCoroutine(LANGUAGE language)
    {
        yield return LocalizationSettings.InitializationOperation;
        
        // LANGUAGE enum을 locale 코드로 매핑
        string code = language == LANGUAGE.KOREAN ? "ko-KR" : "en-US";

        var locale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(code));
        //실패하면 기본 코드로 폴백
        if (locale == null)
        {
            string baseCode = language == LANGUAGE.KOREAN ? "ko" : "en";
            locale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(baseCode));
        }

        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
            // PlayerPrefs에도 저장하여 LocalizationBoot와 동기화
            PlayerPrefs.SetString("Locale", locale.Identifier.Code);
            PlayerPrefs.Save();
            Debug.Log($"[PlayerController] Locale Changed. Language: {language}, Target Code: {code}, Selected Locale: {locale.Identifier.Code}");
        }
        else
        {
            Debug.LogWarning($"[PlayerController] Failed to find locale for code: {code}");
        }
    }
}