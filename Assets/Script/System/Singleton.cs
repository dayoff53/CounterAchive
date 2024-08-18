using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 제네릭 싱글톤 클래스.
/// 이 클래스를 상속받으면 하나의 인스턴스만 존재하도록 보장할 수 있습니다.
/// </summary>
/// <typeparam name="T">MonoBehaviour를 상속받는 타입</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // 싱글톤 인스턴스를 저장하는 정적 변수
    private static T instance;
    // 스레드 안전성을 위한 락 객체
    private static object lockObject = new object();

    /// <summary>
    /// 싱글톤 인스턴스를 반환합니다.
    /// 인스턴스가 없으면 새로운 게임 오브젝트를 생성하여 인스턴스를 추가합니다.
    /// </summary>
    public static T Instance
    {
        get
        {
            // 인스턴스가 없으면 생성합니다.
            if (instance == null)
            {
                // 스레드 안전성을 위해 락을 사용합니다.
                lock (lockObject)
                {
                    // 씬에서 이미 존재하는 인스턴스를 찾습니다.
                    instance = FindObjectOfType<T>();

                    // 존재하지 않으면 새로운 게임 오브젝트를 생성하여 인스턴스를 추가합니다.
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
    /// 유니티의 Awake 메서드. 오브젝트가 생성될 때 호출됩니다.
    /// </summary>
    protected virtual void Awake()
    {
        // 인스턴스가 없으면 이 오브젝트를 인스턴스로 설정하고, 파괴되지 않도록 합니다.
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        // 인스턴스가 이미 존재하면 이 오브젝트를 파괴합니다.
        else
        {
            Debug.Log($"{instance.gameObject.name}");
            Debug.Log("Call : " + transform.name);
            Debug.Log("Instance : " + instance.transform.name);
            Destroy(gameObject);
        }
    }
}
