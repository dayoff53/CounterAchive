using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaloController : MonoBehaviour
{
    [Tooltip("���� ��� ������Ʈ�� �����մϴ�.")]
    [SerializeField] private Transform target;

    [Tooltip("Ÿ���� �������� �� ���Ϸ��� ��ǥ ��ġ")]
    [SerializeField] private Vector3 haloPos;

    [Tooltip("���󰡴� �ӵ��� �����մϴ�.")]
    [SerializeField] private float floatingSpeed = 0.25f;

    [Tooltip("���Ϸ��� ���ϴ� �̵� ����")]
    [SerializeField] private float floatingRange = 0.5f;

    [Tooltip("���󰡴� �ӵ��� �����մϴ�.")]
    [SerializeField] private float followSpeed = 2.0f;

    private Vector3 offset;

    void Start()
    {
        // �ʱ� �������� �����մϴ�.
        offset = transform.position - target.position + haloPos;
    }

    void Update()
    {
        // Time.time�� ����Ͽ� ping-pong ȿ���� ����ϴ�.
        float floatingPos = Mathf.PingPong(Time.time * floatingSpeed, floatingRange) - (floatingRange / 2);

        // Ÿ�� ��ġ�� �������� ���Ͽ� ��ǥ ��ġ�� �����մϴ�.
        Vector3 targetPosition = target.position + offset;

        // floatingPos�� ����Ͽ� y�� ��ġ�� �����մϴ�.
        targetPosition.y += floatingPos;

        // ���� ��ġ���� ��ǥ ��ġ�� �ε巴�� �̵��մϴ�.
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
