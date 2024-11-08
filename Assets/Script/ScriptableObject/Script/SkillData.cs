using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public enum SkillState
{
    Attack,
    Debuff,
    Buff
}


[CreateAssetMenu(fileName = "New SkillData", menuName = "Datas/SkillData", order = 0)]
[System.Serializable]


public class SkillData : ScriptableObject
{
    public int skillNumber;
    public string skillName;
    public string skillFlavorText;
    public SkillState skillState;
    public float skillCost;
    public Sprite skillIcon;
    public float minRange;
    public int[] skillRange;
    public float damage;
    public float skillEndTime;


    /* UnitState�� ����Ͽ� ���̻��� �ʿ� ���� ��ũ��Ʈ
    /// <summary>
    /// SkillData�� ���纻�� �����մϴ�.
    /// </summary>
    public SkillData CreateCopy()
    {
        // ���ο� SkillData �ν��Ͻ� ����
        SkillData copy = ScriptableObject.CreateInstance<SkillData>();

        // ��� �ʵ� ����
        copy.skillName = this.skillName;
        copy.skillFlavorText = this.skillFlavorText;
        copy.skillState = this.skillState;
        copy.skillCost = this.skillCost;
        copy.skillIcon = this.skillIcon;
        copy.minRange = this.minRange;
        copy.skillRange = (int[])this.skillRange.Clone(); // �迭 ����
        copy.damage = this.damage;
        copy.skillEndTime = this.skillEndTime;
        copy.skillCoefficient = this.skillCoefficient;

        return copy;
    }
    */
}
