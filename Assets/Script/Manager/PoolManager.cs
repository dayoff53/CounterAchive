using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// 특정 프리팹을 기반으로 GameObject를 관리하는 풀 클래스
class Pool
{
    // 새로운 GameObject를 생성할 때 사용할 프리팹
    GameObject _prefab;
    // 객체 풀링을 관리하는 IObjectPool 인터페이스
    IObjectPool<GameObject> _pool;

    // 풀링된 오브젝트들을 계층 구조에서 정리하기 위한 부모 Transform
    Transform _root;
    Transform Root
    {
        get
        {
            // _root가 아직 초기화되지 않았다면 새로운 GameObject를 생성하여 부모로 설정
            if (_root == null)
            {
                GameObject go = new GameObject() { name = $"@{_prefab.name}Pool" };
                _root = go.transform;
            }
            return _root;
        }
    }

    // 생성자: 프리팹을 받아 풀을 초기화
    public Pool(GameObject prefab)
    {
        _prefab = prefab;
        // 객체 생성, 획득, 반환, 삭제 시 호출될 콜백을 지정하여 ObjectPool을 생성
        _pool = new ObjectPool<GameObject>(OnCreate, OnGet, OnRelease, OnDestroy);
    }

    // GameObject를 풀에 반환하는 메서드
    public void Push(GameObject go)
    {
        // 활성화된 상태의 오브젝트만 풀에 반환
        if (go.activeSelf)
            _pool.Release(go);
    }

    // 풀에서 GameObject를 가져오는 메서드
    public GameObject Pop()
    {
        return _pool.Get();
    }

    #region 풀 콜백 메서드들
    // 풀에서 새로운 GameObject를 생성해야 할 때 호출
    GameObject OnCreate()
    {
        // 프리팹을 기반으로 새로운 GameObject를 생성
        GameObject go = GameObject.Instantiate(_prefab);
        // 생성된 오브젝트를 Root 아래에 배치
        go.transform.SetParent(Root);
        // 오브젝트의 이름을 프리팹의 이름으로 설정
        go.name = _prefab.name;
        return go;
    }

    // 풀에서 GameObject를 가져올 때 호출
    void OnGet(GameObject go)
    {
        // 오브젝트가 null인지 확인
        if (go is null)
        {
            return;
        }
        // 오브젝트를 활성화하여 씬에서 보이도록 한다.
        go.SetActive(true);
    }

    // GameObject를 풀에 반환할 때 호출
    void OnRelease(GameObject go)
    {
        // 오브젝트를 비활성화하여 씬에서 보이지 않도록 한다.
        go.SetActive(false);
    }

    // 풀에서 GameObject를 삭제해야 할 때 호출
    void OnDestroy(GameObject go)
    {
        // 오브젝트를 삭제하여 메모리를 해제
        GameObject.Destroy(go);
    }
    #endregion
}

// 여러 종류의 GameObject 풀을 관리하는 싱글톤 클래스
public class PoolManager : Singleton<PoolManager>
{
    // 프리팹 이름과 해당 풀을 매핑하는 딕셔너리
    Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

    /// <summary>
    /// 특정 프리팹에 해당하는 풀에서 GameObject를 가져온다
    /// </summary>
    /// <param name="prefab">가져올 프리팹</param>
    /// <returns>가져온 GameObject</returns>
    public GameObject Pop(GameObject prefab)
    {
        // 해당 프리팹의 풀이 없으면 생성
        if (_pools.ContainsKey(prefab.name) == false)
            CreatePool(prefab);

        // 해당 풀에서 GameObject를 가져온다
        return _pools[prefab.name].Pop();
    }

    /// <summary>
    ///  GameObject를 해당 풀에 반환
    /// </summary>
    /// <param name="go">반활할 GameObject</param>
    /// <returns>반환 성공 여부</returns>
    public bool Push(GameObject go)
    {
        // 해당 GameObject의 풀이 없으면 false를 반환
        if (_pools.ContainsKey(go.name) == false)
            return false;

        // GameObject를 풀에 반환
        _pools[go.name].Push(go);
        return true;
    }

    // 모든 풀을 정리
    public void Clear()
    {
        _pools.Clear();
    }

    // 새로운 풀을 생성
    void CreatePool(GameObject original)
    {
        // 프리팹을 기반으로 새로운 풀을 생성
        Pool pool = new Pool(original);
        // 생성된 풀을 딕셔너리에 추가
        _pools.Add(original.name, pool);
    }
}
