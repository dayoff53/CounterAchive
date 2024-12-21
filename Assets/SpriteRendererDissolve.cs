using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRendererDissolve : MonoBehaviour
{
    private SpriteRenderer  spriteRenderer;

    [SerializeField] private Material dissolveMaterial;
    public float speed = .5f;

    private void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        //StartCoroutine(dissolveStart());
    }

    private float t = 0.0f;
    public IEnumerator dissolveStart()
    {
        spriteRenderer.material = dissolveMaterial;
        while (true)
        {
            yield return new WaitForSeconds(Time.deltaTime);

            Material[] mats = spriteRenderer.materials;

            mats[0].SetFloat("_Cutoff", Mathf.Sin(t * speed));
            t += Time.deltaTime;

            //Unity does not allow meshRenderer.materials[0]...
            spriteRenderer.materials = mats;
        }
    }

    
}
