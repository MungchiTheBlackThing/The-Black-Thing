using System.Collections;
using TMPro;
using UnityEngine;

public class MoonRadioEarthController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private TMP_InputField inputField;

    [Header("UI")]
    [SerializeField] private GameObject sendAlert;      // "보내졌습니다" 알럿
    [SerializeField] private GameObject closePopup;
    [SerializeField] private GameObject exceedAlert;    // 500자 초과 알럿
    [SerializeField] private TMP_Text textLength;       // N/500
    [SerializeField] private GameObject answerTextBox;  // 상태 UI(placeholder)

    [SerializeField] private GameObject emptyAlert;
    [SerializeField] private float NullAlertDuration = 1.2f;

    [Header("Animators (2)")]
    [SerializeField] private Animator sendUIAnimator;     // SendUI(페이드/전송 연출)
    [SerializeField] private Animator airplaneAnimator;   // 종이비행기 버튼

    [Header("Animator Triggers")]
    [SerializeField] private string sendTrigger = "Send";
    [SerializeField] private string resetTrigger = "Reset";

    [Header("Timing (seconds)")]
    [Tooltip("전송 애니가 끝나고 알럿이 떠야 하므로, SendUI/비행기 중 더 긴 전송 애니 길이로.")]
    [SerializeField] private float sendAnimationDuration = 0.6f;

    [Tooltip("알럿 표시 시간")]
    [SerializeField] private float alertDuration = 2.0f;
    [SerializeField] private float alertFadeIn = 1.5f;
    [SerializeField] private float alertFadeOut = 0.2f;


    private const int MAX_LEN = 500;

    private bool isWithin500 = true;
    private bool isTransmitting = false;
    private bool exceedAlertCoolingDown = false;



    private void OnEnable()
    {
        // 리스너 중복 방지
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnInputValueChanged);
            inputField.onValueChanged.AddListener(OnInputValueChanged);

            // 입력 검증 콜백 연결 (추가 입력 시도 감지)
            inputField.onValidateInput = ValidateInput;


            // 현재 텍스트로 UI 동기화
            OnInputValueChanged(inputField.text);
        }

        if (sendAlert != null) sendAlert.SetActive(false);
        if (exceedAlert != null) exceedAlert.SetActive(false);
        if (emptyAlert != null) emptyAlert.SetActive(false);


        isTransmitting = false;
    }

    private void OnDisable()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnInputValueChanged);
            inputField.onValidateInput = null;
        }
    }

    // AnswerTextbox는 입력 길이 상태 UI (onValueChanged로만 제어) 
    private void OnInputValueChanged(string s)
    {
        int len = string.IsNullOrEmpty(s) ? 0 : s.Length;

        if (answerTextBox != null)
            answerTextBox.SetActive(len == 0);

        if (textLength != null)
            textLength.text = $"{len}/{MAX_LEN}";

        isWithin500 = (len <= MAX_LEN);
    }

    // ===== 보내기 버튼 클릭 =====
    public void Send2MoonBut()
    {
        if (isTransmitting) return;

        string s = inputField != null
        ? inputField.text.TrimEnd('\n', '\r')
        : "";

        if (string.IsNullOrWhiteSpace(s))
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonlock, transform.position);
            StartCoroutine(CoShowEmptyAlert());
            return;
        }

        isTransmitting = true;

        // 전송 시작: 두 애니메이터에 Send 트리거
        FireTrigger(sendUIAnimator, sendTrigger);
        FireTrigger(airplaneAnimator, sendTrigger);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.earthsend, transform.position);

        // 직선 플로우 시작
        StartCoroutine(CoTransmissionFlow());
    }

    private IEnumerator CoShowEmptyAlert()
    {
        if (emptyAlert != null) emptyAlert.SetActive(true);
        yield return new WaitForSeconds(NullAlertDuration);
        if (emptyAlert != null) emptyAlert.SetActive(false);
    }
    private IEnumerator CoFadeCanvasGroup(
    CanvasGroup cg,
    float from,
    float to,
    float duration)
    {
        if (cg == null) yield break;

        cg.alpha = from;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        cg.alpha = to;
    }


    private IEnumerator CoTransmissionFlow()
    {
        // 전송 애니 끝날 때까지 대기
        yield return new WaitForSeconds(sendAnimationDuration);

        // 알럿 ON + 페이드 인
        CanvasGroup alertCG = null;
        if (sendAlert != null)
        {
            sendAlert.SetActive(true);
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.earthsent, transform.position);

            alertCG = sendAlert.GetComponent<CanvasGroup>();
            if (alertCG != null)
                yield return StartCoroutine(CoFadeCanvasGroup(alertCG, 0f, 1f, alertFadeIn));
        }

        // 완전히 보이는 유지 시간 (기존 alertDuration)
        yield return new WaitForSeconds(alertDuration);

        // 페이드 아웃
        if (sendAlert != null && alertCG != null)
            yield return StartCoroutine(CoFadeCanvasGroup(alertCG, 1f, 0f, alertFadeOut));

        if (sendAlert != null) sendAlert.SetActive(false);

        // 여기서 동시에 복귀시키기
        FireTrigger(sendUIAnimator, resetTrigger);
        FireTrigger(airplaneAnimator, resetTrigger);

        // 입력 초기화
        if (inputField != null)
        {
            inputField.text = string.Empty;
            OnInputValueChanged(inputField.text);
            inputField.ActivateInputField();
        }

        isTransmitting = false;
    }


    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        // 이미 500자에 도달했고, 더 입력하려는 시도라면
        if (text != null && text.Length >= MAX_LEN)
        {
            // 알럿 연타 방지(쿨다운)
            if (!exceedAlertCoolingDown)
                StartCoroutine(CoShowExceedAlertOnce());

            // 이 문자를 거부해서 입력이 더 늘지 않게 함
            return '\0';
        }

        // 정상 입력 허용
        return addedChar;
    }

    private IEnumerator CoShowExceedAlertOnce()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonlock, transform.position);
        exceedAlertCoolingDown = true;

        if (exceedAlert != null) exceedAlert.SetActive(true);
        yield return new WaitForSeconds(alertDuration);
        if (exceedAlert != null) exceedAlert.SetActive(false);

        exceedAlertCoolingDown = false;
    }


    // ===== 채널 종료 =====
    public void ExitChannelBut()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonoff, transform.position);
        if (closePopup != null) closePopup.SetActive(true);
    }

    public void YesBut()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonoff, transform.position);
        if (closePopup != null) closePopup.SetActive(false);
        gameObject.SetActive(false);
    }

    public void NoBut()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.moonbuttonclick, transform.position);
        if (closePopup != null) closePopup.SetActive(false);
    }

    // ===== Animator helper =====
    private void FireTrigger(Animator anim, string trig)
    {
        if (anim == null) return;
        anim.ResetTrigger(trig);  // 안전장치
        anim.SetTrigger(trig);
    }
}
