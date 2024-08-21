using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static object lockObject = new object();

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    instance = FindObjectOfType<T>();

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

    protected virtual void Awake()
    {
        if (instance == null)
        {
            //Debug.Log($"{instance.gameObject.name}");
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log($"{instance.gameObject.name}");
            Debug.Log("Call : " + transform.name);
            Debug.Log("Instance : " + instance.transform.name);
            Destroy(gameObject);
        }
    }
}