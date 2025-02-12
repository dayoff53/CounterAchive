using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateKit : Singleton<CreateKit>
{
    // 원 안에서 랜덤 위치를 구하는 함수
    Vector3 GetRandomPositionInCircle(Vector3 center, float radius)
    {
        // 랜덤 위치의 각도를 구하기 위한 변수
        float angle = Random.Range(0f, 360f);

        // 랜덤 위치의 중심과 거리를 구함 (0과 radius 사이)
        float randomRadius = Random.Range(0f, radius);

        // 구해진 값을 x, z 좌표로 변환 (y 값은 center.y로 그대로 유지)
        float x = center.x + randomRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
        float z = center.z + randomRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

        // 새로운 랜덤 위치 반환
        return new Vector3(x, center.y, z);
    }
}
