using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
public class EffectController : MonoBehaviour
{
    [InfoBox("생성할 프리팹")]
    [SerializeField] 
    private GameObject Effect;
    
    [InfoBox("스폰 영역 크기")]
    [SerializeField] 
    private Vector2 spawnArea;

    [InfoBox("생성 간격")]
    [SerializeField] 
    private float spawnIntervalMin = 0.1f;
    [SerializeField]
    private float spawnIntervalMax = 0.5f;

    [InfoBox("한번에 생성할 프리팹 개수")]
    [SerializeField] 
    private int spawnCountMin = 1;
    [SerializeField]
    private int spawnCountMax = 4;
    
    private float timer;

    void Start()
    {
        timer = 0.1f;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        
        if (timer <= 0)
        {
            for(int i = 0; i < Random.Range(spawnCountMin, spawnCountMax); i++)
            {
                SpawnPrefab();
            }
            timer = Random.Range(spawnIntervalMin, spawnIntervalMax);
        }
    }

    private void SpawnPrefab()
    {
        // 지정된 범위 내에서 랜덤한 위치 계산
        float randomX = Random.Range(-spawnArea.x/2, spawnArea.x/2);
        float randomY = Random.Range(-spawnArea.y/2, spawnArea.y/2);
        Vector3 randomPosition = transform.position + new Vector3(randomX, randomY, 0);

        // 프리팹 생성
        Instantiate(Effect, randomPosition, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea.x, spawnArea.y, 0));
    }
}
