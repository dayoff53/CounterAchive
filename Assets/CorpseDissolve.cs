using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CorpseDissolve : MonoBehaviour
{
    private StageManager stageManager;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Material dissolveMaterial;
    public float speed = 0.5f;
    private float time = 0.0f;

    private void Start()
    {
        stageManager = StageManager.Instance;
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        CorpseInit();
        //StartCoroutine(dissolveStart());
    }

    public void CorpseInit()
    {
        spriteRenderer.sprite = null;
        spriteRenderer.material = dissolveMaterial;

        Material[] mats = spriteRenderer.materials;

        mats[0].SetFloat("_Cutoff", Mathf.Sin(0));

        //Unity does not allow meshRenderer.materials[0]...
        spriteRenderer.materials = mats;
    }

    public void DissolveStart(Sprite corpseSprite)
    {
        StartCoroutine(DissolveStartCorutine(corpseSprite));
    }

    public IEnumerator DissolveStartCorutine(Sprite corpseSprite)
    {
        Debug.Log(gameObject.name + " : DissolveStartCorutine");
        spriteRenderer.sprite = corpseSprite;
        spriteRenderer.material = dissolveMaterial;
        while (true)
        {
            yield return new WaitForSeconds(Time.deltaTime);

            Material[] mats = spriteRenderer.materials;

            time += Time.deltaTime;
            mats[0].SetFloat("_Cutoff", Mathf.Sin(time * speed));

            //Unity does not allow meshRenderer.materials[0]...
            spriteRenderer.materials = mats;

            stageManager.isUnitDying = false;
        }
    }
}
