using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillEffect", menuName = "Skills/Damage")]
public class Damage : SkillEffect
{   
    [SerializeField]
    [Header("damageValue를 고정 데미지르 줄 것인가")]
    private bool isStaticDamage = false;
    
    public override bool SkillEffectStart(UnitBase useUnit, UnitBase targetUnit)
    {
        Debug.Log($"{useUnit.unitName}의 {targetUnit.unitName}을 대상으로 하는 {skillData.skillName}스킬 효과 발동. ");
        if(isStaticDamage)
        {
            targetUnit.Damage(skillValueList[0]);
        }
        else
        {
            targetUnit.Damage(skillValueList[0] * useUnit.atk);
        }

        return true;
    }

    public override float ExpectedDamage(UnitBase useUnit, UnitBase targetUnit)
    {
        return skillValueList[0] * useUnit.atk;
    }
}
