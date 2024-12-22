using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// 스킬의 종류를 분별하는 State
/// </summary>
[System.Serializable]
public enum SkillTypeState
{
    Attack,
    Debuff,
    Buff
}

/// <summary>
/// 스킬의 효과를 분별하는 State
/// </summary>
[System.Serializable]
public enum SkillEffectState
{
    Damage,
    Poison,
    StatusDown
}

/// <summary>
/// 스킬의 효과를 담당하는 클래스
/// </summary>
[System.Serializable]
public class SkillEffect
{
    /// <summary>
    /// 스킬의 효과 종류
    /// </summary>
    public SkillEffectState skillEffectState;

    /// <summary>
    /// 스킬의 효과 계수
    /// </summary>
    public List<float> valueList;
}

[System.Serializable]
[CreateAssetMenu(fileName = "New SkillData", menuName = "Datas/SkillData")]
public class SkillData : ScriptableObject
{
    public int skillNumber;
    public string skillName;
    public string skillFlavorText;
    public float skillCost = 1;
    public float skillAcc = 100;
    public int[] skillRange;
    /// <summary>
    /// 범위 0을 중심으로 피격 당하는 범위를 가리킨다.
    /// </summary>
    public int[] skillArea;

    //(산탄이나 특수 효과들의 경우 피격 연출 prefab에서 구현할 예정)
    public bool isSkillHitMultiple;
    public int skillHitCount;
    public float skillHitRadius = 0;

    public SkillTypeState skillTypeState;
    public List<SkillEffect> skillEffectList;
    public Sprite skillIcon;
    public List<GameObject> skillHitProductionObjects;
}

