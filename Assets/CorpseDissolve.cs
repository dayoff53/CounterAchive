using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class UnitProduction : MonoBehaviour
{
    private StageManager stageManager;
    private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Material dissolveMaterial;
    public float speed = 0.5f;
    private float time = 0.0f;

    private void Start()
    {
        stageManager = StageManager.Instance;
        rigidBody = this.GetComponent<Rigidbody2D>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        CorpseInit();
        //StartCoroutine(dissolveStart());
    }

    public void CorpseInit()
    {
        spriteRenderer.sprite = null;
        spriteRenderer.material = dissolveMaterial;
        spriteRenderer.sortingOrder = (int)stageManager.unitStateColorsObject.orderLayerNumber[0];

        Material[] mats = spriteRenderer.materials;

        mats[0].SetFloat("_Cutoff", Mathf.Sin(0));

        //Unity does not allow meshRenderer.materials[0]...
        spriteRenderer.materials = mats;
    }

    public void DissolveStart(SpriteRenderer corpseSpriteRenderer)
    {
        StartCoroutine(DissolveStartCorutine(corpseSpriteRenderer));
    }

    public IEnumerator DissolveStartCorutine(SpriteRenderer corpseSpriteRenderer)
    {
        Debug.Log(gameObject.name + " : DissolveStartCorutine");
        spriteRenderer.sprite = corpseSpriteRenderer.sprite;
        spriteRenderer.flipX = corpseSpriteRenderer.flipX;
        spriteRenderer.material = dissolveMaterial;
        spriteRenderer.sortingOrder = (int)stageManager.unitStateColorsObject.orderLayerNumber[1];
        while (true)
        {
            yield return new WaitForSeconds(Time.deltaTime);

            Material[] mats = spriteRenderer.materials;

            time += Time.deltaTime;
            mats[0].SetFloat("_Cutoff", Mathf.Clamp01(time * speed));

            //Unity does not allow meshRenderer.materials[0]...
            spriteRenderer.materials = mats;

            if(mats[0].GetFloat("_Cutoff") >= 1.0f)
            {
                stageManager.isUnitDying = false;
            }

            if (time >= 1f && stageManager.isUnitDying == false)
            {
                spriteRenderer.sprite = null;
                yield break;
            }
        }
    }

    public void PushUnit(float pushForce, Vector2 direction)
    {
        rigidBody.AddForce(direction * pushForce, ForceMode2D.Impulse);
    }
}
