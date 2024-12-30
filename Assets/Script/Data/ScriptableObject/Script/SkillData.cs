using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
[Tooltip("스킬의 타입을 나타내는 상태")]
public enum SkillTargetState
{
    Enemy,
    Friend,
    All
}

[System.Serializable]
[Tooltip("스킬 효과의 상태")]
public enum SkillEffectState
{
    Damage,
    Poison,
    StatusDown
}

[System.Serializable]
[CreateAssetMenu(fileName = "New SkillData", menuName = "Datas/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    public int skillNumber;
    public string skillName;
    public string skillFlavorText;
    public Sprite skillIcon;


    [Space(20)]
    [Header("------------------- 스킬 설정 -------------------")]
    [Header("스킬 속성")]
    public List<float> skillValueList;
    public float skillCost = 1;
    public float skillAcc = 100;
    public int[] skillRange;
    public int[] skillArea;

    [Space(10)]
    [Header("스킬 타입 및 효과")]
    [Tooltip("스킬 타입 (적군, 아군, 전체 중 하나를 선택합니다.)")]
    public SkillTargetState skillTargetState;
    [Tooltip("스킬 효과 리스트 (순서대로 작동합니다.)")]
    public List<SkillEffect> skillEffectList;
    
    [Space(20)]
    [Header("------------------- 연출 설정 -------------------")]
    //[Header("히트 설정")]
    [Tooltip("히트 반경")]
    public float skillHitRadius = 0;

    [Space(10)]
    [Header("히트 연출의 랜덤 출력 여부")]
    public bool isRandomHitProduction = false;
    [Tooltip("히트 연출의 객체 리스트")]
    public List<GameObject> skillHitProductionObjects;

    [Space(10)]
    [Header("히트 연출의 순차 출력 제한 여부")]
    public bool isSkillHitCount = false;
    [Tooltip("히트 연출의 순차 출력 갯수")]
    public int skillHitCount = 1;
    
    [Space(10)]
    [Header("히트 연출의 동시 출력 제한 여부")]
    public bool isSkillHitMultiple = false;
    [Tooltip("히트 연출의 동시 출력 갯수")]
    public int skillHitMultipleCount = 1;

    public void SkillEffectStart(UnitBase useUnit, UnitBase targetUnit)
    {
        if (skillValueList.Count == 0)
        {
            Debug.LogWarning($"스킬 데이터가 없습니다. 스킬 데이터를 초기화합니다.");
            skillValueList = new List<float>();
            skillValueList.Add(1.0f);
        }
        foreach (SkillEffect skillEffect in skillEffectList)
        {
            skillEffect.SkillEffectInit(this);
            skillEffect.SkillEffectStart(useUnit, targetUnit);
        }
    }

    public float ExpectedDamage(UnitBase useUnit, UnitBase targetUnit)
    {
        float expectedDamage = 0;
        foreach (SkillEffect skillEffect in skillEffectList)
        {
            skillEffect.SkillEffectInit(this);
            expectedDamage += skillEffect.ExpectedDamage(useUnit, targetUnit);
        }
        return expectedDamage;
    }
}
