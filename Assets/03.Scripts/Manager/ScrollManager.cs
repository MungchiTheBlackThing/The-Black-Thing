using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class ScrollManager : MonoBehaviour
{
    // 상수 : 이동 관련
    private const float DirectionForceReduceRate = 0.935f; // 감속비율
    private const float DirectionForceMin = 0.001f; // 설정치 이하일 경우 움직임을 멈춤

    // 변수 : 이동 관련
    private bool userMoveInput; // 현재 조작을 하고있는지 확인을 위한 변수
    private Vector3 startPosition;  // 입력 시작 위치를 기억
    private Vector3 directionForce; // 조작을 멈췄을때 서서히 감속하면서 이동 시키기 위한 변수

    // 컴포넌트
    private Camera camera;

    [SerializeField]
    [Tooltip("LimitValue (minVal,maxVal)")]
    Vector2 camLimitValue;

    [SerializeField]
    [Tooltip("Scroll Speed")]

    [Range(0f, 1f)]
    float scollSpd;

    [SerializeField]
    bool isScreenStatic = false;


    Vector3 originalPos = new Vector3(0, 0, -10f);

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }
    public void StopCameraByPlayPhase(bool isScreenStatic)
    {
        if (camera == null) return;

        if (isScreenStatic)
        {
            originalPos = camera.transform.position;
            camera.transform.position = new Vector3(camLimitValue.y, 0, -10f);
        }

        this.isScreenStatic = isScreenStatic;
    }
    public void StopCamera(bool isScreenStatic)
    {
        if (camera == null) return;

        if(isScreenStatic)
        {
            originalPos = camera.transform.position;
            camera.transform.position = new Vector3(0, 0, -10f);
        }
        else
        {
            camera.transform.position = originalPos;
        }

        this.isScreenStatic = isScreenStatic;
    }

    // Update is called once per frame
    void Update()
    {
        if(isScreenStatic == false)
        {
            // 카메라 포지션 이동
            ControlCameraPosition();

            // 조작을 멈췄을때 감속
            ReduceDirectionForce();

            // 카메라 위치 업데이트
            UpdateCameraPosition();
        }
    }

    private void ControlCameraPosition()
    {
        //World 좌표 값을 가져온다.
        var mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);

        if (EventSystem.current.IsPointerOverGameObject())
        {
            // 마우스가 UI 위에 있을 때는 이 함수가 동작하지 않도록 함
            return;
        }

        if (Input.GetMouseButtonDown(0)) //입력이 처음 들어왔을 때
        {
            CameraPositionMoveStart(mouseWorldPosition); //누른 시작값 전달
        }
        else if (Input.GetMouseButton(0)) //입력이 진행 중일 때
        {
            CameraPositionMoveProgress(mouseWorldPosition); //이동 방향을 구하기 위해서 현재 좌표값
        }
        else
        {
            CameraPositionMoveEnd();
        }
    }
    private void CameraPositionMoveStart(Vector3 startPosition)
    {
        userMoveInput = true;
        this.startPosition = startPosition;
        directionForce = Vector2.zero;
    }
    private void CameraPositionMoveProgress(Vector3 targetPosition)
    {
        if (!userMoveInput)
        {
            CameraPositionMoveStart(targetPosition);
            return;
        }

        //이전 위치에서 현재 위치를 빼서 방향을 구한다.
        directionForce = startPosition - targetPosition;
    }
    private void CameraPositionMoveEnd()
    {
        userMoveInput = false;
    }
    private void ReduceDirectionForce()
    {
        // 조작 중일때는 아무것도 안함 -> 감속하지 않아도 된다., 스크롤 느낌을 주기위해서 감속을 사용
        if (userMoveInput)
        {
            return;
        }

        // 감속 수치 적용, 현재 방향성에서 속도를 조금씩 약하게 주면서 감속해준다.
        directionForce *= DirectionForceReduceRate;

        // 작은 수치가 되면 강제로 멈춤
        if (directionForce.magnitude < DirectionForceMin)
        {
            directionForce = Vector3.zero;
        }
    }
    private void UpdateCameraPosition()
    {
        // 이동 수치가 없으면 아무것도 안함
        if (directionForce == Vector3.zero)
        {
            return;
        }

        var currentPosition = transform.position; //현재 위치
        var targetPosition = currentPosition + directionForce; //힘을 더해주면 목표 위치가 나온다.

        targetPosition.x = Mathf.Clamp(targetPosition.x, camLimitValue.x, camLimitValue.y);
        targetPosition.y = 0;
        targetPosition.z = -10f;

        transform.position = Vector3.Lerp(currentPosition, targetPosition, scollSpd);
    }
    public void stopscroll()
    {
        //this.transform.position = originalPos; //임시방편이라 없애야됨
        isScreenStatic = true;
    }
    public void scrollable()
    {
        isScreenStatic = false;
    }

    public void MoveCamera(Vector3 targetPosition, float duration)
    {
        StartCoroutine(MoveCameraCoroutine(targetPosition, duration));
    }

    private IEnumerator MoveCameraCoroutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; 
        }
        
        transform.position = targetPosition;
    }
}
