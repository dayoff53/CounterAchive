using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "New SkillData", menuName = "SkillData", order = 0)]
[System.Serializable]

public class SkillData : ScriptableObject
{
    public string skillName;
    public string skillFlavorText;
    public float skillCost;
    public Sprite skillIcon;
    public float minRange;
    public int[] skillRange;
    public float damage;
    public float skillEndTime;

    public float skillCoefficient = 1.0f;
}
