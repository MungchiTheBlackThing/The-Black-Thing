using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class ErrorTestFmod : MonoBehaviour
{
    void Start()
    {
        RuntimeManager.StudioSystem.getBankList(out Bank[] banks);
        Debug.Log($"Banks: {banks?.Length ?? 0}");
        if (banks == null) return;

        foreach (var b in banks)
        {
            b.getPath(out string p);
            b.getLoadingState(out LOADING_STATE s);
            b.getStringCount(out int sc);
            b.getEventCount(out int ec);
            Debug.Log($"Bank '{p}' state={s}, strings={sc}, events={ec}");
        }
    }
}
