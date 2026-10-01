using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillEffect", menuName = "Skills/Damage")]
public class Damage : SkillEffect
{   
    
    public override bool SkillEffectStart(UnitBase useUnit, UnitBase targetUnit)
    {
        targetUnit.Damage(skillValueList[0] * useUnit.atk, skillData.skillValuePierceList[0]);
        

        return true;
    }

    public override float ExpectedDamage(UnitBase useUnit, UnitBase targetUnit)
    {
        return targetUnit.ComputeDamage(skillValueList[0] * useUnit.atk, skillData.skillValuePierceList[0]);
    }
}
