using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;



public class SubDialAlert : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    [SerializeField] private TextMeshProUGUI secondText;

    [SerializeField] private List<Image> images;

    [SerializeField] private GameManager gameManager;
    
    [Header("Sub Skip")]
    [SerializeField] private GameObject subSkipPopup;
    [SerializeField] private Button subSkipYes;
    [SerializeField] private Button subSkipNo;
    [SerializeField] private GameObject subTriggerIcon; // 뭉치 위 ‘서브 트리거’ 아이콘
    [SerializeField] private CanvasGroup cg;

    private bool canOpenPopup = true;
    private Coroutine refreshCo;
    void Awake()
    {

        if (player == null) player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        if (player != null)
        {
            player.nextPhaseDelegate -= OnPhaseChanged;
            player.nextPhaseDelegate += OnPhaseChanged; // 중복 구독 방지
        }
    }
    
    void OnEnable()
    {
        SetVisible(false);
        StartRefresh();

    }

    private void OnDestroy()
    {
        if (player != null) player.nextPhaseDelegate -= OnPhaseChanged;
    }
    private void OnDisable()
    {
        if (refreshCo != null) StopCoroutine(refreshCo);
        refreshCo = null;
    }

    private void OnPhaseChanged(GamePatternState _)
    {
        StartRefresh(); // 페이즈 바뀔 때마다 갱신
    }
    private void StartRefresh()
    {
        if (!isActiveAndEnabled) return;
        if (refreshCo != null) StopCoroutine(refreshCo);
        refreshCo = StartCoroutine(RefreshCo());
    }

    private IEnumerator RefreshCo()
    {

        yield return LocalizationSettings.InitializationOperation;
        RefreshNow();

    }

    void RefreshNow()

    {
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            SetVisible(false);
            return;
        }

        if (player == null) player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        if (player != null)
        {
            player.nextPhaseDelegate -= OnPhaseChanged;
            player.nextPhaseDelegate += OnPhaseChanged;
        }
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();

        if (player == null || gameManager == null)
        {
            Debug.LogWarning("SubDialAlert: player/gameManager null");
            return;
        }

        var phaseState = (GamePatternState)player.GetCurrentPhase();

        // MainA/MainB/Play에서는 종이 자체 OFF
        if (phaseState == GamePatternState.MainA ||
            phaseState == GamePatternState.MainB ||
            phaseState == GamePatternState.Play)
        {
                SetVisible(false);
                return;
        }

        List<int> subs = gameManager.GetSubseqsForPhase(phaseState);
        int count = (subs != null) ? subs.Count : 0;

        // 슬롯 활성화: 서브 개수만큼만
        for (int i = 0; i < images.Count; i++)
        {
            bool on = i < count;
            images[i].gameObject.SetActive(on);
            if (!on) images[i].sprite = null; // 과거 값 청소
        }

        int findChapter = player.GetChapter();
        ChapterInfo chapterInfo = DataManager.Instance.ChapterList.chapters[findChapter];

        if (chapterInfo == null)
        {
            Debug.LogWarning("SubDialAlert: chapterInfo null");
            return;
        }

        // 남은 개수 = (현재 페이즈의 subs 중 아직 안 본 것)
        int remaining = 0;
        if (subs != null)
        {
            foreach (var id in subs)
                if (!player.IsSubWatched(id)) remaining++;
        }
        canOpenPopup = (remaining > 0);

        if (secondText != null)
        {
            if (remaining > 0)
            {
                // 템플릿은 UI 저장하지 말고 로컬라이즈 키에서 매번 가져오기
                string tpl = LocalizationSettings.StringDatabase.GetLocalizedString("SystemUIText", "subcheck_text");

                if (string.IsNullOrEmpty(tpl) || !tpl.Contains("<int>"))
                    tpl = "<int>"; // 키 누락/값 오타 대비

                secondText.text = tpl.Replace("<int>", remaining.ToString());
            }
            else
            {
                // 남은 서브 0개면 완료 문구로 교체 (한/영)
                string doneText = LocalizationSettings.StringDatabase.GetLocalizedString("SystemUIText", "subalert_n");
                secondText.text = doneText;
            }
        }

        // 성공 여부
        List<bool> successPhase = player.GetSubPhase(chapterInfo.id);

        // 아이콘 적용 (subs 기반)
        ApplySubPhaseIcons(chapterInfo, successPhase, subs);
        SetVisible(true);
    }

    private void SetVisible(bool on)
    {
        if (cg != null)
        {
            cg.alpha = on ? 1 : 0;
            cg.blocksRaycasts = on;
            cg.interactable = on;
        }
        else
        {
            gameObject.SetActive(on);
        }
    }



    private void ApplySubPhaseIcons(ChapterInfo chapterInfo, List<bool> successPhase, List<int> subs)
    {
        if (subs == null) return;

        for (int i = 0; i < subs.Count && i < images.Count; i++)
        {
            int subseqId = subs[i];    // 1..4
            int idx = subseqId - 1;    // 0..3

            bool watched = player.IsSubWatched(subseqId);
            bool success = (successPhase != null && idx >= 0 && idx < successPhase.Count) ? successPhase[idx] : false;

            string path = (watched && success)
                ? chapterInfo.subFilePath[idx]
                : chapterInfo.subLockFilePath[idx];

            images[i].sprite = Resources.Load<Sprite>(path);
        }
    }
        public void OnClickSubImage()
    {
        // 트리거 아이콘이 떠 있으면: 팝업 막고 lock 효과음만
        if (subTriggerIcon != null && subTriggerIcon.activeSelf)
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.lockClick, transform.position);
            return;
        }
        if (!canOpenPopup)
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.lockClick, transform.position);
            return;
        }
        if (subSkipPopup != null) subSkipPopup.SetActive(true);
    }

    // Yes -> 팝업 OFF + 서브 즉시
    public void OnClickSubSkipYes()
    {
        if (subSkipPopup != null) subSkipPopup.SetActive(false);
        gameManager.SkipSubDialWaitAndShowNow();
    }

    // No -> 팝업 OFF
    public void OnClickSubSkipNo()
    {
        if (subSkipPopup != null) subSkipPopup.SetActive(false);
    }

}
