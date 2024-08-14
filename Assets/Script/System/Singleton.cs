using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���׸� �̱��� Ŭ����.
/// �� Ŭ������ ��ӹ����� �ϳ��� �ν��Ͻ��� �����ϵ��� ������ �� �ֽ��ϴ�.
/// </summary>
/// <typeparam name="T">MonoBehaviour�� ��ӹ޴� Ÿ��</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // �̱��� �ν��Ͻ��� �����ϴ� ���� ����
    private static T instance;
    // ������ �������� ���� �� ��ü
    private static object lockObject = new object();

    /// <summary>
    /// �̱��� �ν��Ͻ��� ��ȯ�մϴ�.
    /// �ν��Ͻ��� ������ ���ο� ���� ������Ʈ�� �����Ͽ� �ν��Ͻ��� �߰��մϴ�.
    /// </summary>
    public static T Instance
    {
        get
        {
            // �ν��Ͻ��� ������ �����մϴ�.
            if (instance == null)
            {
                // ������ �������� ���� ���� ����մϴ�.
                lock (lockObject)
                {
                    // ������ �̹� �����ϴ� �ν��Ͻ��� ã���ϴ�.
                    instance = FindObjectOfType<T>();

                    // �������� ������ ���ο� ���� ������Ʈ�� �����Ͽ� �ν��Ͻ��� �߰��մϴ�.
                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        instance = singletonObject.AddComponent<T>();
                    }
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// ����Ƽ�� Awake �޼���. ������Ʈ�� ������ �� ȣ��˴ϴ�.
    /// </summary>
    protected virtual void Awake()
    {
        // �ν��Ͻ��� ������ �� ������Ʈ�� �ν��Ͻ��� �����ϰ�, �ı����� �ʵ��� �մϴ�.
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        // �ν��Ͻ��� �̹� �����ϸ� �� ������Ʈ�� �ı��մϴ�.
        else
        {
            Debug.Log($"{instance.gameObject.name}");
            Debug.Log("Call : " + transform.name);
            Debug.Log("Instance : " + instance.transform.name);
            Destroy(gameObject);
        }
    }
}
