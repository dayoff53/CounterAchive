using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaloController : MonoBehaviour
{
    [Tooltip("따라갈 대상 오브젝트를 설정합니다.")]
    [SerializeField] private Transform target;

    [Tooltip("타겟을 기준으로 한 헤일로의 목표 위치")]
    [SerializeField] private Vector3 haloPos;

    [Tooltip("따라가는 속도를 설정합니다.")]
    [SerializeField] private float floatingSpeed = 0.25f;

    [Tooltip("헤일로의 상하단 이동 범위")]
    [SerializeField] private float floatingRange = 0.5f;

    [Tooltip("따라가는 속도를 설정합니다.")]
    [SerializeField] private float followSpeed = 2.0f;

    private Vector3 offset;

    void Start()
    {
        // 초기 오프셋을 설정합니다.
        offset = transform.position - target.position + haloPos;
    }

    void Update()
    {
        // Time.time을 사용하여 ping-pong 효과를 만듭니다.
        float floatingPos = Mathf.PingPong(Time.time * floatingSpeed, floatingRange) - (floatingRange / 2);

        // 타겟 위치와 오프셋을 더하여 목표 위치를 설정합니다.
        Vector3 targetPosition = target.position + offset;

        // floatingPos를 사용하여 y축 위치를 변경합니다.
        targetPosition.y += floatingPos;

        // 현재 위치에서 목표 위치로 부드럽게 이동합니다.
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
