using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class StageManager
{
    UnitBase currentSkillUser = new UnitBase();
    UnitBase currentSkillTarget = new UnitBase();
    public SkillData currentSkillData;
    private float skillAcc = 0;
    private SkillRangeUIController skillRangeUIController;


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

    /// <summary>
    /// 선택된 스킬의 데이터를 적용합니다.
    /// </summary>
    /// <param name="skillData"></param>
    public void SkillTypeSelect(SkillData skillData)
    {
        if (skillData != null)
        {
            currentPrograssState = ProgressState.SkillTargetSearch;
            SetRangeGround(skillData.skillRange);
            currentSkillData = skillData;
        }
    }

    /// <summary>
    /// 스킬의 목표 데이터를 적용합니다.
    /// </summary>
    /// <param name="selectTargetSlot"></param>
    public void SkillTargetSelect(UnitSlotController selectTargetSlot)
    {
        if (cost >= currentSkillData.skillCost)
        {
            if (skillTargetNum == unitSlotList.IndexOf(selectTargetSlot))
            {
                SkillStart();
            }

            skillTargetNum = unitSlotList.IndexOf(selectTargetSlot);
            SetCurrentUnitCardUI(false, skillTargetNum);

            skillAcc = ((unitSlotList[currentTurnSlotNumber].unit.acc * (currentSkillData.acc * 0.01f)) / unitSlotList[skillTargetNum].unit.eva) * 100f;
            skillAccuracyText.text = $"{skillAcc}%";

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
    /// 스킬의 효과 발동 연출을 담당하는 메서드
    /// </summary>
    public virtual void HitProduction()
    {
        GameObject hitProductonObject = poolManager.Pop(currentSkillData.skilIHitProductionObject);
        hitProductonObject.transform.position = unitSlotList[skillTargetNum].unit.hitPosition.gameObject.transform.position;
    }


    /// <summary>
    /// 스킬 사거리만큼 groundSprite를 변경
    /// </summary>
    /// <param name="skillRange"></param>
    public void SetRangeGround(int[] skillRange)
    {
        SlotGroundSpriteController groundSprite;

        for (int i = 0; i < unitSlotList.Count; i++)
        {
            groundSprite = unitSlotList[i].slotGround;

            groundSprite.SetSlotGroundState(SlotGroundState.Default);
        }

        for (int i = 0; i < skillRange.Length; i++)
        {

            if (currentTurnSlotNumber + skillRange[i] < unitSlotList.Count)
            {
                groundSprite = unitSlotList[currentTurnSlotNumber + skillRange[i]].slotGround;

                groundSprite.SetSlotGroundState(SlotGroundState.Target);
            }

            if (currentTurnSlotNumber - skillRange[i] >= 0)
            {
                groundSprite = unitSlotList[currentTurnSlotNumber - skillRange[i]].slotGround;

                groundSprite.SetSlotGroundState(SlotGroundState.Target);
            }
        }

        groundSprite = unitSlotList[currentTurnSlotNumber].slotGround;
        groundSprite.SetSlotGroundState(SlotGroundState.Select);
    }
}
