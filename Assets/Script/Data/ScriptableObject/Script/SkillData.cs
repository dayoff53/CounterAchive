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
    public float acc = 100;
    public int[] skillRange;
    public SkillTypeState skillTypeState;
    public List<SkillEffect> skillEffectList;
    public Sprite skillIcon;
    public GameObject skilIHitProductionObject;
}

