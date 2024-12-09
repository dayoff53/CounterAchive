using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class StageManager
{
    UnitBase currentSkillUser = new UnitBase();
    UnitBase currentSkillTarget = new UnitBase();
    public SkillData currentSkillData;


    /// <summary>
    /// 스킬 슬롯을 초기화하고 주어진 스킬 데이터로 설정합니다.
    /// </summary>
    /// <param name="setSkillDataList">설정할 스킬 데이터 리스트입니다.</param>
    public void SkillSlotInit(List<SkillData> setSkillDataList)
    {
        for (int i = 0; i < setSkillDataList.Count; i++)
        {
            skillSlotList[i].SetSkillData(setSkillDataList[i]);
        }
    }


    public void SkillStart()
    {
        currentSkillUser = unitSlotList[currentTurnSlotNumber].unit;
        currentSkillTarget = unitSlotList[skillTargetNum].unit;
        currentPrograssState = ProgressState.SkillPlay;
        unitSlotList[currentTurnSlotNumber].unit.SetAnim(1);
        targetUnitMarker.SetActive(false);
        cost -= currentSkillData.skillCost;
    }

    public void SkillHitPlay()
    {
        HitProduction();

        foreach (SkillEffect skilleffect in currentSkillData.skillEffectList)
        {
            switch (skilleffect.skillEffectState)
            {
                case SkillEffectState.Damage:
                    currentSkillTarget.Damage(skilleffect.valueList[0]);
                    break;

                case SkillEffectState.StatusDown:
                    currentSkillTarget.currentHp -= skilleffect.valueList[0];
                    break;

                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 스킬의 효과 발동 연출을 담당하는 메서드
    /// </summary>
    public virtual void HitProduction()
    {
        GameObject hitProductonObject = poolManager.Pop(currentSkillData.skilIHitProductionObject);
        hitProductonObject.transform.position = unitSlotList[skillTargetNum].unit.hitPosition.gameObject.transform.position;
    }
}
