using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    private EventInstance bgmInstance; // 현재 BGM 인스턴스

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    public void PlayOneShot(EventReference sound, Vector3 worldPosition)
    {
        RuntimeManager.PlayOneShot(sound, worldPosition);
    }

    /// <summary>
    /// BGM 시작
    /// </summary>
    public void PlayBGM(EventReference bgmEvent)
    {
        if (bgmInstance.isValid())
        {
            bgmInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            bgmInstance.release();
        }

        bgmInstance = RuntimeManager.CreateInstance(bgmEvent);
        bgmInstance.start();
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM(bool fadeOut = true)
    {
        if (bgmInstance.isValid())
        {
            bgmInstance.stop(fadeOut ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
            bgmInstance.release();
            bgmInstance.clearHandle();
        }
    }

    /// <summary>
    /// BGM 교체 (기존 BGM을 페이드아웃하고 새로운 BGM 재생)
    /// </summary>
    public void ChangeBGM(EventReference newBGM)
    {
        StopBGM(true);
        PlayBGM(newBGM);
    }

    public void UpdateBGMByChapter(int chapter)
    {
        EventReference bgmToPlay = default;

        switch (chapter)
        {
            case 1:
            case 2:
            case 3:
            case 4:
                bgmToPlay = FMODEvents.instance.bgmmain_1;
                break;

            case 5:
            case 6:
            case 7:
            case 8:
                bgmToPlay = FMODEvents.instance.bgmmain_2;
                break;

            case 9:
            case 10:
            case 11:
            case 12:
                bgmToPlay = FMODEvents.instance.bgmmain_3;
                break;

            case 13: // 예시 - 12 이후에 Ver2 적용
                bgmToPlay = FMODEvents.instance.bgmmain_3_ver2;
                break;

            case 14:
            case 15:
                bgmToPlay = FMODEvents.instance.bgmmain_4;
                break;

            default:
                Debug.LogWarning($"Chapter {chapter}에 맞는 BGM이 설정되어 있지 않습니다.");
                return;
        }

        AudioManager.instance.ChangeBGM(bgmToPlay);
    }
}