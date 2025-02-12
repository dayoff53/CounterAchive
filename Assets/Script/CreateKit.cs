using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateKit : Singleton<CreateKit>
{
    // 원 내부의 랜덤 위치를 구하는 함수
    Vector3 GetRandomPositionInCircle(Vector3 center, float radius)
    {
        // 원의 반지름 내에서 랜덤한 각도 선택
        float angle = Random.Range(0f, 360f);

        // 원의 반지름 내에서 랜덤한 거리 선택 (0과 radius 사이)
        float randomRadius = Random.Range(0f, radius);

        // 각도에 따라 x, z 좌표를 계산 (y 값은 center.y를 그대로 사용)
        float x = center.x + randomRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
        float z = center.z + randomRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

        // 새로운 랜덤 위치 반환
        return new Vector3(x, center.y, z);
    }
}
