using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            Debug.LogError("No AudioMangager Instance");
        }
        instance = this;
    }

    public void PlayOneShot(EventReference sound, Vector3 World)
    {
        RuntimeManager.PlayOneShot(sound, World);
    }

}
