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


    /* UnitState를 사용하여 더이상은 필요 없는 스크립트
    /// <summary>
    /// SkillData의 복사본을 생성합니다.
    /// </summary>
    public SkillData CreateCopy()
    {
        // 새로운 SkillData 인스턴스 생성
        SkillData copy = ScriptableObject.CreateInstance<SkillData>();

        // 모든 필드 복사
        copy.skillName = this.skillName;
        copy.skillFlavorText = this.skillFlavorText;
        copy.skillState = this.skillState;
        copy.skillCost = this.skillCost;
        copy.skillIcon = this.skillIcon;
        copy.minRange = this.minRange;
        copy.skillRange = (int[])this.skillRange.Clone(); // 배열 복사
        copy.damage = this.damage;
        copy.skillEndTime = this.skillEndTime;
        copy.skillCoefficient = this.skillCoefficient;

        return copy;
    }
    */
}
