using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillEffect", menuName = "Skills/Null")]

public class SkillEffect : ScriptableObject
{
    protected SkillData skillData;
    protected StageManager stageManager;

    public void SkillEffectInit(SkillData skillData)
    {
        if(stageManager == null)    
        {
            stageManager = StageManager.Instance;
            this.skillData = stageManager.currentSkillData; 
        }
        else
        {
            this.skillData = skillData;
        }
    }

/// <summary>
/// 스킬의 효과를 사용합니다. (해당 함수에 스킬 기능을 넣어 발동시킵니다.)
/// </summary>
/// <param name="useUnit">사용자 유닛</param>
/// <param name="targetUnit">타겟 유닛</param>
    virtual public bool SkillEffectStart(UnitBase useUnit, UnitBase targetUnit)
    {
        if(skillData == null)
        {
            Debug.LogWarning($"스킬 데이터가 없습니다. 스킬 데이터를 초기화합니다.");
            stageManager = StageManager.Instance;
            skillData = stageManager.currentSkillData;
        }

        Debug.Log($"{useUnit.unitName}의 {targetUnit.unitName}을 대상으로 하는 {skillData.skillName}스킬 효과 발동. ");

        return true;
    }

    virtual public float ExpectedDamage(UnitBase useUnit, UnitBase targetUnit)
    {
        Debug.Log($"{skillData.skillName}의 예상 데미지는 {0}입니다.");
        return 0;
    }
}   
