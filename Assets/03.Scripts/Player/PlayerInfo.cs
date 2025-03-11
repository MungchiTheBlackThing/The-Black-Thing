using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Script.Reward;

/*Sub 성공 챕터 유무에 따라 아키텍처 타입이 달라짐*/

[System.Serializable]
public struct ArcheType
{
    public int sun;//해와 달 중 구분
    public int moon; 
    public int active; //적극과 소극으로 구분
    public int passive;
    public int deathnote;
}

[System.Serializable]
public class PlayerInfo
{
    //player 닉네임
    public string nickname;
    //player 입장 시간 - 입장한 날짜까지 전부 들고 있는다. 
    public DateTime datetime;
    //현재 진행 중인 챕터
    public int chapter;
    //현재 본 MoonRadioIdx
    public int moonRadioIdx;
    //브금 효과 크기
    public float bgmVolume;
    //사운드 이펙트 효과 크기
    public float sfxVolume;
    //다이어리 체크 유무(매일 1개씩 뜬다.)
    public bool isDiaryCheck;
    //다이어리가 업데이트 되어있는가
    public bool isUpdatedDiary;
    //푸시 알림 유무
    public bool isPushNotificationEnabled;
    //현재 플레이어 언어
    public LANGUAGE language;
    //서브 성공유무를 체크하기 위한 부울 변수, 4*14일차, 즉 56개 나올 예정
    public List<bool> subSuccessOrNot;
    //유서 결과를 체크하기 위한 아키텍처 타입
    public ArcheType archeType;
    //현재 진행 pattern 상태
    public GamePatternState currentPhase;
    public List<EReward> rewardList; //플레이어가 가지고 있는 리워드

    public PlayerInfo()
    {
        nickname = "default";
        chapter = 1;
        Init();
    }

    public PlayerInfo(string nickname, int chapter, GamePatternState initPhase)
    {
        this.nickname = nickname;
        this.chapter = chapter;
        this.currentPhase = initPhase;
        Init();
    }

    void Init()
    {
        datetime = DateTime.Now;
        bgmVolume = 0.5f;
        sfxVolume = 0.5f;
        isDiaryCheck = false;
        isUpdatedDiary = false;
        language = LANGUAGE.KOREAN;
        isPushNotificationEnabled = true;
        currentPhase = GamePatternState.Watching;
        moonRadioIdx = 1;

        if (subSuccessOrNot == null)
        {
            subSuccessOrNot = new List<bool>();

            for (int i = 1; i <= 14 * 4; i++)
            {
                subSuccessOrNot.Add(false);
            }
        }

        if (rewardList == null)
        {
            rewardList = new List<EReward>();
        }
    }

    public void SetSubPhase(int phaseIdx)
    {
        subSuccessOrNot[(chapter - 1) * 4 + phaseIdx] = true;
    }

    public List<bool> GetSubPhase(int Chapter)
    {
        List<bool> subPhase = new List<bool>();

        //4개씩 끊어서 전달한다.
        for (int i = 0; i < 4; i++)
        {
            //0 1 2 3
            //chapter 1, 4 5 6 7
            //2 8 9 10 11
            subPhase.Add(subSuccessOrNot[Chapter * 4 + i]);
        }

        return subPhase;
    }
    

    public bool IsDiaryCheck { get => isDiaryCheck; set=>isDiaryCheck = value;}
    public float BgmVolume{ get=>bgmVolume; set=>bgmVolume = value; }
    public float AcousticVolume { get=>sfxVolume; set=>sfxVolume=value; }
    public int CurrentChapter { get => chapter; set => chapter = value; }
    public DateTime Datetime { get => datetime; set => datetime = value; }
    public string Nickname { get => nickname; set => nickname = value; }
    public int MoonRadioIdx { get=> moonRadioIdx; set => moonRadioIdx = value;}
}
