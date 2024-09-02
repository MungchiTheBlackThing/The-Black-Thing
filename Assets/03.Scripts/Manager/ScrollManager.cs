using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class ScrollManager : MonoBehaviour
{
    // ��� : �̵� ����
    private const float DirectionForceReduceRate = 0.935f; // ���Ӻ���
    private const float DirectionForceMin = 0.001f; // ����ġ ������ ��� �������� ����

    // ���� : �̵� ����
    private bool userMoveInput; // ���� ������ �ϰ��ִ��� Ȯ���� ���� ����
    private Vector3 startPosition;  // �Է� ���� ��ġ�� ���
    private Vector3 directionForce; // ������ �������� ������ �����ϸ鼭 �̵� ��Ű�� ���� ����

    // ������Ʈ
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

    private void Start()
    {
        camera = GetComponent<Camera>();
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
            // ī�޶� ������ �̵�
            ControlCameraPosition();

            // ������ �������� ����
            ReduceDirectionForce();

            // ī�޶� ��ġ ������Ʈ
            UpdateCameraPosition();
        }
    }

    private void ControlCameraPosition()
    {
        //World ��ǥ ���� �����´�.
        var mouseWorldPosition = camera.ScreenToWorldPoint(Input.mousePosition);

        if (EventSystem.current.IsPointerOverGameObject())
        {
            // ���콺�� UI ���� ���� ���� �� �Լ��� �������� �ʵ��� ��
            return;
        }

        if (Input.GetMouseButtonDown(0)) //�Է��� ó�� ������ ��
        {
            CameraPositionMoveStart(mouseWorldPosition); //���� ���۰� ����
        }
        else if (Input.GetMouseButton(0)) //�Է��� ���� ���� ��
        {
            CameraPositionMoveProgress(mouseWorldPosition); //�̵� ������ ���ϱ� ���ؼ� ���� ��ǥ��
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

        //���� ��ġ���� ���� ��ġ�� ���� ������ ���Ѵ�.
        directionForce = startPosition - targetPosition;
    }
    private void CameraPositionMoveEnd()
    {
        userMoveInput = false;
    }
    private void ReduceDirectionForce()
    {
        // ���� ���϶��� �ƹ��͵� ���� -> �������� �ʾƵ� �ȴ�., ��ũ�� ������ �ֱ����ؼ� ������ ���
        if (userMoveInput)
        {
            return;
        }

        // ���� ��ġ ����, ���� ���⼺���� �ӵ��� ���ݾ� ���ϰ� �ָ鼭 �������ش�.
        directionForce *= DirectionForceReduceRate;

        // ���� ��ġ�� �Ǹ� ������ ����
        if (directionForce.magnitude < DirectionForceMin)
        {
            directionForce = Vector3.zero;
        }
    }
    private void UpdateCameraPosition()
    {
        // �̵� ��ġ�� ������ �ƹ��͵� ����
        if (directionForce == Vector3.zero)
        {
            return;
        }

        var currentPosition = transform.position; //���� ��ġ
        var targetPosition = currentPosition + directionForce; //���� �����ָ� ��ǥ ��ġ�� ���´�.

        targetPosition.x = Mathf.Clamp(targetPosition.x, camLimitValue.x, camLimitValue.y);
        targetPosition.y = 0;
        targetPosition.z = -10f;

        transform.position = Vector3.Lerp(currentPosition, targetPosition, scollSpd);
    }
}
