using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
[Tooltip("스킬의 타입을 나타내는 상태")]
public enum SkillTypeState
{
    Attack,
    Debuff,
    Buff
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
[Tooltip("스킬 효과를 정의하는 클래스")]
public class SkillEffect
{
    [Tooltip("스킬 효과의 상태")]
    public SkillEffectState skillEffectState;

    [Tooltip("스킬 효과의 값 리스트")]
    public List<float> valueList;
}

[System.Serializable]
[CreateAssetMenu(fileName = "New SkillData", menuName = "Datas/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    public int skillNumber;
    public string skillName;
    public string skillFlavorText;

    [Space(20)]
    [Header("스킬 속성")]
    public float skillCost = 1;
    public float skillAcc = 100;
    public int[] skillRange;

    [Space(10)]
    [Header("스킬 영역")]
    [Tooltip("스킬의 범위를 정의하는 배열")]
    public int[] skillArea;

    [Space(10)]
    [Header("스킬 타입 및 효과")]
    [Tooltip("스킬 타입 (공격, 디버프, 버프 등)")]
    public SkillTypeState skillTypeState;
    [Tooltip("스킬 효과 리스트")]
    public List<SkillEffect> skillEffectList;
    
    [Space(20)]
    [Header("히트 설정")]
    [Tooltip("히트가 여러 번 발생하는지 여부와 관련된 설정")]
    public bool isSkillHitMultiple;
    [Tooltip("isSkillHitMultiple가 true일 때 히트가 발생하는 횟수")]
    public int skillHitCount = 1;
    [Tooltip("히트 반경")]
    public float skillHitRadius = 0;

    [Space(10)]
    [Header("시각적 요소")]
    public Sprite skillIcon;
    public List<GameObject> skillHitProductionObjects;
}
