using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SkillEffectController : MonoBehaviour
{
    private enum EffectType
    {
        //첫번째 타겟을 향해서 지속적으로 이동하는 공격
        bullet,
        //모든 타켓을 관통하여 이동하는 공격
        piercingBullet,
        //첫번째 타겟을 향해서 지속적으로 이동한 후, 추가적으로 이팩트를 출력하는 공격
        boom,
        //유닛이 이동한 후 공격하기 때문에, 이펙트가 이동하지 않는 부동 공격
        slash,
        //타겟이 된 모든 유닛에게 특정 이펙트를 출력하는 공격
        area,
        //타겟이 된 오브젝트들의 중간 지점에 즉시 이펙트를 출력하는 공격
        explosion
    }
    [SerializeField]
    private EffectType effectType = EffectType.bullet;

    public TurnManager turnManager;
    public UnitSlotController userUnit;
    public List<UnitSlotController> targetUnits;
    public GameObject targetHitEffect;
    public Vector3 startingPosition;
    public int damage;
    public int speed;

    private void Start()
    {
        turnManager = TurnManager.Instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        for (int i = 0; i < targetUnits.Count; i++)
        {
            UnitSlotController targetUnit = targetUnits[i];
            if (collision.gameObject == targetUnit.gameObject)
            {
                targetUnit.Hit(damage);
                Instantiate(targetHitEffect, collision.gameObject.transform.position, Quaternion.identity);
            }
        }
    }

    public void attackSelecter()
    {
        switch(effectType)
        {
            case EffectType.bullet:
                bulletAttack();
                break;
            default:
                attackEnd();
                break;
        }
    }

    public void attackEnd()
    {
        turnManager.turnState.SetState(new FreeTurnState());
        Destroy(this.gameObject);
    }

    public void bulletAttack()
    {
        Vector3 targetPos = targetUnits[0].gameObject.transform.position;
        float duration = Vector3.Distance(transform.position, targetPos) / speed;
        transform.DOMove(targetPos, duration).SetEase(Ease.Linear);
    }
}
