using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// Ư�� �������� ������� GameObject�� �����ϴ� Ǯ Ŭ����
class Pool
{
    // ���ο� GameObject�� ������ �� ����� ������
    GameObject _prefab;
    // ��ü Ǯ���� �����ϴ� IObjectPool �������̽�
    IObjectPool<GameObject> _pool;

    // Ǯ���� ������Ʈ���� ���� �������� �����ϱ� ���� �θ� Transform
    Transform _root;
    Transform Root
    {
        get
        {
            // _root�� ���� �ʱ�ȭ���� �ʾҴٸ� ���ο� GameObject�� �����Ͽ� �θ�� ����
            if (_root == null)
            {
                GameObject go = new GameObject() { name = $"@{_prefab.name}Pool" };
                _root = go.transform;
            }
            return _root;
        }
    }

    // ������: �������� �޾� Ǯ�� �ʱ�ȭ
    public Pool(GameObject prefab)
    {
        _prefab = prefab;
        // ��ü ����, ȹ��, ��ȯ, ���� �� ȣ��� �ݹ��� �����Ͽ� ObjectPool�� ����
        _pool = new ObjectPool<GameObject>(OnCreate, OnGet, OnRelease, OnDestroy);
    }

    // GameObject�� Ǯ�� ��ȯ�ϴ� �޼���
    public void Push(GameObject go)
    {
        // Ȱ��ȭ�� ������ ������Ʈ�� Ǯ�� ��ȯ
        if (go.activeSelf)
            _pool.Release(go);
    }

    // Ǯ���� GameObject�� �������� �޼���
    public GameObject Pop()
    {
        return _pool.Get();
    }

    #region Ǯ �ݹ� �޼����
    // Ǯ���� ���ο� GameObject�� �����ؾ� �� �� ȣ��
    GameObject OnCreate()
    {
        // �������� ������� ���ο� GameObject�� ����
        GameObject go = GameObject.Instantiate(_prefab);
        // ������ ������Ʈ�� Root �Ʒ��� ��ġ
        go.transform.SetParent(Root);
        // ������Ʈ�� �̸��� �������� �̸����� ����
        go.name = _prefab.name;
        return go;
    }

    // Ǯ���� GameObject�� ������ �� ȣ��
    void OnGet(GameObject go)
    {
        // ������Ʈ�� null���� Ȯ��
        if (go is null)
        {
            return;
        }
        // ������Ʈ�� Ȱ��ȭ�Ͽ� ������ ���̵��� �Ѵ�.
        go.SetActive(true);
    }

    // GameObject�� Ǯ�� ��ȯ�� �� ȣ��
    void OnRelease(GameObject go)
    {
        // ������Ʈ�� ��Ȱ��ȭ�Ͽ� ������ ������ �ʵ��� �Ѵ�.
        go.SetActive(false);
    }

    // Ǯ���� GameObject�� �����ؾ� �� �� ȣ��
    void OnDestroy(GameObject go)
    {
        // ������Ʈ�� �����Ͽ� �޸𸮸� ����
        GameObject.Destroy(go);
    }
    #endregion
}

// ���� ������ GameObject Ǯ�� �����ϴ� �̱��� Ŭ����
public class PoolManager : Singleton<PoolManager>
{
    // ������ �̸��� �ش� Ǯ�� �����ϴ� ��ųʸ�
    Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

    /// <summary>
    /// Ư�� �����տ� �ش��ϴ� Ǯ���� GameObject�� �����´�
    /// </summary>
    /// <param name="prefab">������ ������</param>
    /// <returns>������ GameObject</returns>
    public GameObject Pop(GameObject prefab)
    {
        // �ش� �������� Ǯ�� ������ ����
        if (_pools.ContainsKey(prefab.name) == false)
            CreatePool(prefab);

        // �ش� Ǯ���� GameObject�� �����´�
        return _pools[prefab.name].Pop();
    }

    /// <summary>
    ///  GameObject�� �ش� Ǯ�� ��ȯ
    /// </summary>
    /// <param name="go">��Ȱ�� GameObject</param>
    /// <returns>��ȯ ���� ����</returns>
    public bool Push(GameObject go)
    {
        // �ش� GameObject�� Ǯ�� ������ false�� ��ȯ
        if (_pools.ContainsKey(go.name) == false)
            return false;

        // GameObject�� Ǯ�� ��ȯ
        _pools[go.name].Push(go);
        return true;
    }

    // ��� Ǯ�� ����
    public void Clear()
    {
        _pools.Clear();
    }

    // ���ο� Ǯ�� ����
    void CreatePool(GameObject original)
    {
        // �������� ������� ���ο� Ǯ�� ����
        Pool pool = new Pool(original);
        // ������ Ǯ�� ��ųʸ��� �߰�
        _pools.Add(original.name, pool);
    }
}
