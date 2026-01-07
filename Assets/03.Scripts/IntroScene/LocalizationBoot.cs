using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizationBoot : MonoBehaviour
{
    // PlayerPrefs 키(기존 코드와 동일하게 "Locale" 사용)
    const string PrefKey = "Locale";

    [SerializeField] bool dontDestroyOnLoad = true;

    void Awake()
    {
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
    }

    private static bool hasInitialized = false;

    IEnumerator Start()
    {
        // 이미 초기화되었으면 스킵
        if (hasInitialized)
        {
            yield break;
        }

        // Localization 시스템 초기화 대기
        var init = LocalizationSettings.InitializationOperation;
        if (!init.IsDone) yield return init;

        //저장된 언어가 있으면 그대로
        string targetCode = null;
        if (PlayerPrefs.HasKey(PrefKey))
            targetCode = PlayerPrefs.GetString(PrefKey);

        //저장된 값이 없으면 시스템 언어 추정
        if (string.IsNullOrWhiteSpace(targetCode))
            targetCode = GuessCodeFromSystemLanguage(Application.systemLanguage);

        //없으면 기본으로 적용
        Locale chosen = FindLocaleByCodeOrBase(targetCode);
        if (chosen == null)
            chosen = FallbackProjectLocale();

        if (chosen != null)
        {
            //적용
            LocalizationSettings.SelectedLocale = chosen;

            //저장
            PlayerPrefs.SetString(PrefKey, chosen.Identifier.Code);
            PlayerPrefs.Save();

            hasInitialized = true;
            Debug.Log($"[LocalizationBoot] 적용 언어: {chosen.Identifier.Code} ({chosen.LocaleName})");
        }
        else
        {
            Debug.LogWarning("[LocalizationBoot] 적용 언어 없음");
        }
    }

    // --- Helpers ---

    // 시스템 언어 추정
    static string GuessCodeFromSystemLanguage(SystemLanguage lang)
    {
        switch (lang)
        {
            case SystemLanguage.Korean: return "ko-KR";
            case SystemLanguage.English: return "en-US";
        }
        return null;
    }

    // 저장된 코드 or 시스템 추정 코드로부터 Locale 찾기
    static Locale FindLocaleByCodeOrBase(string code)
    {
        if (!IsValid(code))
            return null;

        // 1) 정확 일치 시도
        var exact = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(code));
        if (exact != null) return exact;

        // 2) "en-US" -> "en" 폴백
        int dash = code.IndexOf('-');
        if (dash > 0)
        {
            string baseCode = code.Substring(0, dash);
            var baseLocale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(baseCode));
            if (baseLocale != null) return baseLocale;
        }

        // 3) "en"만 들어온 경우, "en-*" 중 하나라도 매칭
        if (dash < 0)
        {
            foreach (var l in LocalizationSettings.AvailableLocales.Locales)
                if (l.Identifier.Code.StartsWith(code, StringComparison.OrdinalIgnoreCase))
                    return l;
        }

        return null;
    }

    //기본으로 설정
    static Locale FallbackProjectLocale()
    {
        if (LocalizationSettings.ProjectLocale != null)
            return LocalizationSettings.ProjectLocale;

        var list = LocalizationSettings.AvailableLocales;
        if (list != null && list.Locales.Count > 0)
            return list.Locales[0];

        return null;
    }
    //문자열 유효성 체크
    static bool IsValid(string s) => !string.IsNullOrWhiteSpace(s);
}

