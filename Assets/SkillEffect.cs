using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect : MonoBehaviour
{
    [SerializeField]
    protected SkillData skillData;

    [SerializeField]
    protected UnitController user;

    [SerializeField]
    protected UnitController target;

    public virtual void ApplySkillEffect(UnitController user, UnitController target)
    {
        this.user = user;
        this.target = target;

        Debug.Log($"\"{user.unitName}\"�� \"{target.unitName}\"���� \"{skillData.skillName}\" �ߵ�");
    }

    protected virtual void NormalAttack()
    {
        float skillDamage = skillData.damage;

        switch (skillData.skillTypeState)
        {
            case SkillTypeState.Attack:
                skillDamage += (user.atk);
                break;

            default:
                break;
        }

        ExecuteAttack(skillDamage);
    }


    /// <summary>
    /// �� ��ǥ���� ������ �����մϴ�.
    /// </summary>
    /// <param name="damage">������ �������Դϴ�.</param>
    public void ExecuteAttack(float damage)
    {
        target.currentHp -= damage;
    }

    /// <summary>
    /// ������ ��󿡰� �������� ������ �����մϴ�.
    /// </summary>
    /// <param name="damage">������ �������Դϴ�.</param>
    public void ExecuteAttack(UnitController attackTarget, float damage)
    {
         attackTarget.currentHp -= damage;
    }
}
