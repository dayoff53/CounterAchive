using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRendererDissolveTest : MonoBehaviour
{
    private SpriteRenderer  spriteRenderer;

    public float speed = .5f;

    private void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    private float t = 0.0f;
    private void Update()
    {
        Material[] mats = spriteRenderer.materials;

        mats[0].SetFloat("_Cutoff", Mathf.Sin(t * speed));
        t += Time.deltaTime;

        //Unity does not allow meshRenderer.materials[0]...
        spriteRenderer.materials = mats;
    }

}
