using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// ��ų�� ������ �к��ϴ� State
/// </summary>
[System.Serializable]
public enum SkillTypeState
{
    Attack,
    Debuff,
    Buff
}

/// <summary>
/// ��ų�� ȿ���� �к��ϴ� State
/// </summary>
[System.Serializable]
public enum SkillEffectState
{
    Damage,
    Poison,
    StatusDown
}

/// <summary>
/// ��ų�� ȿ���� ����ϴ� Ŭ����
/// </summary>
[System.Serializable]
public class SkillEffect
{
    /// <summary>
    /// ��ų�� ȿ�� ����
    /// </summary>
    public SkillEffectState skillEffectState;

    /// <summary>
    /// ��ų�� ȿ�� ���
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
    public float skillCost;
    public int[] skillRange;
    public SkillTypeState skillTypeState;
    public List<SkillEffect> skillEffectList;
    public Sprite skillIcon;
    public GameObject skilIHitProductionObject;
}
