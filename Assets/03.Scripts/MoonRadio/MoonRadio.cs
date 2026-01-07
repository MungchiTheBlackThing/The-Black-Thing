using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class MoonRadio : MonoBehaviour
{
    [SerializeField]
    GameObject moonRadioController;
    [SerializeField]
    GameObject alert;
    [SerializeField]
    TMP_Text text;
    [SerializeField]
    GameObject screen;
    [SerializeField]
    DotController Dot;

    TranslateManager translator;
    Animator blinkMoonRadioAnim;

    [SerializeField]
    string originaltext = "달이 뜬 밤, 저녁 여덟 시부터 \n새벽 세 시 까지";

    [SerializeField]
    string endtext = "치익-치이익.치직.\n송신 불가. 송신 불가";

    [SerializeField]
    string checktext = "작동하지 않는다.\n송신기가 고장난 듯하다.";


    private void Start()
    {
        screen = GameObject.Find("Screen");
        Dot = GameObject.FindWithTag("DotController").GetComponent<DotController>();
        blinkMoonRadioAnim = GetComponent<Animator>();
        moonRadioController = GameObject.Find("MoonRadio").transform.GetChild(0).gameObject;
        translator = GameObject.FindWithTag("Translator").GetComponent<TranslateManager>();
        translator.translatorDel += Translate;
        
        // 로컬라이제이션 설정 변경 감지
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        
        if (GameManager.isend && blinkMoonRadioAnim != null)
            blinkMoonRadioAnim.SetFloat("speed", 0f);
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
    
    void OnLocaleChanged(Locale locale)
    {
        // 언어 변경 시 alert가 활성화되어 있으면 텍스트 업데이트
        if (alert != null && text != null && alert.activeSelf)
        {
            // OpenAlert를 다시 호출하여 로컬라이제이션된 텍스트로 업데이트
            OpenAlert();
        }
    }
    
    void Translate(LANGUAGE language, TMP_FontAsset font)
    {
        // Translate는 폰트만 변경하고, 텍스트는 OpenAlert에서 로컬라이제이션 적용
        if (text != null && font != null)
        {
            text.font = font;
        }
    }
    private void OnMouseDown()
    {
        // ✅ 엔딩이면 알럿은 예외로 허용(유서 후에만 허용하고 싶으면 조건 바꿔)
        if (InputGuard.BlockWorldInput() && !(GameManager.isend && DeathNoteClick.readDeathnote))
            return;

        RecentData data = RecentManager.Load();
        if (!data.tutoend) return;

        // ✅ 엔딩이면 밤/낮 무관하게 무조건 checktext 알럿만
        if (GameManager.isend)
        {
            OpenAlert();
            return;
        }

        // (이하 기존 로직)
        if (alert != null)
        {
            OpenAlert();
            return;
        }

        //시간대가 밤이 아닐 경우에는 아래는 작동하지 않는다.
        if (moonRadioController.activeSelf == false && !Dot.tutorial && !GameManager.isend)
        {
            //밤일 때 speed 1, 밤이 아니면 0
            blinkMoonRadioAnim.SetFloat("speed", 0f);
            moonRadioController.SetActive(true);
            screen.SetActive(false);
        }
        Debug.Log("문라디오 클릭");
    }

    private void Update()
    {
        if (GameManager.isend && blinkMoonRadioAnim != null)
        {
            blinkMoonRadioAnim.enabled = false;
        }
    }

    public void OpenAlert()
    {
        if (alert == null || text == null)
            return;

        // 로컬라이제이션 패키지 사용
        StringTable table = LocalizationSettings.StringDatabase.GetTable("SystemUIText");
        if (table != null)
        {
            // 엔딩일 때는 checktext, 아니면 originaltext
            string key = GameManager.isend ? "mapalert_moonRadio_check" : "mapalert_moonRadio";
            var entry = table.GetEntry(key);
            if (entry != null)
            {
                text.text = entry.GetLocalizedString();
            }
            else
            {
                text.text = GameManager.isend ? checktext : originaltext;
            }
        }
        else
        {
            text.text = GameManager.isend ? checktext : originaltext;
        }

        if (!alert.activeSelf)
        {
            alert.SetActive(true);
            StartCoroutine(CloseAlter(alert));
        }
    }

    IEnumerator CloseAlter(GameObject alert)
    {
        yield return new WaitForSeconds(1.5f);
        alert.SetActive(false);
    }
}
