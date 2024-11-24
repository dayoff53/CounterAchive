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

        Debug.Log($"\"{user.unitName}\"이 \"{target.unitName}\"에게 \"{skillData.skillName}\" 발동");
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
    /// 현 목표에게 공격을 수행합니다.
    /// </summary>
    /// <param name="damage">적용할 데미지입니다.</param>
    public void ExecuteAttack(float damage)
    {
        target.currentHp -= damage;
    }

    /// <summary>
    /// 지정된 대상에게 데미지로 공격을 수행합니다.
    /// </summary>
    /// <param name="damage">적용할 데미지입니다.</param>
    public void ExecuteAttack(UnitController attackTarget, float damage)
    {
         attackTarget.currentHp -= damage;
    }
}
