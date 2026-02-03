using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using System.Text;
using System;

public class LangPopup : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown myDropdown;          // 드롭다운
    public Button applyButton;               // 설정하기
    public Button cancelButton;              // 취소하기

    [Header("기존 로직")]
    [SerializeField] IntroScene intro;
    [SerializeField] PlayerInfo playerInfo;
    [SerializeField] MypageUIController mypage;

    [Header("Locale")]
    //0: 한국어, 1: 영어, 인스펙터에서 변경
    [SerializeField] string[] localeCodes = new string[] { "ko-KR", "en-US" };
    [SerializeField] string[] localeLabels = new string[] { "한국어", "English" };

    const string PrefKey = "Locale";

    //취소버튼 클릭 시 롤백용
    string preOpenLocaleCode = null;
    int preOpenDropdownIndex = -1;

    //루프 방지 
    bool suppressDropdownEvent = false;

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;

        //취소, 적용 버튼
        if (applyButton) applyButton.onClick.AddListener(OnClickApply);
        if (cancelButton) cancelButton.onClick.AddListener(OnClickCancel);

        // Localization 초기화 후 현재 선택 반영
        StartCoroutine(InitAndSyncDropdown());
    }

    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;

        if (applyButton) applyButton.onClick.RemoveListener(OnClickApply);
        if (cancelButton) cancelButton.onClick.RemoveListener(OnClickCancel);
        if (myDropdown) myDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }

    IEnumerator InitAndSyncDropdown()
    {
        // 패키지 초기화 대기
        var init = LocalizationSettings.InitializationOperation;
        if (!init.IsDone) yield return init;

        // PlayerPrefs에 저장된 값이 있으면 우선 적용
        if (PlayerPrefs.HasKey(PrefKey))
        {
            var saved = PlayerPrefs.GetString(PrefKey);
            var savedLocale = FindLocaleByCode(saved);
            if (savedLocale != null)
            {
                LocalizationSettings.SelectedLocale = savedLocale;
            }
        }

        //현재 locale을 드롭다운에 반영
        var current = LocalizationSettings.SelectedLocale;
        int idx = IndexOfLocale(current);
        if (idx < 0) { idx = 0; ApplyLocaleByIndex(idx, save: false); }

        suppressDropdownEvent = true;
        BuildDropdownOptions();
        myDropdown.SetValueWithoutNotify(idx);
        suppressDropdownEvent = false;

        //스냅샷 저장
        preOpenLocaleCode = LocalizationSettings.SelectedLocale.Identifier.Code;
        preOpenDropdownIndex = idx;

        //드롭다운 이벤트 구독
        myDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        myDropdown.onValueChanged.AddListener(OnDropdownValueChanged);

    }

    // 드롭다운 옵션 구성
    void BuildDropdownOptions()
    {
        if (myDropdown == null) return;

        myDropdown.ClearOptions();
        var opts = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < localeCodes.Length; i++)
        {
            string label = (i < localeLabels.Length && !string.IsNullOrEmpty(localeLabels[i]))
                ? localeLabels[i]
                : localeCodes[i];
            opts.Add(new TMP_Dropdown.OptionData(label));
        }
        myDropdown.AddOptions(opts);
    }

    // 드롭다운 값 변경시 호출 (언어 변경)
    void OnDropdownValueChanged(int index)
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        if (suppressDropdownEvent) return;

        ApplyLocaleByIndex(index, save: false);

        //playerInfo, mypage에도 적용
        if (intro) //게임 처음 시작시에만 사용
        {
            switch (index)
            {
                case 0: playerInfo.language = LANGUAGE.KOREAN; break;
                case 1: playerInfo.language = LANGUAGE.ENGLISH; break;
            }
            intro.WritePlayerFile();
        }
        else
        {   //기존 코드 (오류)
            // switch (index)
            // {
            //     case 0: mypage.SetKorean(); break;
            //     case 1: mypage.SetEnglish(); break;
            // }
        }
    }

    //index에 맞는 locale 코드 찾아 언어 변경하고 저장
    void ApplyLocaleByIndex(int index, bool save)
    {
        if (index < 0 || index >= localeCodes.Length)
        {
            Debug.LogError($"잘못된 인덱스: {index}");
            return;
        }

        string code = localeCodes[index];
        var locale = FindLocaleByCode(code);
        if (locale == null)
        {
            Debug.LogError($"잘못된 locale 코드: {code}");
            return;
        }

        LocalizationSettings.SelectedLocale = locale;

        if (save)
        {
            PlayerPrefs.SetString(PrefKey, code);
            PlayerPrefs.Save();
        }

        Debug.Log($"언어 미리보기 적용: {code} (save={save})");
    }

    // 외부에서 Locale이 바뀐 경우 드롭다운도 동기화
    void OnSelectedLocaleChanged(Locale locale)
    {
        int idx = IndexOfLocale(locale);
        if (idx >= 0 && idx < (myDropdown?.options.Count ?? 0))
        {
            suppressDropdownEvent = true;
            myDropdown.SetValueWithoutNotify(idx);
            suppressDropdownEvent = false;
        }
    }

    // 적용 버튼: 현재 선택 언어를 저장하고 팝업 닫음
    void OnClickApply()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        int idx = myDropdown.value;
        ApplyLocaleByIndex(idx, save: true);

        // 다음에 팝업 열 때의 이전값이 현재값이 되도록 스냅샷 갱신
        var cur = LocalizationSettings.SelectedLocale;
        preOpenLocaleCode = cur.Identifier.Code;
        preOpenDropdownIndex = IndexOfLocale(cur);

        // 팝업 닫기(여기서는 단순히 비활성화만 예시)
        gameObject.SetActive(false);
    }

    // 취소하기: 팝업 닫고 롤백
    void OnClickCancel()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        var prevLocale = FindLocaleByCode(preOpenLocaleCode);
        if (prevLocale != null)
        {
            LocalizationSettings.SelectedLocale = prevLocale;

            suppressDropdownEvent = true;
            if (preOpenDropdownIndex >= 0 && preOpenDropdownIndex < myDropdown.options.Count)
                myDropdown.SetValueWithoutNotify(preOpenDropdownIndex);
            suppressDropdownEvent = false;

            // 저장도 이전 값 그대로 유지(Apply에서만 저장하기 때문)
            Debug.Log($"{preOpenLocaleCode}로 롤백");
        }

        // 팝업 닫기
        gameObject.SetActive(false);
    }

    //코드에 맞는 Locale 찾기
    Locale FindLocaleByCode(string code)
    {
        foreach (var l in LocalizationSettings.AvailableLocales.Locales)
        {
            if (string.Equals(l.Identifier.Code, code, StringComparison.OrdinalIgnoreCase))
                return l;
        }

        string raw = code;
        var list = LocalizationSettings.AvailableLocales.Locales;
        var sb = new StringBuilder();
        sb.AppendLine($"[LangPopup] Locale NOT FOUND");
        sb.AppendLine($"  raw: '{raw}' (len={raw?.Length ?? -1})");
        sb.AppendLine($"  normalized: '{code}' (len={code?.Length ?? -1})");
        sb.AppendLine($"  Available Locales:");
        for (int i = 0; i < list.Count; i++)
            sb.AppendLine($"    {i}: {list[i].LocaleName} | Identifier={list[i].Identifier}");
        Debug.LogError(sb.ToString());
        return null;
    }

    //현재 locale이 localeCodes의 몇번째인지
    int IndexOfLocale(Locale locale)
    {
        if (locale == null) return -1;
        string curCode = locale.Identifier.Code;
        for (int i = 0; i < localeCodes.Length; i++)
        {
            if (string.Equals(localeCodes[i]?.Trim(), curCode, System.StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    void OnDestroy()
    {
        myDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }
}