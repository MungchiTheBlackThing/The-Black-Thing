using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class FixedAspectLetterbox2796x1290 : MonoBehaviour
{
    [Header("Target resolution (aspect 기준)")]
    public float targetWidth = 2796f;
    public float targetHeight = 1290f;

    [Header("Black bar color")]
    public Color barColor = Color.black;

    [Header("Optional: create a background camera automatically")]
    public bool autoCreateBackgroundCamera = true;

    private Camera mainCam;
    private Camera bgCam;

    private int lastW = -1;
    private int lastH = -1;

    void Awake()
    {
        mainCam = GetComponent<Camera>();

        if (autoCreateBackgroundCamera)
            EnsureBackgroundCamera();
    }

    void OnEnable()
    {
        Apply();
    }

    void Update()
    {
        // 화면 크기 변경 감지 (에디터 리사이즈/모바일 회전 대응)
        if (Screen.width != lastW || Screen.height != lastH)
            Apply();
    }

    private void Apply()
    {
        lastW = Screen.width;
        lastH = Screen.height;

        float targetAspect = targetWidth / targetHeight;
        float windowAspect = (float)Screen.width / Screen.height;

        Rect rect;

        if (windowAspect > targetAspect)
        {
            // 화면이 더 넓음 -> 좌/우 검은 영역(필러박스)
            float scale = targetAspect / windowAspect; // 0~1
            float x = (1f - scale) * 0.5f;
            rect = new Rect(x, 0f, scale, 1f);
        }
        else
        {
            // 화면이 더 높음 -> 상/하 검은 영역(레터박스)
            float scale = windowAspect / targetAspect; // 0~1
            float y = (1f - scale) * 0.5f;
            rect = new Rect(0f, y, 1f, scale);
        }

        // 1) 배경카메라가 전체 화면을 검게 Clear
        if (bgCam != null)
        {
            bgCam.backgroundColor = barColor;
            bgCam.rect = new Rect(0f, 0f, 1f, 1f);
        }

        // 2) 메인카메라는 목표 비율에 해당하는 영역만 렌더
        mainCam.rect = rect;

        // 배경이 이미 검정이므로 메인은 색 지우기 불필요(깜빡임/이중클리어 방지)
        mainCam.clearFlags = CameraClearFlags.Depth;
    }

    private void EnsureBackgroundCamera()
    {
        // 이미 있으면 재사용
        Transform existing = transform.Find("__LetterboxBGCamera");
        if (existing != null)
        {
            bgCam = existing.GetComponent<Camera>();
            return;
        }

        GameObject go = new GameObject("__LetterboxBGCamera");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        bgCam = go.AddComponent<Camera>();

        // 배경카메라 설정: 아무 것도 그리지 않고, 전체 화면을 검게 지우기만
        bgCam.cullingMask = 0;                 // Nothing
        bgCam.clearFlags = CameraClearFlags.SolidColor;
        bgCam.backgroundColor = barColor;

        // 메인보다 먼저 렌더되게 depth 낮게
        bgCam.depth = mainCam.depth - 1f;
        bgCam.rect = new Rect(0f, 0f, 1f, 1f);

        // 기타: 메인과 동일한 프로젝션 유지(의미는 거의 없지만 안정적으로)
        bgCam.orthographic = mainCam.orthographic;
        bgCam.fieldOfView = mainCam.fieldOfView;
        bgCam.orthographicSize = mainCam.orthographicSize;
        bgCam.nearClipPlane = mainCam.nearClipPlane;
        bgCam.farClipPlane = mainCam.farClipPlane;
    }
}
