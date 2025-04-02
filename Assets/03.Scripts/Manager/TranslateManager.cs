using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TranslateManager : MonoBehaviour
{
    public delegate void TranslateLanaguageDelegate(LANGUAGE language, TMP_FontAsset font);

    public TranslateLanaguageDelegate translatorDel;

    [SerializeField]
    TMP_FontAsset[] fonts;
    public void Translate(LANGUAGE language)
    {
        TMP_FontAsset font = fonts[(int)language];
        translatorDel?.Invoke(language, font);
    }
}
