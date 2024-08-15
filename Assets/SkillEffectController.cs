using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SkillEffectController : MonoBehaviour
{
    private enum EffectType
    {
        //ù��° Ÿ���� ���ؼ� ���������� �̵��ϴ� ����
        bullet,
        //��� Ÿ���� �����Ͽ� �̵��ϴ� ����
        piercingBullet,
        //ù��° Ÿ���� ���ؼ� ���������� �̵��� ��, �߰������� ����Ʈ�� ����ϴ� ����
        boom,
        //������ �̵��� �� �����ϱ� ������, ����Ʈ�� �̵����� �ʴ� �ε� ����
        slash,
        //Ÿ���� �� ��� ���ֿ��� Ư�� ����Ʈ�� ����ϴ� ����
        area,
        //Ÿ���� �� ������Ʈ���� �߰� ������ ��� ����Ʈ�� ����ϴ� ����
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
