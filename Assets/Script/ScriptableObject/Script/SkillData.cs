using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public enum SkillTag
{
    Attack,
    Buff,
    Debuff
}

[CreateAssetMenu(fileName = "New SkillData", menuName = "SkillData", order = 0)]
[System.Serializable]

public class SkillData : ScriptableObject
{
    public string skillName;
    public string skillFlavorText;
    public float skillCost;
    public Sprite skillIcon;
    public SkillTag skillTag = SkillTag.Attack;
    public int damage;
    public List<int> skillRange = new List<int> { 2, 4 };
    public UnityEvent skillEvent;

    [Tooltip("사용 유닛의 이펙트 출력까지의 시간")]
    public float userSkillEffectTime;
    [Tooltip("사용 유닛의 이펙트")]
    public GameObject userSkillEffect;
}
