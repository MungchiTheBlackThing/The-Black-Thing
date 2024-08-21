using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class ScrollManager : MonoBehaviour
{
    // ��� : �̵� ����
    private const float DirectionForceReduceRate = 0.935f; // ���Ӻ���
    private const float DirectionForceMin = 0.001f; // ����ġ ������ ��� �������� ����

    // ���� : �̵� ����
    private bool _userMoveInput; // ���� ������ �ϰ��ִ��� Ȯ���� ���� ����
    private Vector3 _startPosition;  // �Է� ���� ��ġ�� ���
    private Vector3 _directionForce; // ������ �������� ������ �����ϸ鼭 �̵� ��Ű�� ���� ����

    // ������Ʈ
    private Camera _camera;

    [SerializeField]
    [Tooltip("LimitValue (minVal,maxVal)")]
    Vector2 camLimitValue;

    [SerializeField]
    [Tooltip("Scroll Speed")]

    [Range(0f, 1f)]
    float scollSpd;

    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // ī�޶� ������ �̵�
        ControlCameraPosition();

        // ������ �������� ����
        ReduceDirectionForce();

        // ī�޶� ��ġ ������Ʈ
        UpdateCameraPosition();

    }

    private void ControlCameraPosition()
    {
        //World ��ǥ ���� �����´�.
        var mouseWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);

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
        _userMoveInput = true;
        _startPosition = startPosition;
        _directionForce = Vector2.zero;
    }
    private void CameraPositionMoveProgress(Vector3 targetPosition)
    {
        if (!_userMoveInput)
        {
            CameraPositionMoveStart(targetPosition);
            return;
        }

        //���� ��ġ���� ���� ��ġ�� ���� ������ ���Ѵ�.
        _directionForce = _startPosition - targetPosition;
    }
    private void CameraPositionMoveEnd()
    {
        _userMoveInput = false;
    }
    private void ReduceDirectionForce()
    {
        // ���� ���϶��� �ƹ��͵� ���� -> �������� �ʾƵ� �ȴ�., ��ũ�� ������ �ֱ����ؼ� ������ ���
        if (_userMoveInput)
        {
            return;
        }

        // ���� ��ġ ����, ���� ���⼺���� �ӵ��� ���ݾ� ���ϰ� �ָ鼭 �������ش�.
        _directionForce *= DirectionForceReduceRate;

        // ���� ��ġ�� �Ǹ� ������ ����
        if (_directionForce.magnitude < DirectionForceMin)
        {
            _directionForce = Vector3.zero;
        }
    }
    private void UpdateCameraPosition()
    {
        // �̵� ��ġ�� ������ �ƹ��͵� ����
        if (_directionForce == Vector3.zero)
        {
            return;
        }

        var currentPosition = transform.position; //���� ��ġ
        var targetPosition = currentPosition + _directionForce; //���� �����ָ� ��ǥ ��ġ�� ���´�.

        targetPosition.x = Mathf.Clamp(targetPosition.x, camLimitValue.x, camLimitValue.y);
        targetPosition.y = 0;
        targetPosition.z = -10f;

        transform.position = Vector3.Lerp(currentPosition, targetPosition, scollSpd);
    }
}
