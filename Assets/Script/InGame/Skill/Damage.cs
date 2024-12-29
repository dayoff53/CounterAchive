using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillEffect", menuName = "Skills/Damage")]
public class Damage : SkillEffect
{   
    [SerializeField]
    [Header("데미지 배율 (1.0f = 100%)")]
    private float damageValue = 1.0f;
    
    public override bool SkillEffectStart(UnitBase useUnit, UnitBase targetUnit)
    {
        Debug.Log($"{useUnit.unitName}의 {targetUnit.unitName}을 대상으로 하는 {skillData.skillName}스킬 효과 발동. ");
        targetUnit.Damage(damageValue * useUnit.atk);

        return true;
    }

    public override float ExpectedDamage(UnitBase useUnit, UnitBase targetUnit)
    {
        return damageValue * useUnit.atk;
    }
}
