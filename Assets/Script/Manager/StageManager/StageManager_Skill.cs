using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class StageManager
{
    UnitBase currentSkillUser = new UnitBase();
    UnitBase currentSkillTarget = new UnitBase();
    public SkillData currentSkillData;


    /// <summary>
    /// ��ų ������ �ʱ�ȭ�ϰ� �־��� ��ų �����ͷ� �����մϴ�.
    /// </summary>
    /// <param name="setSkillDataList">������ ��ų ������ ����Ʈ�Դϴ�.</param>
    public void SkillSlotInit(List<SkillData> setSkillDataList)
    {
        for (int i = 0; i < setSkillDataList.Count; i++)
        {
            skillSlotList[i].SetSkillData(setSkillDataList[i]);
        }
    }

    public void SkillSelect(UnitSlotController selectTargetSlot)
    {
        if (cost >= currentSkillData.skillCost)
        {
            if (skillTargetNum == unitSlotList.IndexOf(selectTargetSlot))
            {
                SkillStart();
            }

            skillTargetNum = unitSlotList.IndexOf(selectTargetSlot);
            SetCurrentUnitCardUI(false, skillTargetNum);

            targetUnitMarker.SetActive(true);
            targetUnitMarker.transform.parent = selectTargetSlot.unit.hitPosition.transform;
            targetUnitMarker.transform.position = selectTargetSlot.unit.hitPosition.transform.position;
            Debug.Log($"stageManager.skillTargetNum = {skillTargetNum}");
        }
        else
        {
            SetCurrentUnitCardUI(false, 0);
            Debug.Log("Not enough costs");
        }
    }

    public void SkillStart()
    {
        if (currentSkillData )
        {
            currentSkillUser = unitSlotList[currentTurnSlotNumber].unit;
            currentSkillTarget = unitSlotList[skillTargetNum].unit;
            currentPrograssState = ProgressState.SkillPlay;
            unitSlotList[currentTurnSlotNumber].unit.SetAnim(1);
            targetUnitMarker.SetActive(false);
            cost -= currentSkillData.skillCost;
        }
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
    /// ��ų�� ȿ�� �ߵ� ������ ����ϴ� �޼���
    /// </summary>
    public virtual void HitProduction()
    {
        GameObject hitProductonObject = poolManager.Pop(currentSkillData.skilIHitProductionObject);
        hitProductonObject.transform.position = unitSlotList[skillTargetNum].unit.hitPosition.gameObject.transform.position;
    }
}
