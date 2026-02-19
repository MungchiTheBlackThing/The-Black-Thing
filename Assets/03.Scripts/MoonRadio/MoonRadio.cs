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
using Assets.Script.TimeEnum;
using UnityEngine.SceneManagement;



public class MoonRadio : MonoBehaviour
{
    private GameManager gm;

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

    [SerializeField] private GameObject light_moonradio;


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
        
        gm = FindObjectOfType<GameManager>(true);
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        if (translator != null)
            translator.translatorDel -= Translate;
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

        // 아무 버튼 클릭 시도 후 호출되는 곳에 임시로 추가
        var results = new List<UnityEngine.EventSystems.RaycastResult>();
        var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = Input.mousePosition
        };
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, results);
        foreach (var r in results)
            Debug.Log($"[Raycast] {r.gameObject.name} / {r.gameObject.layer}");
        // 0) 튜토리얼 씬이면 무조건 입력 차단
        if (IsTutorialScene()) return;

        // 1) 엔딩이면 시간대 무관: 엔딩 알럿
        if (GameManager.isend)
        {
            OpenAlert();
            return;
        }

        // 2) 일반 입력 가드
        if (InputGuard.BlockWorldInput()) return;

        // 3) 시간대 체크
        bool isNight = (gm != null && gm.CurrentSITime == SITime.Night);

        // 4) 밤이 아니면: 알럿
        if (!isNight)
        {
            OpenAlert();
            return;
        }

        // 4) 밤이면: 송신기 컨트롤러 열기
        if (blinkMoonRadioAnim != null) blinkMoonRadioAnim.SetFloat("speed", 1f);
        if (moonRadioController != null) moonRadioController.SetActive(true);
        if (screen != null) screen.SetActive(false);
    }

    private bool IsTutorialScene()
    {
        return SceneManager.GetActiveScene().name == "Tutorial";
    }

    private bool _endingApplied;

    private void Update()
    {
        if (GameManager.isend && !_endingApplied)
        {
            ApplyEndingMoonRadioLock();
            _endingApplied = true;
        }

        if (GameManager.isend) return;

        bool isNight = (gm != null && gm.CurrentSITime == SITime.Night);

        if (blinkMoonRadioAnim != null)
            blinkMoonRadioAnim.SetFloat("speed", isNight ? 1f : 0f);

        if (light_moonradio != null)
            light_moonradio.SetActive(isNight);

        // 낮이면 혹시 열려있던 컨트롤러 닫기(원하면)
        if (!isNight && moonRadioController != null && moonRadioController.activeSelf)
            moonRadioController.SetActive(false);
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
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.lockClick, transform.position);
            StartCoroutine(CloseAlter(alert));
        }
    }

    IEnumerator CloseAlter(GameObject alert)
    {
        yield return new WaitForSeconds(1.5f);
        alert.SetActive(false);
    }

    public void ApplyEndingMoonRadioLock()
    {
        // 깜빡이 애니메이션 완전 정지
        if (blinkMoonRadioAnim != null)
        {
            blinkMoonRadioAnim.enabled = false;      // Animator 자체 OFF (가장 확실)
        }

        if (light_moonradio != null)
        {
            var r = light_moonradio.GetComponentInChildren<Renderer>(true);
            if (r != null) r.enabled = false;

            var sr = light_moonradio.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null) sr.enabled = false;
        }

        // 3) 송신기 UI/클릭도 막기
        if (moonRadioController != null)
            moonRadioController.SetActive(false);

    }
}
