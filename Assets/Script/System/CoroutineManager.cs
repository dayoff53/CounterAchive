using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : Singleton<CoroutineManager>
{
    public Coroutine StartManagedCoroutine(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }

    public void StopManagedCoroutine(Coroutine coroutine)
    {
        StopCoroutine(coroutine);
    }

    public void StopCoroutines()
    {
        StopAllCoroutines();
    }
}
