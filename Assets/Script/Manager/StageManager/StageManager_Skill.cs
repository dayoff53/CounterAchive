using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class StageManager
{
    List<UnitSlotController> currentSkillTargetSlots;
    private SkillData _currentSkillData;
    public SkillData currentSkillData
    {
        get
        {
            return _currentSkillData;
        }
        set
        {
            _currentSkillData = value;

            stageMenuController.StageMenuInit();
        }
    }
    private float skillAcc = 0;
    private SkillRangeUIController skillRangeUIController;
    public StageMenuController stageMenuController;


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

            skillAcc = ((unitSlotList[currentTurnSlotNumber].unit.acc * (currentSkillData.skillAcc * 0.01f)) / unitSlotList[skillTargetNum].unit.eva) * 100f;
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

    /// <summary>
    /// PrograssState를 변경하거나, Unit의 Anim를 작동 시키는등의 스킬의 초기 구동을 진행한다.
    /// </summary>
    public void SkillStart()
    {
        if (currentSkillData)
        {
            currentSkillTargetSlots = new List<UnitSlotController>();

            foreach (int skillAreaNum in currentSkillData.skillArea)
            {
                if (currentSkillTargetSlots.IndexOf(unitSlotList[skillAreaNum]) != -1)
                {
                    currentSkillTargetSlots.Add(unitSlotList[skillAreaNum]);
                }
            }

            cost -= currentSkillData.skillCost;
            currentPrograssState = ProgressState.SkillPlay;
            unitSlotList[currentTurnSlotNumber].unit.SetAnim(1);

            targetUnitMarker.SetActive(false);
        }
    }

    /// <summary>
    /// 스킬이 종료 된 이후 해당 스킬의 요과를 순서대로 실행시킨다.
    /// </summary>
    public void SkillEndPlay()
    {
        foreach (SkillEffect skilleffect in currentSkillData.skillEffectList)
        {
            switch (skilleffect.skillEffectState)
            {
                case SkillEffectState.Damage:
                    foreach (UnitSlotController targetUnit in currentSkillTargetSlots)
                        targetUnit.unit.Damage(skilleffect.valueList[0]);
                    break;

                case SkillEffectState.StatusDown:
                    foreach(UnitSlotController targetUnit in currentSkillTargetSlots)
                        targetUnit.unit.currentHp -= skilleffect.valueList[0];
                    break;

                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 스킬의 연출을 담당하는 메서드
    /// </summary>
    public virtual void HitProduction(int hitProductionNum)
    {
        foreach (UnitSlotController targetUnit in currentSkillTargetSlots)
        {
            if (!targetUnit.isNull)
            {
                targetUnit.unit.SetAnim(2);
            }
        }
        GameObject hitProductonObject = poolManager.Pop(currentSkillData.skilIHitProductionObjects[hitProductionNum]);
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
