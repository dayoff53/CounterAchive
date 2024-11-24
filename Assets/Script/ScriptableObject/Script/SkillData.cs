using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public enum SkillTypeState
{
    Attack,
    Debuff,
    Buff
}

[System.Serializable]
[CreateAssetMenu(fileName = "New SkillData", menuName = "Datas/SkillData")]
public class SkillData : ScriptableObject
{
    public int skillNumber;
    public string skillName;
    public string skillFlavorText;
    public SkillTypeState skillTypeState;
    public float skillCost;
    public Sprite skillIcon;
    public int[] skillRange;
    public float damage;

    public float skillScale;
}

