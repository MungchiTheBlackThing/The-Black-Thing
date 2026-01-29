using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Assets.Script.TimeEnum;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private float bgmFadeSeconds = 0.5f;   // BGM 바뀔 때 크로스페이드
    [SerializeField] private float muteFadeSeconds = 0.2f;  // 뮤트/해제 페이드
    private Coroutine bgmFadeCo;
    private Coroutine bgmVolCo;
    private EventReference currentBGM;
    private EventInstance bgmInstance; // ���� BGM �ν��Ͻ�
    private EventInstance ambInstance;
    float sfxVolume = 1f;
    float bgmVolume = 1f;
    const string EVENT_PATH = "event:/AMB/AMB_Room";
    private Stack<EventReference> _bgmOverrideStack = new Stack<EventReference>();

    private bool _bgmMuted = false;
    private float _bgmVolumeBeforeMute = 1f;
    public bool IsBGMMuted => _bgmMuted;


    /// <summary>
    /// ȿ���� ���
    /// </summary>
    public void PlayOneShot(EventReference sound, Vector3 worldPosition)
    {
        RuntimeManager.PlayOneShot(sound, worldPosition, sfxVolume);
    }
        public void ToggleBGMMute()
    {
        SetBGMMute(!_bgmMuted);
    }

    public void SetBGMMute(bool mute)
    {
        _bgmMuted = mute;

        if (!bgmInstance.isValid()) return;

        float target = _bgmMuted ? 0f : bgmVolume;
        StartBGMVolumeFade(target, muteFadeSeconds);
    }

    private void StartBGMVolumeFade(float target, float sec)
    {
        if (bgmVolCo != null) StopCoroutine(bgmVolCo);
        bgmVolCo = StartCoroutine(FadeBGMVolumeCo(target, sec));
    }

    private IEnumerator FadeBGMVolumeCo(float target, float sec)
    {
        if (!bgmInstance.isValid()) yield break;

        bgmInstance.getVolume(out float startVol, out _);

        float t = 0f;
        while (t < sec)
        {
            t += Time.unscaledDeltaTime;
            float a = (sec <= 0f) ? 1f : Mathf.Clamp01(t / sec);

            if (!bgmInstance.isValid()) yield break; // 중간에 BGM 교체되면 안전 종료
            bgmInstance.setVolume(Mathf.Lerp(startVol, target, a));

            yield return null;
        }

        if (bgmInstance.isValid()) bgmInstance.setVolume(target);
        bgmVolCo = null;
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
        bgmInstance.setVolume(_bgmMuted ? 0f : bgmVolume);
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

        CrossfadeBGM(newBGM, bgmFadeSeconds);
        currentBGM = newBGM;
    }

    private void CrossfadeBGM(EventReference next, float sec)
    {
        if (bgmFadeCo != null) StopCoroutine(bgmFadeCo);
        bgmFadeCo = StartCoroutine(CrossfadeCo(next, sec));
    }

    private IEnumerator CrossfadeCo(EventReference next, float sec)
    {
        EventInstance old = bgmInstance;
        bool hasOld = old.isValid();

        // 새 인스턴스 준비(볼륨 0에서 시작)
        EventInstance nextInst = RuntimeManager.CreateInstance(next);
        nextInst.setVolume(0f);
        nextInst.start();

        float oldStartVol = 0f;
        if (hasOld) old.getVolume(out oldStartVol, out _);

        float t = 0f;
        while (t < sec)
        {
            t += Time.unscaledDeltaTime;
            float a = (sec <= 0f) ? 1f : Mathf.Clamp01(t / sec);

            float targetVol = _bgmMuted ? 0f : bgmVolume;

            if (hasOld) old.setVolume(Mathf.Lerp(oldStartVol, 0f, a));
            nextInst.setVolume(Mathf.Lerp(0f, targetVol, a));

            yield return null;
        }

        float finalTarget = _bgmMuted ? 0f : bgmVolume;
        nextInst.setVolume(finalTarget);

        if (hasOld)
        {
            old.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            old.release();
            old.clearHandle();
        }

        bgmInstance = nextInst;
        bgmFadeCo = null;
    }


    public void PushBGMOverride(EventReference bgmEvent)
    {
        if (bgmEvent.Guid == System.Guid.Empty) return;

        _bgmOverrideStack.Push(bgmEvent);
        ChangeBGM(bgmEvent);
    }

    public void PopBGMOverride()
    {
        if (_bgmOverrideStack.Count > 0)
            _bgmOverrideStack.Pop();

        if (_bgmOverrideStack.Count > 0)
        {
            ChangeBGM(_bgmOverrideStack.Peek());
            return;
        }

        // 메인으로 복귀
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
            UpdateBGMByChapter(gm.Chapter, gm.Pattern);
    }


    public void UpdateBGMByChapter(int chapter, GamePatternState phase)
    {
        if (_bgmOverrideStack.Count > 0) return;
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
        bgmVolume = f;
        if (bgmInstance.isValid())
            bgmInstance.setVolume(_bgmMuted ? 0f : bgmVolume);
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