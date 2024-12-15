using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateKit : Singleton<CreateKit>
{
    // �� ������ ���� ��ġ�� ���ϴ� �Լ�
    Vector3 GetRandomPositionInCircle(Vector3 center, float radius)
    {
        // ���� ������ ������ ������ ���� ����
        float angle = Random.Range(0f, 360f);

        // ���� ������ ������ ������ �Ÿ� ���� (0�� radius ����)
        float randomRadius = Random.Range(0f, radius);

        // ������ ���� x, z ��ǥ�� ��� (y ���� center.y�� �״�� ���)
        float x = center.x + randomRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
        float z = center.z + randomRadius * Mathf.Sin(angle * Mathf.Deg2Rad);

        // ���ο� ���� ��ġ ��ȯ
        return new Vector3(x, center.y, z);
    }
}
