using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class UnitProduction : MonoBehaviour
{
    [SerializeField]
    private BattleStageMaster battleStageManager;
    private DataManager dataManager;
    private Rigidbody2D rigidBody;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Material dissolveMaterial;
    public float speed = 0.5f;
    private float time = 0.0f;

    private void Reset()
    {
        battleStageManager = FindObjectOfType<BattleStageMaster>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        dissolveMaterial = Resources.Load<Material>("Materials/Dissolve");
    }

    private void Start()
    {
        dataManager = DataManager.Instance;
        if(dataManager == null)
        {
            Debug.LogError("DataManager가 없습니다.");
        } else {
            Debug.Log("DataManager가 있습니다.");
        }
    
        rigidBody = this.GetComponent<Rigidbody2D>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        Init(null);
    }

    public void Init(BattleStageMaster stageManager)
    {
        if(dataManager == null)
        {
            Debug.LogError("DataManager가 없습니다.");
        } else {
            Debug.Log("DataManager가 있습니다.");
        }
    
        spriteRenderer.sprite = null;
        spriteRenderer.material = dissolveMaterial;
        spriteRenderer.sortingOrder = (int)dataManager.unitColorStateObject.orderLayerNumber[0];
        


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
        spriteRenderer.sortingOrder = (int)dataManager.unitColorStateObject.orderLayerNumber[1];
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
                battleStageManager.isUnitDying = false;
            }

            if (time >= 1f && battleStageManager.isUnitDying == false)
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
