using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Assets.Script.TimeEnum;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    private EventInstance bgmInstance; // ���� BGM �ν��Ͻ�
    private EventInstance ambInstance;
    const string EVENT_PATH = "event:/AMB/AMB_Room";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �ÿ��� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ȿ���� ���
    /// </summary>
    public void PlayOneShot(EventReference sound, Vector3 worldPosition)
    {
        RuntimeManager.PlayOneShot(sound, worldPosition);
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

            case 13: // ���� - 12 ���Ŀ� Ver2 ����
                bgmToPlay = FMODEvents.instance.bgmmain_3_ver2;
                break;

            case 14:
            case 15:
                bgmToPlay = FMODEvents.instance.bgmmain_4;
                break;

            default:
                Debug.LogWarning($"Chapter {chapter}�� �´� BGM�� �����Ǿ� ���� �ʽ��ϴ�.");
                return;
        }

        AudioManager.instance.ChangeBGM(bgmToPlay);
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
}