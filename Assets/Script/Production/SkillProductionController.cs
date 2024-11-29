using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillProductionController : MonoBehaviour
{
    [SerializeField]
    private float destoryTime;

    private void Start()
    {
        StartCoroutine(DistroyTimer());
    }

    IEnumerator DistroyTimer()
    {
        yield return new WaitForSeconds(destoryTime);

        Destroy(gameObject);
    }
}
