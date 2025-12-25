using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Assets.Script.TimeEnum;

public class AudioManager : Singleton<AudioManager>
{
    private EventReference currentBGM;
    private EventInstance bgmInstance; // ���� BGM �ν��Ͻ�
    private EventInstance ambInstance;
    float sfxVolume = 1f;
    float bgmVolume = 1f;
    const string EVENT_PATH = "event:/AMB/AMB_Room";

    /// <summary>
    /// ȿ���� ���
    /// </summary>
    public void PlayOneShot(EventReference sound, Vector3 worldPosition)
    {
        RuntimeManager.PlayOneShot(sound, worldPosition, sfxVolume);
    }

    /// <summary>
    /// BGM ����
    /// </summary>
    public void PlayBGM(EventReference bgmEvent)
    {
        if (bgmInstance.isValid())
        {
            bgmInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            bgmInstance.release();
        }

        bgmInstance = RuntimeManager.CreateInstance(bgmEvent);
        bgmInstance.setVolume(bgmVolume);
        bgmInstance.start();
    }

    /// <summary>
    /// BGM ����
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
    /// BGM ��ü (���� BGM�� ���̵�ƿ��ϰ� ���ο� BGM ���)
    /// </summary>
    public void ChangeBGM(EventReference newBGM)
    {
        if (currentBGM.Equals(newBGM) && bgmInstance.isValid())
            return;

        StopBGM(true);
        PlayBGM(newBGM);
        currentBGM = newBGM;
    }

    public void UpdateBGMByChapter(int chapter, GamePatternState phase)
    {
        EventReference bgmToPlay = default;

        if (chapter == 12 && IsThinkingOrLater(phase))
        {
            bgmToPlay = FMODEvents.Instance.bgmmain_3_ver2;
        }
        else
        {
            switch (chapter)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    bgmToPlay = FMODEvents.Instance.bgmmain_1;
                    break;

                case 5:
                case 6:
                case 7:
                case 8:
                    bgmToPlay = FMODEvents.Instance.bgmmain_2;
                    break;

                case 9:
                case 10:
                case 11:
                case 12:
                    bgmToPlay = FMODEvents.Instance.bgmmain_3;
                    break;

                case 13: 
                    bgmToPlay = FMODEvents.Instance.bgmmain_3_ver2;
                    break;

                case 14:
                case 15:
                    bgmToPlay = FMODEvents.Instance.bgmmain_4;
                    break;

                default:
                    Debug.LogWarning($"Chapter {chapter}�� �´� BGM�� �����Ǿ� ���� �ʽ��ϴ�.");
                    return;
            }
        }

        AudioManager.Instance.ChangeBGM(bgmToPlay);
    }

    public void EnsureAMB(EventReference ambEvent, string label = null)
    {

        if (!ambInstance.isValid())
        {
            ambInstance = RuntimeManager.CreateInstance(ambEvent);
            ambInstance.start(); // �׻� ���� ����
        }

        if (!string.IsNullOrEmpty(label))
        {
            // �Ķ���� �̸�: AMB_Room
            ambInstance.setParameterByNameWithLabel("AMB_Room", label);
        }
    }

    public void SetSFXVolume(float f)
    {
        sfxVolume = f;
    }

    public void SetBGMVolume(float f)
    {
        bgmInstance.setVolume(f);
        bgmVolume = f;
    }

    private bool IsThinkingOrLater(GamePatternState phase)
    {
        return phase == GamePatternState.Thinking
            || phase == GamePatternState.MainB
            || phase == GamePatternState.Writing
            || phase == GamePatternState.Play            
            || phase == GamePatternState.Sleeping
            || phase == GamePatternState.NextChapter;
    }

}